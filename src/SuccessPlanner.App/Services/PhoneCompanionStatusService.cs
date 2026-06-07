using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionStatusService
{
    public PhoneCompanionConnectionStatus GetInitialStatus(
        ConnectionSettings connectionSettings,
        PhoneCompanionSettings? phoneCompanionSettings = null)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings);

        if (!connectionSettings.EnablePhoneCompanion)
        {
            return PhoneCompanionConnectionStatus.Disabled();
        }

        string sharedPath = phoneCompanionSettings?.SharedCaptureFolderPath?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(sharedPath))
        {
            return PhoneCompanionConnectionStatus.NotConfigured();
        }

        if (!Directory.Exists(sharedPath))
        {
            return PhoneCompanionConnectionStatus.Unavailable(
                $"Phone companion path is unavailable: {sharedPath}");
        }

        return PhoneCompanionConnectionStatus.Ready(
            $"Reading phone capture batches from {sharedPath}.");
    }
}
