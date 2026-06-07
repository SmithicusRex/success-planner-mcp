namespace SuccessPlanner.App.Services;

public static class PhoneCompanionSyncContract
{
    public const int CurrentVersion = 1;
    public const int MaxBatchCaptureCount = 100;
    public const int MaxTitleLength = 200;
    public const int MaxNotesLength = 4000;
    public const int MaxTagCount = 12;
    public const int MaxTagLength = 40;

    public static void ValidateVersion(int contractVersion)
    {
        if (contractVersion != CurrentVersion)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contractVersion),
                $"Phone companion contract version {contractVersion} is not supported.");
        }
    }
}
