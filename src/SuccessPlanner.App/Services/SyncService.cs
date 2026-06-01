using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class SyncService
{
    private readonly SyncQueueRepository _queueRepository;
    private readonly Func<DateTimeOffset> _nowProvider;

    public SyncService(SyncQueueRepository queueRepository)
        : this(queueRepository, () => DateTimeOffset.Now)
    {
    }

    public SyncService(SyncQueueRepository queueRepository, Func<DateTimeOffset> nowProvider)
    {
        _queueRepository = queueRepository ?? throw new ArgumentNullException(nameof(queueRepository));
        _nowProvider = nowProvider ?? throw new ArgumentNullException(nameof(nowProvider));
    }

    public Task<SyncQueueItem> QueueCreateAsync(
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        string? payloadJson = null,
        Guid? sourceLinkId = null,
        CancellationToken cancellationToken = default)
    {
        return QueueLocalChangeAsync(
            localItemType,
            localItemId,
            sourceSystem,
            SyncQueueActionType.Create,
            payloadJson,
            sourceLinkId,
            cancellationToken);
    }

    public Task<SyncQueueItem> QueueUpdateAsync(
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        string? payloadJson = null,
        Guid? sourceLinkId = null,
        CancellationToken cancellationToken = default)
    {
        return QueueLocalChangeAsync(
            localItemType,
            localItemId,
            sourceSystem,
            SyncQueueActionType.Update,
            payloadJson,
            sourceLinkId,
            cancellationToken);
    }

    public Task<SyncQueueItem> QueueDeleteAsync(
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        string? payloadJson = null,
        Guid? sourceLinkId = null,
        CancellationToken cancellationToken = default)
    {
        return QueueLocalChangeAsync(
            localItemType,
            localItemId,
            sourceSystem,
            SyncQueueActionType.Delete,
            payloadJson,
            sourceLinkId,
            cancellationToken);
    }

    public async Task<SyncQueueItem> QueueLocalChangeAsync(
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        SyncQueueActionType actionType,
        string? payloadJson = null,
        Guid? sourceLinkId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SyncQueueItem item = SyncQueueItem.Create(
            localItemType,
            localItemId,
            sourceSystem,
            actionType,
            payloadJson,
            sourceLinkId,
            _nowProvider());

        await _queueRepository.EnqueueAsync(item, cancellationToken);
        return item;
    }

    public Task<IReadOnlyList<SyncQueueItem>> GetReadyItemsAsync(
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        return _queueRepository.GetReadyAsync(_nowProvider(), limit, cancellationToken);
    }

    public async Task<SyncQueueStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyDictionary<SyncState, int> counts =
            await _queueRepository.CountByStateAsync(cancellationToken);

        return SyncQueueStatus.FromCounts(counts);
    }

    public async Task<SyncQueueItem?> MarkSyncingAsync(
        Guid queueItemId,
        DateTimeOffset? attemptedAt = null,
        CancellationToken cancellationToken = default)
    {
        SyncQueueItem? item = await _queueRepository.GetByIdAsync(queueItemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.MarkSyncing(attemptedAt ?? _nowProvider());
        await _queueRepository.SaveAsync(item, cancellationToken);
        return item;
    }

    public async Task<SyncQueueItem?> MarkSyncedAsync(
        Guid queueItemId,
        DateTimeOffset? syncedAt = null,
        CancellationToken cancellationToken = default)
    {
        SyncQueueItem? item = await _queueRepository.GetByIdAsync(queueItemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.MarkSynced(syncedAt ?? _nowProvider());
        await _queueRepository.SaveAsync(item, cancellationToken);
        return item;
    }

    public async Task<SyncQueueItem?> MarkFailedAsync(
        Guid queueItemId,
        string failureMessage,
        DateTimeOffset? nextAttemptAt = null,
        DateTimeOffset? failedAt = null,
        CancellationToken cancellationToken = default)
    {
        SyncQueueItem? item = await _queueRepository.GetByIdAsync(queueItemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.MarkFailed(failureMessage, nextAttemptAt, failedAt ?? _nowProvider());
        await _queueRepository.SaveAsync(item, cancellationToken);
        return item;
    }
}
