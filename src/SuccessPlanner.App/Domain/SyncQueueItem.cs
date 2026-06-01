using System.Text.Json;

namespace SuccessPlanner.App.Domain;

public sealed class SyncQueueItem
{
    private SyncQueueItem(
        Guid id,
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        SyncQueueActionType actionType,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        ValidateLocalTarget(localItemType, localItemId);
        ValidateSourceSystem(sourceSystem);
        ValidateActionType(actionType);

        Id = id;
        LocalItemType = localItemType;
        LocalItemId = localItemId;
        SourceSystem = sourceSystem;
        ActionType = actionType;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        PayloadJson = "{}";
        SyncState = SyncState.Pending;
    }

    public Guid Id { get; }

    public SourceLinkItemType LocalItemType { get; private set; }

    public Guid LocalItemId { get; private set; }

    public SourceSystem SourceSystem { get; private set; }

    public Guid? SourceLinkId { get; private set; }

    public SyncQueueActionType ActionType { get; private set; }

    public string PayloadJson { get; private set; }

    public SyncState SyncState { get; private set; }

    public int RetryCount { get; private set; }

    public DateTimeOffset? NextAttemptAt { get; private set; }

    public DateTimeOffset? LastAttemptedAt { get; private set; }

    public string FailureMessage { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public bool IsReady(DateTimeOffset now)
    {
        return (SyncState == SyncState.Pending || SyncState == SyncState.Failed)
            && (!NextAttemptAt.HasValue || NextAttemptAt.Value <= now);
    }

    public static SyncQueueItem Create(
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        SyncQueueActionType actionType,
        string? payloadJson = null,
        Guid? sourceLinkId = null,
        DateTimeOffset? createdAt = null)
    {
        DateTimeOffset now = createdAt ?? DateTimeOffset.Now;
        SyncQueueItem item = new(Guid.NewGuid(), localItemType, localItemId, sourceSystem, actionType, now, now);
        item.SetPayload(payloadJson);
        item.AttachSourceLink(sourceLinkId);
        return item;
    }

    public static SyncQueueItem Rehydrate(
        Guid id,
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        SyncQueueActionType actionType,
        string payloadJson,
        SyncState syncState,
        int retryCount,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        Guid? sourceLinkId = null,
        DateTimeOffset? nextAttemptAt = null,
        DateTimeOffset? lastAttemptedAt = null,
        string? failureMessage = null)
    {
        ValidateSyncState(syncState);
        ValidateRetryCount(retryCount);

        SyncQueueItem item = new(id, localItemType, localItemId, sourceSystem, actionType, createdAt, updatedAt)
        {
            PayloadJson = NormalizePayloadJson(payloadJson, nameof(payloadJson)),
            SyncState = syncState,
            RetryCount = retryCount,
            NextAttemptAt = nextAttemptAt,
            LastAttemptedAt = lastAttemptedAt,
            FailureMessage = failureMessage?.Trim() ?? string.Empty
        };
        item.AttachSourceLink(sourceLinkId, updatedAt);

        return item;
    }

    public void MarkPending(DateTimeOffset? nextAttemptAt = null, DateTimeOffset? updatedAt = null)
    {
        SyncState = SyncState.Pending;
        NextAttemptAt = nextAttemptAt;
        FailureMessage = string.Empty;
        UpdatedAt = updatedAt ?? DateTimeOffset.Now;
    }

    public void MarkSyncing(DateTimeOffset? attemptedAt = null)
    {
        DateTimeOffset now = attemptedAt ?? DateTimeOffset.Now;
        SyncState = SyncState.Syncing;
        LastAttemptedAt = now;
        NextAttemptAt = null;
        UpdatedAt = now;
    }

    public void MarkSynced(DateTimeOffset? syncedAt = null)
    {
        DateTimeOffset now = syncedAt ?? DateTimeOffset.Now;
        SyncState = SyncState.Synced;
        RetryCount = 0;
        LastAttemptedAt = now;
        NextAttemptAt = null;
        FailureMessage = string.Empty;
        UpdatedAt = now;
    }

    public void MarkFailed(
        string failureMessage,
        DateTimeOffset? nextAttemptAt = null,
        DateTimeOffset? failedAt = null)
    {
        DateTimeOffset now = failedAt ?? DateTimeOffset.Now;
        SyncState = SyncState.Failed;
        RetryCount++;
        LastAttemptedAt = now;
        NextAttemptAt = nextAttemptAt;
        FailureMessage = NormalizeRequired(failureMessage, nameof(failureMessage));
        UpdatedAt = now;
    }

    public void AttachSourceLink(Guid? sourceLinkId, DateTimeOffset? updatedAt = null)
    {
        if (sourceLinkId.HasValue && sourceLinkId.Value == Guid.Empty)
        {
            throw new ArgumentException("Source link id cannot be empty.", nameof(sourceLinkId));
        }

        SourceLinkId = sourceLinkId;
        UpdatedAt = updatedAt ?? UpdatedAt;
    }

    public void SetPayload(string? payloadJson, DateTimeOffset? updatedAt = null)
    {
        PayloadJson = NormalizePayloadJson(payloadJson, nameof(payloadJson));
        UpdatedAt = updatedAt ?? UpdatedAt;
    }

    private static void ValidateLocalTarget(SourceLinkItemType localItemType, Guid localItemId)
    {
        if (!Enum.IsDefined(typeof(SourceLinkItemType), localItemType))
        {
            throw new ArgumentOutOfRangeException(nameof(localItemType), "Sync queue item type is not valid.");
        }

        if (localItemId == Guid.Empty)
        {
            throw new ArgumentException("Local item id cannot be empty.", nameof(localItemId));
        }
    }

    private static void ValidateSourceSystem(SourceSystem sourceSystem)
    {
        if (!Enum.IsDefined(typeof(SourceSystem), sourceSystem))
        {
            throw new ArgumentOutOfRangeException(nameof(sourceSystem), "Source system is not valid.");
        }
    }

    private static void ValidateActionType(SyncQueueActionType actionType)
    {
        if (!Enum.IsDefined(typeof(SyncQueueActionType), actionType))
        {
            throw new ArgumentOutOfRangeException(nameof(actionType), "Sync queue action type is not valid.");
        }
    }

    private static void ValidateSyncState(SyncState syncState)
    {
        if (!Enum.IsDefined(typeof(SyncState), syncState))
        {
            throw new ArgumentOutOfRangeException(nameof(syncState), "Sync state is not valid.");
        }
    }

    private static void ValidateRetryCount(int retryCount)
    {
        if (retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Retry count cannot be negative.");
        }
    }

    private static string NormalizePayloadJson(string? value, string parameterName)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        try
        {
            using JsonDocument _ = JsonDocument.Parse(normalized);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Payload JSON must be valid JSON.", parameterName, ex);
        }

        return normalized;
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
