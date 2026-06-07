namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionSyncResult
{
    private PhoneCompanionSyncResult(
        PhoneCompanionSyncResultState state,
        string statusText,
        string detailText,
        IReadOnlyList<PhoneCompanionCaptureImportOutcome>? outcomes = null)
    {
        ValidateState(state);

        State = state;
        StatusText = NormalizeRequired(statusText, nameof(statusText));
        DetailText = NormalizeRequired(detailText, nameof(detailText));
        Outcomes = outcomes?.ToArray() ?? [];
    }

    public PhoneCompanionSyncResultState State { get; }

    public string StatusText { get; }

    public string DetailText { get; }

    public IReadOnlyList<PhoneCompanionCaptureImportOutcome> Outcomes { get; }

    public int ImportedCount => Outcomes.Count(outcome => outcome.State == PhoneCompanionCaptureImportState.Imported);

    public int SkippedCount => Outcomes.Count(outcome => outcome.State == PhoneCompanionCaptureImportState.Skipped);

    public int RejectedCount => Outcomes.Count(outcome => outcome.State == PhoneCompanionCaptureImportState.Rejected);

    public bool WasSuccessful => State is PhoneCompanionSyncResultState.Accepted or PhoneCompanionSyncResultState.Empty;

    public bool NeedsAttention => State is PhoneCompanionSyncResultState.Partial or PhoneCompanionSyncResultState.Rejected;

    public static PhoneCompanionSyncResult Accepted(
        IReadOnlyList<PhoneCompanionCaptureImportOutcome> outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        int importedCount = outcomes.Count(outcome => outcome.State == PhoneCompanionCaptureImportState.Imported);
        int skippedCount = outcomes.Count(outcome => outcome.State == PhoneCompanionCaptureImportState.Skipped);
        string detailText = skippedCount == 0
            ? $"Imported {FormatCaptureCount(importedCount)} from phone."
            : $"Imported {FormatCaptureCount(importedCount)} from phone. {FormatCaptureCount(skippedCount)} already existed.";

        return new PhoneCompanionSyncResult(
            PhoneCompanionSyncResultState.Accepted,
            importedCount == 0 ? "No new phone captures" : "Phone captures imported",
            detailText,
            outcomes);
    }

    public static PhoneCompanionSyncResult Empty()
    {
        return new PhoneCompanionSyncResult(
            PhoneCompanionSyncResultState.Empty,
            "No phone captures found",
            "The phone companion did not have new captures to import.");
    }

    public static PhoneCompanionSyncResult Partial(
        IReadOnlyList<PhoneCompanionCaptureImportOutcome> outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcomes);

        return new PhoneCompanionSyncResult(
            PhoneCompanionSyncResultState.Partial,
            "Some phone captures need attention",
            $"Imported {FormatCaptureCount(outcomes.Count(outcome => outcome.WasImported))}. {FormatCaptureCount(outcomes.Count(outcome => !outcome.WasImported))} need review.",
            outcomes);
    }

    public static PhoneCompanionSyncResult Rejected(string detailText)
    {
        return new PhoneCompanionSyncResult(
            PhoneCompanionSyncResultState.Rejected,
            "Phone sync unavailable",
            detailText);
    }

    private static void ValidateState(PhoneCompanionSyncResultState state)
    {
        if (!Enum.IsDefined(typeof(PhoneCompanionSyncResultState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Phone companion sync result state is not valid.");
        }
    }

    private static string FormatCaptureCount(int count)
    {
        return count == 1 ? "1 capture" : $"{count} captures";
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
