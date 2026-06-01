namespace SuccessPlanner.App.Services;

public interface IBackgroundWorker
{
    bool IsRunning { get; }

    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
