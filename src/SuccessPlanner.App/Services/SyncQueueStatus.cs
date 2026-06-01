using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed record SyncQueueStatus(
    int PendingCount,
    int SyncingCount,
    int SyncedCount,
    int FailedCount,
    int ConflictCount,
    int DisabledCount)
{
    public int TotalCount => PendingCount
        + SyncingCount
        + SyncedCount
        + FailedCount
        + ConflictCount
        + DisabledCount;

    public int NeedsAttentionCount => FailedCount + ConflictCount;

    public int ActiveWorkCount => PendingCount + SyncingCount + FailedCount;

    public bool HasActiveWork => ActiveWorkCount > 0;

    public string SummaryText
    {
        get
        {
            if (NeedsAttentionCount > 0)
            {
                return FormatSummary(NeedsAttentionCount, "needs", "need", "attention");
            }

            if (SyncingCount > 0)
            {
                return FormatSummary(SyncingCount, "is", "are", "running");
            }

            if (PendingCount > 0)
            {
                return FormatSummary(PendingCount, "is", "are", "waiting");
            }

            return "Sync queue is clear.";
        }
    }

    public static SyncQueueStatus FromCounts(IReadOnlyDictionary<SyncState, int> counts)
    {
        ArgumentNullException.ThrowIfNull(counts);

        return new SyncQueueStatus(
            GetCount(counts, SyncState.Pending),
            GetCount(counts, SyncState.Syncing),
            GetCount(counts, SyncState.Synced),
            GetCount(counts, SyncState.Failed),
            GetCount(counts, SyncState.Conflict),
            GetCount(counts, SyncState.Disabled));
    }

    private static int GetCount(IReadOnlyDictionary<SyncState, int> counts, SyncState state)
    {
        return counts.TryGetValue(state, out int count) ? count : 0;
    }

    private static string FormatSummary(int count, string singularVerb, string pluralVerb, string stateText)
    {
        string noun = count == 1 ? "sync item" : "sync items";
        string verb = count == 1 ? singularVerb : pluralVerb;
        return $"{count} {noun} {verb} {stateText}.";
    }
}
