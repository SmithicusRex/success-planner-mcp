namespace SuccessPlanner.App.Services;

public sealed record BackgroundSyncWorkerRunResult(
    int ReadyItemCount,
    int ProcessedItemCount,
    int FailedItemCount,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt)
{
    public string StatusText
    {
        get
        {
            if (FailedItemCount > 0)
            {
                return FormatSummary(
                    FailedItemCount,
                    "ready item failed in the background worker",
                    "ready items failed in the background worker");
            }

            if (ProcessedItemCount > 0)
            {
                return FormatSummary(
                    ProcessedItemCount,
                    "ready item was synced by the background worker",
                    "ready items were synced by the background worker");
            }

            return "Background sync worker found no ready items.";
        }
    }

    private static string FormatSummary(int count, string singularText, string pluralText)
    {
        string itemText = count == 1 ? singularText : pluralText;
        return $"{count} {itemText}.";
    }
}
