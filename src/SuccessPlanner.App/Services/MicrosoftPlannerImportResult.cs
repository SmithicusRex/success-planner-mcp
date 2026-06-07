namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerImportResult
{
    private MicrosoftPlannerImportResult(
        MicrosoftPlannerConnectionStatus connectionStatus,
        bool wasSuccessful,
        string statusText,
        string detailText,
        IReadOnlyList<MicrosoftPlannerTaskItem>? plannerTasks = null,
        IReadOnlyList<Guid>? localTaskIds = null,
        IReadOnlyList<Guid>? sourceLinkIds = null,
        int skippedExistingCount = 0)
    {
        ConnectionStatus = connectionStatus ?? throw new ArgumentNullException(nameof(connectionStatus));
        WasSuccessful = wasSuccessful;
        StatusText = NormalizeRequired(statusText, nameof(statusText));
        DetailText = NormalizeRequired(detailText, nameof(detailText));
        PlannerTasks = plannerTasks?.ToArray() ?? [];
        LocalTaskIds = localTaskIds?.ToArray() ?? [];
        SourceLinkIds = sourceLinkIds?.ToArray() ?? [];
        SkippedExistingCount = skippedExistingCount;
    }

    public MicrosoftPlannerConnectionStatus ConnectionStatus { get; }

    public bool WasSuccessful { get; }

    public string StatusText { get; }

    public string DetailText { get; }

    public IReadOnlyList<MicrosoftPlannerTaskItem> PlannerTasks { get; }

    public IReadOnlyList<Guid> LocalTaskIds { get; }

    public IReadOnlyList<Guid> SourceLinkIds { get; }

    public int ImportedCount => LocalTaskIds.Count;

    public int SkippedExistingCount { get; }

    public static MicrosoftPlannerImportResult Success(
        MicrosoftPlannerConnectionStatus connectionStatus,
        IReadOnlyList<MicrosoftPlannerTaskItem> plannerTasks,
        IReadOnlyList<Guid> localTaskIds,
        IReadOnlyList<Guid>? sourceLinkIds = null,
        int skippedExistingCount = 0)
    {
        string statusText = BuildSuccessStatus(localTaskIds.Count, skippedExistingCount);
        string detailText = BuildSuccessDetail(localTaskIds.Count, plannerTasks.Count, skippedExistingCount);

        return new MicrosoftPlannerImportResult(
            connectionStatus,
            wasSuccessful: true,
            statusText,
            detailText,
            plannerTasks,
            localTaskIds,
            sourceLinkIds,
            skippedExistingCount);
    }

    public static MicrosoftPlannerImportResult Failed(
        MicrosoftPlannerConnectionStatus connectionStatus,
        string statusText,
        string detailText)
    {
        return new MicrosoftPlannerImportResult(
            connectionStatus,
            wasSuccessful: false,
            statusText,
            detailText);
    }

    private static string BuildSuccessStatus(int importedCount, int skippedExistingCount)
    {
        if (importedCount == 0 && skippedExistingCount > 0)
        {
            return "Planner tasks already local";
        }

        return importedCount == 0
            ? "No Planner tasks found"
            : "Planner tasks imported";
    }

    private static string BuildSuccessDetail(
        int importedCount,
        int pulledCount,
        int skippedExistingCount)
    {
        if (pulledCount == 0)
        {
            return "No assigned Planner tasks were available to import.";
        }

        List<string> parts = [];
        if (importedCount > 0)
        {
            parts.Add($"Imported {FormatPlannerTaskCount(importedCount)} as local read-only tasks.");
        }

        if (skippedExistingCount > 0)
        {
            parts.Add($"{FormatPlannerTaskCount(skippedExistingCount)} already existed locally.");
        }

        return string.Join(" ", parts);
    }

    private static string FormatPlannerTaskCount(int count)
    {
        return count == 1 ? "1 Planner task" : $"{count} Planner tasks";
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
