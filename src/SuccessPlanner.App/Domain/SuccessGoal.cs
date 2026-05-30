namespace SuccessPlanner.App.Domain;

public sealed class SuccessGoal
{
    private readonly List<Guid> _taskIds = [];
    private readonly List<string> _tags = [];

    private SuccessGoal(Guid id, string title, string minimumWin, DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        Id = id;
        Title = NormalizeRequired(title, nameof(title));
        MinimumWin = NormalizeRequired(minimumWin, nameof(minimumWin));
        CreatedAt = createdAt;
        Status = SuccessGoalStatus.Draft;
        Priority = TaskPriority.Normal;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string WhyItMatters { get; private set; } = string.Empty;

    public string MinimumWin { get; private set; }

    public string StretchGoal { get; private set; } = string.Empty;

    public SuccessGoalStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public Guid? ProjectId { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? TargetDate { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string CompletionNote { get; private set; } = string.Empty;

    public IReadOnlyList<Guid> TaskIds => _taskIds;

    public IReadOnlyList<string> Tags => _tags;

    public bool HasStretchGoal => !string.IsNullOrWhiteSpace(StretchGoal);

    public static SuccessGoal Create(string title, string minimumWin)
    {
        return new SuccessGoal(Guid.NewGuid(), title, minimumWin, DateTimeOffset.Now);
    }

    public static SuccessGoal Rehydrate(
        Guid id,
        string title,
        string minimumWin,
        DateTimeOffset createdAt,
        SuccessGoalStatus status,
        TaskPriority priority,
        Guid? projectId = null,
        DateOnly? startDate = null,
        DateOnly? targetDate = null,
        DateTimeOffset? completedAt = null,
        string? whyItMatters = null,
        string? stretchGoal = null,
        string? completionNote = null,
        IEnumerable<Guid>? taskIds = null,
        IEnumerable<string>? tags = null)
    {
        if (!Enum.IsDefined(typeof(SuccessGoalStatus), status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), "Success goal status is not valid.");
        }

        ValidateOptionalId(projectId, nameof(projectId));

        SuccessGoal goal = new(id, title, minimumWin, createdAt)
        {
            Status = status,
            Priority = priority,
            ProjectId = projectId,
            StartDate = startDate,
            TargetDate = targetDate,
            CompletedAt = completedAt,
            WhyItMatters = whyItMatters?.Trim() ?? string.Empty,
            StretchGoal = stretchGoal?.Trim() ?? string.Empty,
            CompletionNote = completionNote?.Trim() ?? string.Empty
        };

        if (taskIds is not null)
        {
            foreach (Guid taskId in taskIds)
            {
                goal.AddTask(taskId);
            }
        }

        if (tags is not null)
        {
            foreach (string tag in tags)
            {
                goal.AddTag(tag);
            }
        }

        return goal;
    }

    public void Rename(string title)
    {
        Title = NormalizeRequired(title, nameof(title));
    }

    public void SetWhyItMatters(string? whyItMatters)
    {
        WhyItMatters = whyItMatters?.Trim() ?? string.Empty;
    }

    public void SetMinimumWin(string minimumWin)
    {
        MinimumWin = NormalizeRequired(minimumWin, nameof(minimumWin));
    }

    public void SetStretchGoal(string? stretchGoal)
    {
        StretchGoal = stretchGoal?.Trim() ?? string.Empty;
    }

    public void Schedule(DateOnly? startDate, DateOnly? targetDate)
    {
        StartDate = startDate;
        TargetDate = targetDate;

        if (Status == SuccessGoalStatus.Draft)
        {
            Status = SuccessGoalStatus.Active;
        }
    }

    public void AssignProject(Guid? projectId)
    {
        ValidateOptionalId(projectId, nameof(projectId));
        ProjectId = projectId;
    }

    public void SetPriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void Activate()
    {
        if (Status != SuccessGoalStatus.Completed)
        {
            Status = SuccessGoalStatus.Active;
        }
    }

    public void Pause()
    {
        if (Status != SuccessGoalStatus.Completed)
        {
            Status = SuccessGoalStatus.Paused;
        }
    }

    public void MarkNeedsDecision()
    {
        if (Status != SuccessGoalStatus.Completed)
        {
            Status = SuccessGoalStatus.NeedsDecision;
            AddTag("Decision");
        }
    }

    public void Complete(string? completionNote = null, DateTimeOffset? completedAt = null)
    {
        Status = SuccessGoalStatus.Completed;
        CompletedAt = completedAt ?? DateTimeOffset.Now;
        CompletionNote = completionNote?.Trim() ?? string.Empty;
        AddTag("Win");
    }

    public void Reopen()
    {
        if (Status == SuccessGoalStatus.Completed)
        {
            Status = SuccessGoalStatus.Active;
            CompletedAt = null;
            CompletionNote = string.Empty;
        }
    }

    public void Archive()
    {
        Status = SuccessGoalStatus.Archived;
    }

    public void AddTask(Guid taskId)
    {
        AddUniqueId(_taskIds, taskId);
    }

    public void RemoveTask(Guid taskId)
    {
        _taskIds.Remove(taskId);
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

    private static void AddUniqueId(List<Guid> ids, Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        if (!ids.Contains(id))
        {
            ids.Add(id);
        }
    }

    private static void ValidateOptionalId(Guid? id, string parameterName)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", parameterName);
        }
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
