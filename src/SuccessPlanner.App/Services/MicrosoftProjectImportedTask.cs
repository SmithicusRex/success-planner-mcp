namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectImportedTask
{
    public MicrosoftProjectImportedTask(
        string? externalId,
        string name,
        DateTimeOffset? startAt = null,
        DateTimeOffset? finishAt = null,
        int? percentComplete = null,
        string? notes = null,
        int? durationMinutes = null,
        int? outlineLevel = null,
        bool isSummary = false,
        bool isMilestone = false,
        bool isCritical = false,
        int? projectPriority = null)
    {
        ExternalId = NormalizeOptional(externalId);
        Name = NormalizeRequired(name, nameof(name));
        StartAt = startAt;
        FinishAt = finishAt;
        PercentComplete = NormalizePercentComplete(percentComplete);
        Notes = NormalizeOptional(notes);
        DurationMinutes = NormalizeNonNegative(durationMinutes);
        OutlineLevel = outlineLevel is > 0 ? outlineLevel : null;
        IsSummary = isSummary;
        IsMilestone = isMilestone;
        IsCritical = isCritical;
        ProjectPriority = NormalizeProjectPriority(projectPriority);
    }

    public string ExternalId { get; }

    public string Name { get; }

    public DateTimeOffset? StartAt { get; }

    public DateTimeOffset? FinishAt { get; }

    public int? PercentComplete { get; }

    public string Notes { get; }

    public int? DurationMinutes { get; }

    public int? OutlineLevel { get; }

    public bool IsSummary { get; }

    public bool IsMilestone { get; }

    public bool IsCritical { get; }

    public int? ProjectPriority { get; }

    public bool IsComplete => PercentComplete >= 100;

    private static int? NormalizePercentComplete(int? percentComplete)
    {
        return percentComplete.HasValue
            ? Math.Clamp(percentComplete.Value, 0, 100)
            : null;
    }

    private static int? NormalizeNonNegative(int? value)
    {
        return value.HasValue ? Math.Max(0, value.Value) : null;
    }

    private static int? NormalizeProjectPriority(int? projectPriority)
    {
        return projectPriority.HasValue
            ? Math.Clamp(projectPriority.Value, 0, 1000)
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
