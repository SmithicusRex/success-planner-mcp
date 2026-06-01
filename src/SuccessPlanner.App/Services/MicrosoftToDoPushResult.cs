using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoPushResult
{
    public MicrosoftToDoPushResult(
        MicrosoftToDoConnectionStatus connectionStatus,
        MicrosoftToDoTaskItem? pushedTask = null,
        SourceLink? sourceLink = null)
    {
        ConnectionStatus = connectionStatus ?? throw new ArgumentNullException(nameof(connectionStatus));
        PushedTask = pushedTask;
        SourceLink = sourceLink;
    }

    public MicrosoftToDoConnectionStatus ConnectionStatus { get; }

    public MicrosoftToDoTaskItem? PushedTask { get; }

    public SourceLink? SourceLink { get; }

    public bool WasPushed => ConnectionStatus.IsConnected
        && PushedTask is not null
        && SourceLink is not null;
}
