namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoTaskItem
{
    public MicrosoftToDoTaskItem(
        string id,
        string listId,
        string title,
        string status,
        string importance = "",
        string bodyContent = "",
        DateTimeOffset? dueAt = null,
        DateTimeOffset? completedAt = null,
        DateTimeOffset? lastModifiedAt = null,
        string webLink = "")
    {
        Id = NormalizeRequired(id, nameof(id));
        ListId = NormalizeRequired(listId, nameof(listId));
        Title = NormalizeRequired(title, nameof(title));
        Status = NormalizeOptional(status);
        Importance = NormalizeOptional(importance);
        BodyContent = NormalizeOptional(bodyContent);
        DueAt = dueAt;
        CompletedAt = completedAt;
        LastModifiedAt = lastModifiedAt;
        WebLink = NormalizeOptional(webLink);
    }

    public string Id { get; }

    public string ListId { get; }

    public string Title { get; }

    public string Status { get; }

    public string Importance { get; }

    public string BodyContent { get; }

    public DateTimeOffset? DueAt { get; }

    public DateTimeOffset? CompletedAt { get; }

    public DateTimeOffset? LastModifiedAt { get; }

    public string WebLink { get; }

    public bool IsCompleted => string.Equals(Status, "completed", StringComparison.OrdinalIgnoreCase);

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
