namespace SuccessPlanner.App.Domain;

public sealed class FocusSession
{
    public const int DefaultPlannedMinutes = 20;

    private readonly List<string> _tags = [];

    private FocusSession(
        Guid id,
        Guid? taskId,
        string intention,
        int plannedMinutes,
        DateTimeOffset startedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        ValidateOptionalId(taskId, nameof(taskId));
        ValidatePlannedMinutes(plannedMinutes);

        Id = id;
        TaskId = taskId;
        Intention = NormalizeRequired(intention, nameof(intention));
        PlannedMinutes = plannedMinutes;
        StartedAt = startedAt;
        Status = FocusSessionStatus.InProgress;
    }

    public Guid Id { get; }

    public Guid? TaskId { get; private set; }

    public string Intention { get; private set; }

    public int PlannedMinutes { get; private set; }

    public FocusSessionStatus Status { get; private set; }

    public DateTimeOffset StartedAt { get; }

    public DateTimeOffset? PausedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public int TotalPausedMinutes { get; private set; }

    public int? ActualFocusMinutes { get; private set; }

    public string WinNote { get; private set; } = string.Empty;

    public string BlockedReason { get; private set; } = string.Empty;

    public IReadOnlyList<string> Tags => _tags;

    public static FocusSession Start(string intention, int plannedMinutes = DefaultPlannedMinutes)
    {
        return StartForTask(null, intention, plannedMinutes);
    }

    public static FocusSession StartForTask(Guid? taskId, string intention, int plannedMinutes = DefaultPlannedMinutes)
    {
        return new FocusSession(Guid.NewGuid(), taskId, intention, plannedMinutes, DateTimeOffset.Now);
    }

    public static FocusSession Rehydrate(
        Guid id,
        Guid? taskId,
        string intention,
        int plannedMinutes,
        FocusSessionStatus status,
        DateTimeOffset startedAt,
        DateTimeOffset? pausedAt = null,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? endedAt = null,
        int totalPausedMinutes = 0,
        int? actualFocusMinutes = null,
        string? winNote = null,
        string? blockedReason = null,
        IEnumerable<string>? tags = null)
    {
        if (!Enum.IsDefined(typeof(FocusSessionStatus), status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Focus session status is not valid.");
        }

        if (totalPausedMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalPausedMinutes), "Paused minutes cannot be negative.");
        }

