namespace SuccessPlanner.App.Services;

public interface IMicrosoftProjectAutomationAdapter
{
    Task<IReadOnlyList<MicrosoftProjectImportedTask>> ImportTasksAsync(
        string projectFilePath,
        CancellationToken cancellationToken = default);
}
