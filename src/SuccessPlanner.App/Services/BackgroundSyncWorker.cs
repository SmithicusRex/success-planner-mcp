using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class BackgroundSyncWorker : IBackgroundWorker
{
    public const int DefaultReadyItemLimit = 10;
    public static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(30);

    private readonly SyncService _syncService;
    private readonly Func<SyncQueueItem, CancellationToken, Task> _processItemAsync;
    private readonly SyncRetryPolicy _retryPolicy;
    private readonly TimeSpan _pollInterval;
    private readonly int _readyItemLimit;
    private readonly Func<DateTimeOffset> _nowProvider;
    private readonly SemaphoreSlim _runGate = new(1, 1);
    private CancellationTokenSource? _workerCancellation;
    private Task? _workerTask;

    public BackgroundSyncWorker(SyncService syncService)
        : this(syncService, ProcessItemWhenAdaptersExistAsync)
    {
    }

    public BackgroundSyncWorker(
        SyncService syncService,
        Func<SyncQueueItem, CancellationToken, Task> processItemAsync,
        TimeSpan? pollInterval = null,
        int readyItemLimit = DefaultReadyItemLimit,
        SyncRetryPolicy? retryPolicy = null,
        Func<DateTimeOffset>? nowProvider = null)
    {
        if (readyItemLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(readyItemLimit), "Ready item limit must be at least 1.");
        }

        TimeSpan effectivePollInterval = pollInterval ?? DefaultPollInterval;
        if (effectivePollInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(pollInterval), "Poll interval must be greater than zero.");
        }

        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));
        _processItemAsync = processItemAsync ?? throw new ArgumentNullException(nameof(processItemAsync));
        _retryPolicy = retryPolicy ?? new SyncRetryPolicy();
        _pollInterval = effectivePollInterval;
        _readyItemLimit = readyItemLimit;
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public bool IsRunning => _workerTask is { IsCompleted: false };

    public BackgroundSyncWorkerRunResult? LastRun { get; private set; }

    public string LastStatusText { get; private set; } = "Background sync worker has not run yet.";

    public string LastErrorText { get; private set; } = string.Empty;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        _workerCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _workerTask = Task.Run(() => RunLoopAsync(_workerCancellation.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_workerCancellation is null || _workerTask is null)
        {
            return;
        }

        _workerCancellation.Cancel();

        try
        {
            await _workerTask.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (_workerCancellation.IsCancellationRequested)
        {
        }
        finally
        {
            _workerCancellation.Dispose();
            _workerCancellation = null;
            _workerTask = null;
        }
    }

    public async Task<BackgroundSyncWorkerRunResult> RunOnceAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _runGate.WaitAsync(cancellationToken);

        try
        {
            DateTimeOffset startedAt = _nowProvider();
            IReadOnlyList<SyncQueueItem> readyItems =
                await _syncService.GetReadyItemsAsync(_readyItemLimit, cancellationToken);

            int processedCount = 0;
            int failedCount = 0;
            LastErrorText = string.Empty;

            foreach (SyncQueueItem item in readyItems)
            {
                cancellationToken.ThrowIfCancellationRequested();
                DateTimeOffset attemptedAt = _nowProvider();
                SyncQueueItem? syncingItem =
                    await _syncService.MarkSyncingAsync(item.Id, attemptedAt, cancellationToken);
                if (syncingItem is null)
                {
                    continue;
                }

                try
                {
                    await _processItemAsync(syncingItem, cancellationToken);
                    await _syncService.MarkSyncedAsync(syncingItem.Id, _nowProvider(), cancellationToken);
                    processedCount++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    DateTimeOffset failedAt = _nowProvider();
                    int retryCountAfterFailure = syncingItem.RetryCount + 1;
                    DateTimeOffset nextAttemptAt =
                        _retryPolicy.GetNextAttemptAt(failedAt, retryCountAfterFailure);
                    string failureMessage = BuildFailureMessage(ex);
                    await _syncService.MarkFailedAsync(
                        syncingItem.Id,
                        failureMessage,
                        nextAttemptAt,
                        failedAt,
                        cancellationToken);
                    failedCount++;
                    LastErrorText = failureMessage;
                }
            }

            BackgroundSyncWorkerRunResult result = new(
                readyItems.Count,
                processedCount,
                failedCount,
                startedAt,
                _nowProvider());
            LastRun = result;
            LastStatusText = result.StatusText;
            return result;
        }
        finally
        {
            _runGate.Release();
        }
    }

    private async Task RunLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                LastErrorText = ex.Message;
                LastStatusText = "Background sync worker needs attention.";
            }

            try
            {
                await Task.Delay(_pollInterval, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private static Task ProcessItemWhenAdaptersExistAsync(
        SyncQueueItem item,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        throw new InvalidOperationException("Sync adapter is not connected yet.");
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
    }
}
