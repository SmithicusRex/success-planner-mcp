using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionStatusService
{
    public PhoneCompanionConnectionStatus GetInitialStatus(ConnectionSettings connectionSettings)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings);

        return connectionSettings.EnablePhoneCompanion
            ? PhoneCompanionConnectionStatus.NotConfigured()
            : PhoneCompanionConnectionStatus.Disabled();
    }
}
