namespace SuccessPlanner.App.Domain;

public sealed class MovementSession
{
    public const int DefaultPlannedMinutes = 20;

    private readonly List<string> _tags = [];

    private MovementSession(
        Guid id,
        MovementActivityType activityType,
        string activityName,
        int plannedMinutes,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        ValidateActivityType(activityType);
        ValidatePlannedMinutes(plannedMinutes);

        Id = id;
        ActivityType = activityType;
        ActivityName = NormalizeActivityName(activityName, activityType);
        PlannedMinutes = plannedMinutes;
        CreatedAt = createdAt;
        Status = MovementSessionStatus.Planned;
        AddTag("Move");
        AddTag(ActivityName);
    }

    public Guid Id { get; }

    public MovementActivityType ActivityType { get; private set; }

    public string ActivityName { get; private set; }

    public MovementSessionStatus Status { get; private set; }

    public Guid? TaskId { get; private set; }

    public int PlannedMinutes { get; private set; }

    public int? ActualMinutes { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? ScheduledFor { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? EndedAt { get; private set; }

    public string MindOccupier { get; private set; } = string.Empty;

    public bool IsWithSpouse { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public string WinNote { get; private set; } = string.Empty;

    public IReadOnlyList<string> Tags => _tags;

    public static MovementSession Schedule(
        MovementActivityType activityType,
        DateTimeOffset scheduledFor,
        int plannedMinutes = DefaultPlannedMinutes,
        string? activityName = null)
    {
        MovementSession session = new(
            Guid.NewGuid(),
            activityType,
            activityName ?? GetDefaultActivityName(activityType),
            plannedMinutes,
            DateTimeOffset.Now)
        {
            ScheduledFor = scheduledFor
        };

        return session;
    }

    public static MovementSession StartNow(
        MovementActivityType activityType,
        int plannedMinutes = DefaultPlannedMinutes,
        string? activityName = null)
    {
        MovementSession session = Schedule(activityType, DateTimeOffset.Now, plannedMinutes, activityName);
        session.Start();

        return session;
    }

    public static MovementSession Rehydrate(
        Guid id,
        MovementActivityType activityType,
        string activityName,
        int plannedMinutes,
        MovementSessionStatus status,
        DateTimeOffset createdAt,
        Guid? taskId = null,
        int? actualMinutes = null,
        DateTimeOffset? scheduledFor = null,
        DateTimeOffset? startedAt = null,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? endedAt = null,
        string? mindOccupier = null,
        bool isWithSpouse = false,
        string? notes = null,
        string? winNote = null,
        IEnumerable<string>? tags = null)
    {
        if (!Enum.IsDefined(typeof(MovementSessionStatus), status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Movement session status is not valid.");
        }

        ValidateOptionalId(taskId, nameof(taskId));
        ValidateActualMinutes(actualMinutes);

        MovementSession session = new(id, activityType, activityName, plannedMinutes, createdAt)
        {
            Status = status,
            TaskId = taskId,
            ActualMinutes = actualMinutes,
            ScheduledFor = scheduledFor,
            StartedAt = startedAt,
            CompletedAt = completedAt,
            EndedAt = endedAt,
            MindOccupier = mindOccupier?.Trim() ?? string.Empty,
            IsWithSpouse = isWithSpouse,
            Notes = notes?.Trim() ?? string.Empty,
            WinNote = winNote?.Trim() ?? string.Empty
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

    public void SetActivity(MovementActivityType activityType, string? activityName = null)
    {
        ValidateActivityType(activityType);
        ActivityType = activityType;
        ActivityName = NormalizeActivityName(activityName ?? GetDefaultActivityName(activityType), activityType);
        AddTag(ActivityName);
    }

    public void SetPlannedMinutes(int plannedMinutes)
    {
        ValidatePlannedMinutes(plannedMinutes);
        PlannedMinutes = plannedMinutes;
    }

    public void ScheduleFor(DateTimeOffset scheduledFor)
    {
        ScheduledFor = scheduledFor;
        if (Status is not MovementSessionStatus.Completed and not MovementSessionStatus.Cancelled)
        {
            Status = MovementSessionStatus.Planned;
        }
    }

    public void Start(DateTimeOffset? startedAt = null)
    {
        if (Status is MovementSessionStatus.Completed or MovementSessionStatus.Cancelled)
        {
            return;
        }

        DateTimeOffset startTime = startedAt ?? DateTimeOffset.Now;
        ValidateNotBeforeCreated(startTime, nameof(startedAt));

        StartedAt = startTime;
        Status = MovementSessionStatus.Active;
    }

    public void Complete(string? winNote = null, int? actualMinutes = null, DateTimeOffset? completedAt = null)
    {
        if (Status is MovementSessionStatus.Completed or MovementSessionStatus.Cancelled)
        {
            return;
        }

        ValidateActualMinutes(actualMinutes);

        DateTimeOffset finishTime = completedAt ?? DateTimeOffset.Now;
        ValidateNotBeforeCreated(finishTime, nameof(completedAt));

        if (StartedAt is not null && finishTime < StartedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAt), "Completion time cannot be before movement start.");
        }

        Status = MovementSessionStatus.Completed;
        CompletedAt = finishTime;
        EndedAt = finishTime;
        ActualMinutes = actualMinutes ?? CalculateActualMinutes(finishTime);
        WinNote = winNote?.Trim() ?? string.Empty;
        AddTag("Win");
    }

    public void Skip(string? note = null, DateTimeOffset? skippedAt = null)
    {
        if (Status is MovementSessionStatus.Completed or MovementSessionStatus.Cancelled)
        {
            return;
        }

        DateTimeOffset endTime = skippedAt ?? DateTimeOffset.Now;
        ValidateNotBeforeCreated(endTime, nameof(skippedAt));

        Status = MovementSessionStatus.Skipped;
        EndedAt = endTime;
        Notes = note?.Trim() ?? Notes;
    }

    public void Cancel(DateTimeOffset? cancelledAt = null)
    {
        if (Status is MovementSessionStatus.Completed or MovementSessionStatus.Cancelled)
        {
            return;
        }

        DateTimeOffset endTime = cancelledAt ?? DateTimeOffset.Now;
        ValidateNotBeforeCreated(endTime, nameof(cancelledAt));

        Status = MovementSessionStatus.Cancelled;
        EndedAt = endTime;
    }

    public void SetMindOccupier(string? mindOccupier)
    {
        MindOccupier = mindOccupier?.Trim() ?? string.Empty;
    }

    public void MarkWithSpouse()
    {
        IsWithSpouse = true;
        AddTag("With spouse");
    }

    public void ClearWithSpouse()
    {
        IsWithSpouse = false;
        RemoveTag("With spouse");
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim() ?? string.Empty;
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

    private int CalculateActualMinutes(DateTimeOffset completedAt)
    {
        if (StartedAt is null)
        {
            return PlannedMinutes;
        }

        return Math.Max(1, (int)(completedAt - StartedAt.Value).TotalMinutes);
    }

    private static void ValidateActivityType(MovementActivityType activityType)
    {
        if (!Enum.IsDefined(typeof(MovementActivityType), activityType))
        {
            throw new ArgumentOutOfRangeException(nameof(activityType), "Movement activity type is not valid.");
        }
    }

    private static void ValidatePlannedMinutes(int plannedMinutes)
    {
        if (plannedMinutes is < 1 or > 480)
        {
            throw new ArgumentOutOfRangeException(nameof(plannedMinutes), "Movement plans must be between 1 and 480 minutes.");
        }
    }

    private static void ValidateActualMinutes(int? actualMinutes)
    {
        if (actualMinutes is < 1 or > 480)
        {
            throw new ArgumentOutOfRangeException(nameof(actualMinutes), "Completed movement must be between 1 and 480 minutes.");
        }
    }

    private static void ValidateOptionalId(Guid? id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", parameterName);
        }
    }

    private void ValidateNotBeforeCreated(DateTimeOffset value, string parameterName)
    {
        if (value < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Time cannot be before the session was created.");
        }
    }

    private static string NormalizeActivityName(string value, MovementActivityType activityType)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GetDefaultActivityName(activityType);
        }

        return value.Trim();
    }

    private static string GetDefaultActivityName(MovementActivityType activityType)
    {
        return activityType switch
        {
            MovementActivityType.Walk => "Walk",
            MovementActivityType.Workout => "Workout",
            MovementActivityType.Stretch => "Stretch",
            MovementActivityType.Other => "Movement",
            _ => throw new ArgumentOutOfRangeException(nameof(activityType), "Movement activity type is not valid.")
        };
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
