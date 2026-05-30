namespace SuccessPlanner.App.Domain;

public enum SyncState
{
    Pending,
    Syncing,
    Synced,
    Failed,
    Conflict,
    Disabled
}
