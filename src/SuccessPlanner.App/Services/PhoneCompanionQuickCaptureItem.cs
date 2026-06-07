namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionQuickCaptureItem
{
    public PhoneCompanionQuickCaptureItem(
        string clientCaptureId,
        string title,
        DateTimeOffset capturedAt,
        string? notes = null,
        DateOnly? dueDate = null,
        PhoneCompanionCaptureDestination destination = PhoneCompanionCaptureDestination.LetSuccessPlannerChoose,
        IEnumerable<string>? tags = null,
        int contractVersion = PhoneCompanionSyncContract.CurrentVersion)
    {
        PhoneCompanionSyncContract.ValidateVersion(contractVersion);
        ValidateDestination(destination);

        ContractVersion = contractVersion;
        ClientCaptureId = NormalizeRequired(clientCaptureId, nameof(clientCaptureId));
        Title = NormalizeLimitedRequired(
            title,
            nameof(title),
            PhoneCompanionSyncContract.MaxTitleLength);
        Notes = NormalizeLimitedOptional(
            notes,
            nameof(notes),
            PhoneCompanionSyncContract.MaxNotesLength);
        CapturedAt = capturedAt;
        DueDate = dueDate;
        Destination = destination;
        Tags = NormalizeTags(tags);
    }

    public int ContractVersion { get; }

    public string ClientCaptureId { get; }

    public string Title { get; }

    public string Notes { get; }

    public DateTimeOffset CapturedAt { get; }

    public DateOnly? DueDate { get; }

    public PhoneCompanionCaptureDestination Destination { get; }

    public IReadOnlyList<string> Tags { get; }

    public bool HasNotes => !string.IsNullOrWhiteSpace(Notes);

    public bool HasDueDate => DueDate.HasValue;

    public bool HasTags => Tags.Count > 0;

    private static void ValidateDestination(PhoneCompanionCaptureDestination destination)
    {
        if (!Enum.IsDefined(typeof(PhoneCompanionCaptureDestination), destination))
        {
            throw new ArgumentOutOfRangeException(nameof(destination), "Phone capture destination is not valid.");
        }
    }

    private static IReadOnlyList<string> NormalizeTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return [];
        }

        List<string> normalizedTags = [];
        foreach (string tag in tags)
        {
            string normalized = NormalizeLimitedOptional(
                tag,
                nameof(tags),
                PhoneCompanionSyncContract.MaxTagLength);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                continue;
            }

            if (!normalizedTags.Contains(normalized, StringComparer.OrdinalIgnoreCase))
            {
                normalizedTags.Add(normalized);
            }
        }

        if (normalizedTags.Count > PhoneCompanionSyncContract.MaxTagCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tags),
                $"Phone capture can include at most {PhoneCompanionSyncContract.MaxTagCount} tags.");
        }

        return normalizedTags;
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeLimitedRequired(
        string value,
        string parameterName,
        int maxLength)
    {
        string normalized = NormalizeRequired(value, parameterName);
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string NormalizeLimitedOptional(
        string? value,
        string parameterName,
        int maxLength)
    {
        string normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