        if (actualFocusMinutes is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(actualFocusMinutes), "Actual focus minutes cannot be negative.");
        }

        FocusSession session = new(id, taskId, intention, plannedMinutes, startedAt)
        {
            Status = status,
            PausedAt = pausedAt,
            CompletedAt = completedAt,
            EndedAt = endedAt,
            TotalPausedMinutes = totalPausedMinutes,
            ActualFocusMinutes = actualFocusMinutes,
            WinNote = winNote?.Trim() ?? string.Empty,
            BlockedReason = blockedReason?.Trim() ?? string.Empty
        };

        if (tags is not null)
        {
            foreach (string tag in tags)
            {
                session.AddTag(tag);
            }
        }

        return session;
    }

    public void AttachTask(Guid? taskId)
    {
        ValidateOptionalId(taskId, nameof(taskId));
        TaskId = taskId;
    }

    public void UpdateIntention(string intention)
    {
        Intention = NormalizeRequired(intention, nameof(intention));
    }

    public void SetPlannedMinutes(int plannedMinutes)
    {
        ValidatePlannedMinutes(plannedMinutes);
        PlannedMinutes = plannedMinutes;
    }

    public void Pause(DateTimeOffset? pausedAt = null)
    {
        if (Status != FocusSessionStatus.InProgress)
        {
            return;
        }

        DateTimeOffset pauseTime = pausedAt ?? DateTimeOffset.Now;
        ValidateNotBeforeStart(pauseTime, nameof(pausedAt));

        Status = FocusSessionStatus.Paused;
        PausedAt = pauseTime;
    }

    public void Resume(DateTimeOffset? resumedAt = null)
    {
        if (Status != FocusSessionStatus.Paused || PausedAt is null)
        {
            return;
        }

        DateTimeOffset resumeTime = resumedAt ?? DateTimeOffset.Now;
        TotalPausedMinutes += CountWholeMinutes(PausedAt.Value, resumeTime, nameof(resumedAt));
        Status = FocusSessionStatus.InProgress;
        PausedAt = null;
    }

    public void Complete(string? winNote = null, DateTimeOffset? completedAt = null)
    {
        if (Status is FocusSessionStatus.Completed or FocusSessionStatus.Cancelled)
        {
            return;
        }

        DateTimeOffset finishTime = completedAt ?? DateTimeOffset.Now;
        CloseActivePause(finishTime, nameof(completedAt));

        Status = FocusSessionStatus.Completed;
        CompletedAt = finishTime;
        EndedAt = finishTime;
        ActualFocusMinutes = GetElapsedFocusMinutes(finishTime);
        WinNote = winNote?.Trim() ?? string.Empty;
        BlockedReason = string.Empty;
        AddTag("Win");
    }

    public void MarkBlocked(string? reason = null, DateTimeOffset? blockedAt = null)
    {
        if (Status is FocusSessionStatus.Completed or FocusSessionStatus.Cancelled)
        {
            return;
        }

        DateTimeOffset finishTime = blockedAt ?? DateTimeOffset.Now;
        CloseActivePause(finishTime, nameof(blockedAt));

        Status = FocusSessionStatus.Blocked;
        EndedAt = finishTime;
        ActualFocusMinutes = GetElapsedFocusMinutes(finishTime);
        BlockedReason = reason?.Trim() ?? string.Empty;
        WinNote = string.Empty;
        AddTag("Blocked");
    }

    public void Cancel(DateTimeOffset? cancelledAt = null)
    {
        if (Status is FocusSessionStatus.Completed or FocusSessionStatus.Cancelled)
        {
            return;
        }

        DateTimeOffset finishTime = cancelledAt ?? DateTimeOffset.Now;
        CloseActivePause(finishTime, nameof(cancelledAt));

        Status = FocusSessionStatus.Cancelled;
        EndedAt = finishTime;
        ActualFocusMinutes = GetElapsedFocusMinutes(finishTime);
    }

    public int GetElapsedFocusMinutes(DateTimeOffset? asOf = null)
    {
        DateTimeOffset endTime = asOf ?? CompletedAt ?? EndedAt ?? DateTimeOffset.Now;
        ValidateNotBeforeStart(endTime, nameof(asOf));

        int pausedMinutes = TotalPausedMinutes;
        if (Status == FocusSessionStatus.Paused && PausedAt is not null)
        {
            pausedMinutes += CountWholeMinutes(PausedAt.Value, endTime, nameof(asOf));
        }

        int totalMinutes = CountWholeMinutes(StartedAt, endTime, nameof(asOf));
        return Math.Max(0, totalMinutes - pausedMinutes);
    }

    public void AddTag(string tag)
    {
        string normalized = NormalizeRequired(tag, nameof(tag));
        if (!_tags.Contains(normalized, StringComparer.OrdinalIgnoreCase))
        {
            _tags.Add(normalized);
        }
    }

    public void RemoveTag(string tag)
    {
        _tags.RemoveAll(existing => string.Equals(existing, tag, StringComparison.OrdinalIgnoreCase));
    }

    private void CloseActivePause(DateTimeOffset endTime, string parameterName)
    {
        ValidateNotBeforeStart(endTime, parameterName);

        if (Status == FocusSessionStatus.Paused && PausedAt is not null)
        {
            TotalPausedMinutes += CountWholeMinutes(PausedAt.Value, endTime, parameterName);
            PausedAt = null;
        }
    }

    private static void ValidatePlannedMinutes(int plannedMinutes)
    {
        if (plannedMinutes is < 1 or > 120)
        {
            throw new ArgumentOutOfRangeException(nameof(plannedMinutes), "Focus sessions must be between 1 and 120 minutes.");
        }
    }

    private static void ValidateOptionalId(Guid? id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", parameterName);
        }
    }

    private void ValidateNotBeforeStart(DateTimeOffset value, string parameterName)
    {
        if (value < StartedAt)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Time cannot be before the session start.");
        }
    }

    private static int CountWholeMinutes(DateTimeOffset start, DateTimeOffset end, string parameterName)
    {
        if (end < start)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Time cannot move backward.");
        }

        return Math.Max(0, (int)(end - start).TotalMinutes);
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
