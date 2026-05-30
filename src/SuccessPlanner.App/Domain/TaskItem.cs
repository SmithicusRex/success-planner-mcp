namespace SuccessPlanner.App.Domain;

public sealed class TaskItem
{
    private readonly List<string> _tags = [];

    private TaskItem(Guid id, string title, DateTimeOffset createdAt)
    {
        Id = id;
        Title = NormalizeRequired(title, nameof(title));
        CreatedAt = createdAt;
        Status = TaskItemStatus.Captured;
        Priority = TaskPriority.Normal;
    }

    public Guid Id { get; }

    public string Title { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public TaskItemStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public Guid? ProjectId { get; private set; }

    public int? EstimatedMinutes { get; private set; }

    public string EnergyLevel { get; private set; } = "Normal";

    public bool IsTinyStep { get; private set; }

    public bool IsPhysicalActivity { get; private set; }

    public IReadOnlyList<string> Tags => _tags;

    public static TaskItem Capture(string title)
    {
        return new TaskItem(Guid.NewGuid(), title, DateTimeOffset.Now);
    }

    public static TaskItem Rehydrate(
        Guid id,
        string title,
        DateTimeOffset createdAt,
        TaskItemStatus status,
        TaskPriority priority,
        string? notes = null,
        DateOnly? dueDate = null,
        DateOnly? startDate = null,
        DateTimeOffset? completedAt = null,
        Guid? projectId = null,
        int? estimatedMinutes = null,
        string? energyLevel = null,
        bool isTinyStep = false,
        bool isPhysicalActivity = false,
        IEnumerable<string>? tags = null)
    {
        TaskItem item = new(id, title, createdAt)
        {
            Status = status,
            Priority = priority,
            Notes = notes?.Trim() ?? string.Empty,
            DueDate = dueDate,
            StartDate = startDate,
            CompletedAt = completedAt,
            ProjectId = projectId,
            EstimatedMinutes = estimatedMinutes,
            EnergyLevel = string.IsNullOrWhiteSpace(energyLevel) ? "Normal" : energyLevel.Trim(),
            IsTinyStep = isTinyStep,
            IsPhysicalActivity = isPhysicalActivity
        };

        if (tags is not null)
        {
            foreach (string tag in tags)
            {
                item.AddTag(tag);
            }
        }

        return item;
    }

    public void Rename(string title)
    {
        Title = NormalizeRequired(title, nameof(title));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim() ?? string.Empty;
    }

    public void Schedule(DateOnly? dueDate, DateOnly? startDate = null)
    {
        DueDate = dueDate;
        StartDate = startDate;
        if (Status == TaskItemStatus.Captured)
        {
            Status = TaskItemStatus.Planned;
        }
    }

    public void AssignProject(Guid? projectId)
    {
        ProjectId = projectId;
    }

    public void SetPriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void SetEstimate(int? minutes)
    {
        if (minutes is < 1 or > 480)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes), "Estimate must be between 1 and 480 minutes.");
        }

        EstimatedMinutes = minutes;
    }

    public void SetEnergyLevel(string? energyLevel)
    {
        EnergyLevel = string.IsNullOrWhiteSpace(energyLevel) ? "Normal" : energyLevel.Trim();
    }

    public void MarkTinyStep()
    {
        IsTinyStep = true;
    }

    public void MarkPhysicalActivity()
    {
        IsPhysicalActivity = true;
        AddTag("Move");
    }

    public void Start()
    {
        if (Status != TaskItemStatus.Done)
        {
            Status = TaskItemStatus.InProgress;
        }
    }

    public void Complete(DateTimeOffset? completedAt = null)
    {
        Status = TaskItemStatus.Done;
        CompletedAt = completedAt ?? DateTimeOffset.Now;
    }

    public void Reopen()
    {
        if (Status == TaskItemStatus.Done)
        {
            Status = TaskItemStatus.Planned;
            CompletedAt = null;
        }
    }

    public void MarkBlocked()
    {
        if (Status != TaskItemStatus.Done)
        {
            Status = TaskItemStatus.Blocked;
        }
    }

    public void Snooze(DateOnly newDueDate)
    {
        DueDate = newDueDate;
        if (Status != TaskItemStatus.Done)
        {
            Status = TaskItemStatus.Planned;
        }
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

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
