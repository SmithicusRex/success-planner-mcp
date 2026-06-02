namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectImportResult
{
    private MicrosoftProjectImportResult(
        string projectFilePath,
        bool wasSuccessful,
        string statusText,
        string detailText,
        IReadOnlyList<MicrosoftProjectImportedTask>? importedTasks = null,
        IReadOnlyList<Guid>? localTaskIds = null,
        IReadOnlyList<Guid>? sourceLinkIds = null)
    {
        ProjectFilePath = projectFilePath.Trim();
        WasSuccessful = wasSuccessful;
        StatusText = statusText.Trim();
        DetailText = detailText.Trim();
        ImportedTasks = importedTasks?.ToArray() ?? [];
        LocalTaskIds = localTaskIds?.ToArray() ?? [];
        SourceLinkIds = sourceLinkIds?.ToArray() ?? [];
    }

    public string ProjectFilePath { get; }

    public bool WasSuccessful { get; }

    public string StatusText { get; }

    public string DetailText { get; }

    public IReadOnlyList<MicrosoftProjectImportedTask> ImportedTasks { get; }

    public IReadOnlyList<Guid> LocalTaskIds { get; }

    public IReadOnlyList<Guid> SourceLinkIds { get; }

    public int ImportedCount => LocalTaskIds.Count;

    public static MicrosoftProjectImportResult Success(
        string projectFilePath,
        IReadOnlyList<MicrosoftProjectImportedTask> importedTasks,
        IReadOnlyList<Guid> localTaskIds,
        IReadOnlyList<Guid>? sourceLinkIds = null)
    {
        string fileName = Path.GetFileName(projectFilePath);
        string statusText = localTaskIds.Count == 0
            ? "No Project tasks found"
            : "Project tasks imported";
        string detailText = localTaskIds.Count == 0
            ? $"No importable tasks were found in {fileName}."
            : $"Imported {FormatCount(localTaskIds.Count)} from {fileName}.";

        return new MicrosoftProjectImportResult(
            projectFilePath,
            wasSuccessful: true,
            statusText,
            detailText,
            importedTasks,
            localTaskIds,
            sourceLinkIds);
    }

    public static MicrosoftProjectImportResult Failed(
        string projectFilePath,
        string statusText,
        string detailText)
    {
        return new MicrosoftProjectImportResult(
            projectFilePath,
            wasSuccessful: false,
            statusText,
            detailText);
    }

    private static string FormatCount(int count)
    {
        return count == 1 ? "1 Project task" : $"{count} Project tasks";
    }
}
