namespace SuccessPlanner.App.Domain;

public sealed class MilestoneItem
{
    private readonly List<Guid> _taskIds = [];
    private readonly List<string> _tags = [];

    private MilestoneItem(Guid id, Guid projectId, string name, DateTimeOffset createdAt)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id cannot be empty.", nameof(projectId));
        }

        Id = id;
        ProjectId = projectId;
        Name = NormalizeRequired(name, nameof(name));
        CreatedAt = createdAt;
        Status = MilestoneStatus.Upcoming;
    }

    public Guid Id { get; }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; }

    public string Notes { get; private set; } = string.Empty;

    public MilestoneStatus Status { get; private set; }

    public DateOnly? TargetDate { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string MinimumWin { get; private set; } = string.Empty;

    public bool IsReviewMarker { get; private set; }

    public IReadOnlyList<Guid> TaskIds => _taskIds;

    public IReadOnlyList<string> Tags => _tags;

    public static MilestoneItem Create(Guid projectId, string name)
    {
        return new MilestoneItem(Guid.NewGuid(), projectId, name, DateTimeOffset.Now);
    }

    public static MilestoneItem Rehydrate(
        Guid id,
        Guid projectId,
        string name,
        DateTimeOffset createdAt,
        MilestoneStatus status)
    {
        MilestoneItem item = new(id, projectId, name, createdAt)
        {
            Status = status
        };

        return item;
    }

    public void Rename(string name)
    {
        Name = NormalizeRequired(name, nameof(name));
    }

    public void MoveToProject(Guid projectId)
    {
        if (projectId == Guid.Empty)
        {
            throw new ArgumentException("Project id cannot be empty.", nameof(projectId));
        }

        ProjectId = projectId;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim() ?? string.Empty;
    }

    public void Schedule(DateOnly? targetDate)
    {
        TargetDate = targetDate;
    }

    public void SetMinimumWin(string? minimumWin)
    {
        MinimumWin = minimumWin?.Trim() ?? string.Empty;
    }

    public void MarkReviewMarker()
    {
        IsReviewMarker = true;
        AddTag("Review");
    }

    public void ClearReviewMarker()
    {
        IsReviewMarker = false;
        RemoveTag("Review");
    }

    public void AddTask(Guid taskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException("Task id cannot be empty.", nameof(taskId));
        }

        if (!_taskIds.Contains(taskId))
        {
            _taskIds.Add(taskId);
        }
    }

    public void RemoveTask(Guid taskId)
    {
        _taskIds.Remove(taskId);
    }

    public void MarkAtRisk()
    {
        if (Status != MilestoneStatus.Completed)
        {
            Status = MilestoneStatus.AtRisk;
        }
    }

    public void MarkBlocked()
    {
        if (Status != MilestoneStatus.Completed)
        {
            Status = MilestoneStatus.Blocked;
        }
    }

    public void Resume()
    {
        if (Status != MilestoneStatus.Completed)
        {
            Status = MilestoneStatus.Upcoming;
        }
    }

    public void Complete(DateTimeOffset? completedAt = null)
    {
        Status = MilestoneStatus.Completed;
        CompletedAt = completedAt ?? DateTimeOffset.Now;
    }

    public void Reopen()
    {
        if (Status == MilestoneStatus.Completed)
        {
            Status = MilestoneStatus.Upcoming;
            CompletedAt = null;
        }
    }

    public void Archive()
    {
        Status = MilestoneStatus.Archived;
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
