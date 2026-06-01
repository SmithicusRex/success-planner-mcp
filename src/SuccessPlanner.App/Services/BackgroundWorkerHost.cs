namespace SuccessPlanner.App.Services;

public sealed class BackgroundWorkerHost
{
    private readonly IReadOnlyList<IBackgroundWorker> _workers;

    public BackgroundWorkerHost(params IBackgroundWorker[] workers)
    {
        _workers = workers;
    }

    public bool IsRunning { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IsRunning)
        {
            return;
        }

        List<IBackgroundWorker> startedWorkers = [];
        try
        {
            foreach (IBackgroundWorker worker in _workers)
            {
                await worker.StartAsync(cancellationToken);
                startedWorkers.Add(worker);
            }
        }
        catch
        {
            foreach (IBackgroundWorker worker in startedWorkers.AsEnumerable().Reverse())
            {
                await worker.StopAsync(CancellationToken.None);
            }

            throw;
        }

        IsRunning = true;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!IsRunning)
        {
            return;
        }

        foreach (IBackgroundWorker worker in _workers.AsEnumerable().Reverse())
        {
            await worker.StopAsync(cancellationToken);
        }

        IsRunning = false;
    }
}
