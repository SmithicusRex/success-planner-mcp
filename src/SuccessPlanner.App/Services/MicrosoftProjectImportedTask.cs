namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectImportedTask
{
    public MicrosoftProjectImportedTask(
        string? externalId,
        string name,
        DateTimeOffset? startAt = null,
        DateTimeOffset? finishAt = null,
        int? percentComplete = null,
        string? notes = null)
    {
        ExternalId = NormalizeOptional(externalId);
        Name = NormalizeRequired(name, nameof(name));
        StartAt = startAt;
        FinishAt = finishAt;
        PercentComplete = NormalizePercentComplete(percentComplete);
        Notes = NormalizeOptional(notes);
    }

    public string ExternalId { get; }

    public string Name { get; }

    public DateTimeOffset? StartAt { get; }

    public DateTimeOffset? FinishAt { get; }

    public int? PercentComplete { get; }

    public string Notes { get; }

    private static int? NormalizePercentComplete(int? percentComplete)
    {
        return percentComplete.HasValue
            ? Math.Clamp(percentComplete.Value, 0, 100)
            : null;
    }

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
