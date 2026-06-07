using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionConnectionStatus
{
    private PhoneCompanionConnectionStatus(
        PhoneCompanionConnectionState state,
        bool isEnabled,
        string message)
    {
        ValidateState(state);

        State = state;
        IsEnabled = isEnabled;
        Message = NormalizeOptional(message);
    }

    public SourceSystem SourceSystem => SourceSystem.PhoneCompanion;

    public string DisplayName => "Phone Companion";

    public PhoneCompanionConnectionState State { get; }

    public bool IsEnabled { get; }

    public string Message { get; }

    public bool CanImportCaptures => IsEnabled
        && State == PhoneCompanionConnectionState.Ready;

    public bool NeedsAttention => IsEnabled
        && State is PhoneCompanionConnectionState.NotConfigured
            or PhoneCompanionConnectionState.Unavailable
            or PhoneCompanionConnectionState.Failed;

    public string StatusText => State switch
    {
        PhoneCompanionConnectionState.Disabled => "Phone companion is off",
        PhoneCompanionConnectionState.NotConfigured => "Ready to set up",
        PhoneCompanionConnectionState.Ready => "Phone companion ready",
        PhoneCompanionConnectionState.Unavailable => "Phone sync unavailable",
        PhoneCompanionConnectionState.Failed => "Phone sync failed",
        _ => "Phone companion status unknown"
    };

    public string DetailText => State switch
    {
        PhoneCompanionConnectionState.Disabled => "Phone Companion is turned off in Settings.",
        PhoneCompanionConnectionState.NotConfigured => UseMessageOrFallback(
            "Choose a phone companion sync path before phone captures can import."),
        PhoneCompanionConnectionState.Ready => UseMessageOrFallback(
            "Phone captures can import into the local inbox."),
        PhoneCompanionConnectionState.Unavailable => UseMessageOrFallback(
            "Phone companion sync is unavailable. Check the selected path and try again."),
        PhoneCompanionConnectionState.Failed => UseMessageOrFallback(
            "Phone companion sync did not finish."),
        _ => "Phone Companion status is unknown."
    };

    public static PhoneCompanionConnectionStatus Disabled()
    {
        return new PhoneCompanionConnectionStatus(
            PhoneCompanionConnectionState.Disabled,
            isEnabled: false,
            message: string.Empty);
    }

    public static PhoneCompanionConnectionStatus NotConfigured(string? message = null)
    {
        return new PhoneCompanionConnectionStatus(
            PhoneCompanionConnectionState.NotConfigured,
            isEnabled: true,
            message: message ?? string.Empty);
    }

    public static PhoneCompanionConnectionStatus Ready(string? message = null)
    {
        return new PhoneCompanionConnectionStatus(
            PhoneCompanionConnectionState.Ready,
            isEnabled: true,
            message: message ?? string.Empty);
    }

    public static PhoneCompanionConnectionStatus Unavailable(string? message = null)
    {
        return new PhoneCompanionConnectionStatus(
            PhoneCompanionConnectionState.Unavailable,
            isEnabled: true,
            message: message ?? string.Empty);
    }

    public static PhoneCompanionConnectionStatus Failed(string failureMessage)
    {
        return new PhoneCompanionConnectionStatus(
            PhoneCompanionConnectionState.Failed,
            isEnabled: true,
            message: NormalizeRequired(failureMessage, nameof(failureMessage)));
    }

    private string UseMessageOrFallback(string fallback)
    {
        return string.IsNullOrWhiteSpace(Message) ? fallback : Message;
    }

    private static void ValidateState(PhoneCompanionConnectionState state)
    {
        if (!Enum.IsDefined(typeof(PhoneCompanionConnectionState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Phone Companion connection state is not valid.");
        }
    }

    private static string NormalizeOptional(string? value)
    {
        return value?.Trim() ?? string.Empty;
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
