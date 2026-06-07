namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerPullResult
{
    public MicrosoftPlannerPullResult(
        MicrosoftPlannerConnectionStatus connectionStatus,
        IReadOnlyList<MicrosoftPlannerTaskItem>? tasks = null)
    {
        ConnectionStatus = connectionStatus ?? throw new ArgumentNullException(nameof(connectionStatus));
        Tasks = tasks?.ToArray() ?? [];
    }

    public MicrosoftPlannerConnectionStatus ConnectionStatus { get; }

    public IReadOnlyList<MicrosoftPlannerTaskItem> Tasks { get; }

    public bool HasData => Tasks.Count > 0;

    public bool CanUseData => ConnectionStatus.IsAvailable;
}
