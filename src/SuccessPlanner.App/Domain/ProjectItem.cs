namespace SuccessPlanner.App.Domain;

public sealed class ProjectItem
{
    private readonly List<Guid> _taskIds = [];
    private readonly List<Guid> _milestoneIds = [];
    private readonly List<string> _tags = [];

    private ProjectItem(Guid id, string name, DateTimeOffset createdAt)
    {
        Id = id;
        Name = NormalizeRequired(name, nameof(name));
        CreatedAt = createdAt;
        Status = ProjectStatus.Active;
        Priority = TaskPriority.Normal;
    }

    public Guid Id { get; }

    public string Name { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public ProjectStatus Status { get; private set; }

    public TaskPriority Priority { get; private set; }

    public DateOnly? StartDate { get; private set; }

    public DateOnly? DueDate { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string ImmediateNeed { get; private set; } = string.Empty;

    public string MinimumWin { get; private set; } = string.Empty;

    public IReadOnlyList<Guid> TaskIds => _taskIds;

    public IReadOnlyList<Guid> MilestoneIds => _milestoneIds;

    public IReadOnlyList<string> Tags => _tags;

    public static ProjectItem Create(string name)
    {
        return new ProjectItem(Guid.NewGuid(), name, DateTimeOffset.Now);
    }

    public static ProjectItem Rehydrate(
        Guid id,
        string name,
        DateTimeOffset createdAt,
        ProjectStatus status,
        TaskPriority priority)
    {
        ProjectItem item = new(id, name, createdAt)
        {
            Status = status,
            Priority = priority
        };

        return item;
    }

    public void Rename(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim() ?? string.Empty;
    }

    public void Schedule(DateOnly? startDate, DateOnly? dueDate)
    {
        StartDate = startDate;
        DueDate = dueDate;
    }

    public void SetPriority(TaskPriority priority)
    {
        Priority = priority;
    }

    public void SetImmediateNeed(string? immediateNeed)
    {
        ImmediateNeed = immediateNeed?.Trim() ?? string.Empty;
    }

    public void SetMinimumWin(string? minimumWin)
    {
        MinimumWin = minimumWin?.Trim() ?? string.Empty;
    }

    public void AddTask(Guid taskId)
    {
        AddUniqueId(_taskIds, taskId);
    }

    public void RemoveTask(Guid taskId)
    {
        _taskIds.Remove(taskId);
    }

    public void AddMilestone(Guid milestoneId)
    {
        AddUniqueId(_milestoneIds, milestoneId);
    }

    public void RemoveMilestone(Guid milestoneId)
    {
        _milestoneIds.Remove(milestoneId);
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

    public void Pause()
    {
        if (Status != ProjectStatus.Completed)
        {
            Status = ProjectStatus.Paused;
        }
    }

    public void Resume()
    {
        if (Status != ProjectStatus.Completed)
        {
            Status = ProjectStatus.Active;
        }
    }

    public void MarkBlocked()
    {
        if (Status != ProjectStatus.Completed)
        {
            Status = ProjectStatus.Blocked;
        }
    }

    public void Complete(DateTimeOffset? completedAt = null)
    {
        Status = ProjectStatus.Completed;
        CompletedAt = completedAt ?? DateTimeOffset.Now;
    }

    public void Reopen()
    {
        if (Status == ProjectStatus.Completed)
        {
            Status = ProjectStatus.Active;
            CompletedAt = null;
        }
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

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
