namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerTaskItem
{
    public MicrosoftPlannerTaskItem(
        string id,
        string title,
        string planId = "",
        string bucketId = "",
        int? percentComplete = null,
        int? priority = null,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? startAt = null,
        DateTimeOffset? dueAt = null,
        DateTimeOffset? completedAt = null,
        string webLink = "")
    {
        Id = NormalizeRequired(id, nameof(id));
        Title = NormalizeRequired(title, nameof(title));
        PlanId = NormalizeOptional(planId);
        BucketId = NormalizeOptional(bucketId);
        PercentComplete = percentComplete;
        Priority = priority;
        CreatedAt = createdAt;
        StartAt = startAt;
        DueAt = dueAt;
        CompletedAt = completedAt;
        WebLink = NormalizeOptional(webLink);
    }

    public string Id { get; }

    public string Title { get; }

    public string PlanId { get; }

    public string BucketId { get; }

    public int? PercentComplete { get; }

    public int? Priority { get; }

    public DateTimeOffset? CreatedAt { get; }

    public DateTimeOffset? StartAt { get; }

    public DateTimeOffset? DueAt { get; }

    public DateTimeOffset? CompletedAt { get; }

    public string WebLink { get; }

    public bool IsComplete => PercentComplete >= 100 || CompletedAt.HasValue;

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeOptional(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
