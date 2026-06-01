namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoPullResult
{
    public MicrosoftToDoPullResult(
        MicrosoftToDoConnectionStatus connectionStatus,
        IReadOnlyList<MicrosoftToDoTaskList>? taskLists = null,
        IReadOnlyList<MicrosoftToDoTaskItem>? tasks = null)
    {
        ConnectionStatus = connectionStatus ?? throw new ArgumentNullException(nameof(connectionStatus));
        TaskLists = taskLists ?? [];
        Tasks = tasks ?? [];
    }

    public MicrosoftToDoConnectionStatus ConnectionStatus { get; }

    public IReadOnlyList<MicrosoftToDoTaskList> TaskLists { get; }

    public IReadOnlyList<MicrosoftToDoTaskItem> Tasks { get; }

    public bool HasData => TaskLists.Count > 0 || Tasks.Count > 0;

    public bool CanUseData => ConnectionStatus.IsConnected;
}
