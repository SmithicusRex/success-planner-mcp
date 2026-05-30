namespace SuccessPlanner.App.Domain;

public sealed class SourceLink
{
    private SourceLink(
        Guid id,
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        string externalId,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty.", nameof(id));
        }

        ValidateLocalTarget(localItemType, localItemId);
        ValidateSourceSystem(sourceSystem);

        Id = id;
        LocalItemType = localItemType;
        LocalItemId = localItemId;
        SourceSystem = sourceSystem;
        ExternalId = NormalizeRequired(externalId, nameof(externalId));
        CreatedAt = createdAt;
        SyncState = SyncState.Pending;
    }

    public Guid Id { get; }

    public SourceLinkItemType LocalItemType { get; private set; }

    public Guid LocalItemId { get; private set; }

    public SourceSystem SourceSystem { get; private set; }

    public string ExternalId { get; private set; }

    public string ExternalContainerId { get; private set; } = string.Empty;

    public string ExternalDisplayName { get; private set; } = string.Empty;

    public string ExternalWebUrl { get; private set; } = string.Empty;

    public string SourceVersion { get; private set; } = string.Empty;

    public SyncState SyncState { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset? LastAttemptedAt { get; private set; }

    public DateTimeOffset? LastSyncedAt { get; private set; }

    public DateTimeOffset? LastFailedAt { get; private set; }

    public int RetryCount { get; private set; }

    public string FailureMessage { get; private set; } = string.Empty;

    public bool IsReadOnly { get; private set; }

    public bool CanOpenSource => !string.IsNullOrWhiteSpace(ExternalWebUrl);

    public static SourceLink Create(
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        string externalId)
    {
        return new SourceLink(Guid.NewGuid(), localItemType, localItemId, sourceSystem, externalId, DateTimeOffset.Now);
    }

    public static SourceLink Rehydrate(
        Guid id,
        SourceLinkItemType localItemType,
        Guid localItemId,
        SourceSystem sourceSystem,
        string externalId,
        DateTimeOffset createdAt,
        SyncState syncState,
        string? externalContainerId = null,
        string? externalDisplayName = null,
        string? externalWebUrl = null,
        string? sourceVersion = null,
        DateTimeOffset? lastAttemptedAt = null,
        DateTimeOffset? lastSyncedAt = null,
        DateTimeOffset? lastFailedAt = null,
        int retryCount = 0,
        string? failureMessage = null,
        bool isReadOnly = false)
    {
        ValidateSyncState(syncState);

        if (retryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount), "Retry count cannot be negative.");
        }

        SourceLink link = new(id, localItemType, localItemId, sourceSystem, externalId, createdAt)
        {
            SyncState = syncState,
            ExternalContainerId = externalContainerId?.Trim() ?? string.Empty,
            ExternalDisplayName = externalDisplayName?.Trim() ?? string.Empty,
            ExternalWebUrl = externalWebUrl?.Trim() ?? string.Empty,
            SourceVersion = sourceVersion?.Trim() ?? string.Empty,
            LastAttemptedAt = lastAttemptedAt,
            LastSyncedAt = lastSyncedAt,
            LastFailedAt = lastFailedAt,
            RetryCount = retryCount,
            FailureMessage = failureMessage?.Trim() ?? string.Empty,
            IsReadOnly = isReadOnly
        };

        return link;
    }

    public void MoveLocalTarget(SourceLinkItemType localItemType, Guid localItemId)
    {
        ValidateLocalTarget(localItemType, localItemId);

        LocalItemType = localItemType;
        LocalItemId = localItemId;
        MarkPending();
    }

    public void UpdateExternalReference(
        string externalId,
        string? externalContainerId = null,
        string? externalDisplayName = null,
        string? externalWebUrl = null,
        string? sourceVersion = null)
    {
        ExternalId = NormalizeRequired(externalId, nameof(externalId));
        ExternalContainerId = externalContainerId?.Trim() ?? ExternalContainerId;
        ExternalDisplayName = externalDisplayName?.Trim() ?? ExternalDisplayName;
        ExternalWebUrl = externalWebUrl?.Trim() ?? ExternalWebUrl;
        SourceVersion = sourceVersion?.Trim() ?? SourceVersion;
        MarkPending();
    }

    public void RenameSourceDisplay(string? externalDisplayName)
    {
        ExternalDisplayName = externalDisplayName?.Trim() ?? string.Empty;
        MarkPending();
    }

    public void MarkReadOnly()
    {
        IsReadOnly = true;
    }

    public void ClearReadOnly()
    {
        IsReadOnly = false;
    }

    public void MarkPending()
    {
        if (SyncState != SyncState.Disabled)
        {
            SyncState = SyncState.Pending;
        }
    }

    public void MarkSyncing(DateTimeOffset? attemptedAt = null)
    {
        LastAttemptedAt = attemptedAt ?? DateTimeOffset.Now;
        SyncState = SyncState.Syncing;
    }

    public void MarkSynced(string? sourceVersion = null, DateTimeOffset? syncedAt = null)
    {
        DateTimeOffset syncTime = syncedAt ?? DateTimeOffset.Now;
        LastAttemptedAt = syncTime;
        LastSyncedAt = syncTime;
        LastFailedAt = null;
        RetryCount = 0;
        FailureMessage = string.Empty;
        SourceVersion = sourceVersion?.Trim() ?? SourceVersion;
        SyncState = SyncState.Synced;
    }

    public void MarkFailed(string failureMessage, DateTimeOffset? failedAt = null)
    {
        DateTimeOffset failureTime = failedAt ?? DateTimeOffset.Now;
        LastAttemptedAt = failureTime;
        LastFailedAt = failureTime;
        RetryCount++;
        FailureMessage = NormalizeRequired(failureMessage, nameof(failureMessage));
        SyncState = SyncState.Failed;
    }

    public void MarkConflict(string failureMessage, DateTimeOffset? conflictAt = null)
    {
        DateTimeOffset conflictTime = conflictAt ?? DateTimeOffset.Now;
        LastAttemptedAt = conflictTime;
        LastFailedAt = conflictTime;
        FailureMessage = NormalizeRequired(failureMessage, nameof(failureMessage));
        SyncState = SyncState.Conflict;
    }

    public void DisableSync(string? reason = null)
    {
        SyncState = SyncState.Disabled;
        FailureMessage = reason?.Trim() ?? string.Empty;
    }

    public void EnableSync()
    {
        if (SyncState == SyncState.Disabled)
        {
            SyncState = SyncState.Pending;
        }
    }

    private static void ValidateLocalTarget(SourceLinkItemType localItemType, Guid localItemId)
    {
        if (!Enum.IsDefined(typeof(SourceLinkItemType), localItemType))
        {
            throw new ArgumentOutOfRangeException(nameof(localItemType), "Source link item type is not valid.");
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

    private static void ValidateSyncState(SyncState syncState)
    {
        if (!Enum.IsDefined(typeof(SyncState), syncState))
        {
            throw new ArgumentOutOfRangeException(nameof(syncState), "Sync state is not valid.");
        }
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
