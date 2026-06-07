namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionSyncBatch
{
    public PhoneCompanionSyncBatch(
        string batchId,
        string deviceId,
        string deviceName,
        DateTimeOffset createdAt,
        IEnumerable<PhoneCompanionQuickCaptureItem>? captures = null,
        int contractVersion = PhoneCompanionSyncContract.CurrentVersion)
    {
        PhoneCompanionSyncContract.ValidateVersion(contractVersion);

        ContractVersion = contractVersion;
        BatchId = NormalizeRequired(batchId, nameof(batchId));
        DeviceId = NormalizeRequired(deviceId, nameof(deviceId));
        DeviceName = NormalizeRequired(deviceName, nameof(deviceName));
        CreatedAt = createdAt;
        Captures = NormalizeCaptures(captures);
    }

    public int ContractVersion { get; }

    public string BatchId { get; }

    public string DeviceId { get; }

    public string DeviceName { get; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyList<PhoneCompanionQuickCaptureItem> Captures { get; }

    public int CaptureCount => Captures.Count;

    public bool HasCaptures => CaptureCount > 0;

    private static IReadOnlyList<PhoneCompanionQuickCaptureItem> NormalizeCaptures(
        IEnumerable<PhoneCompanionQuickCaptureItem>? captures)
    {
        if (captures is null)
        {
            return [];
        }

        List<PhoneCompanionQuickCaptureItem> normalized = captures.ToList();
        if (normalized.Count > PhoneCompanionSyncContract.MaxBatchCaptureCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(captures),
                $"Phone sync batch can include at most {PhoneCompanionSyncContract.MaxBatchCaptureCount} captures.");
        }

        HashSet<string> clientCaptureIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (PhoneCompanionQuickCaptureItem capture in normalized)
        {
            ArgumentNullException.ThrowIfNull(capture);
            PhoneCompanionSyncContract.ValidateVersion(capture.ContractVersion);

            if (!clientCaptureIds.Add(capture.ClientCaptureId))
            {
                throw new ArgumentException(
                    $"Phone sync batch contains duplicate capture id '{capture.ClientCaptureId}'.",
                    nameof(captures));
            }
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
