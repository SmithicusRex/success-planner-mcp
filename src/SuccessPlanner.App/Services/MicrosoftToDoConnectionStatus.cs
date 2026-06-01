using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoConnectionStatus
{
    private MicrosoftToDoConnectionStatus(
        MicrosoftToDoConnectionState state,
        bool isEnabled,
        string accountDisplayName,
        DateTimeOffset? lastCheckedAt,
        string message)
    {
        ValidateState(state);

        State = state;
        IsEnabled = isEnabled;
        AccountDisplayName = NormalizeOptional(accountDisplayName);
        LastCheckedAt = lastCheckedAt;
        Message = NormalizeOptional(message);
    }

    public SourceSystem SourceSystem => SourceSystem.MicrosoftToDo;

    public string DisplayName => "Microsoft To Do";

    public MicrosoftToDoConnectionState State { get; }

    public bool IsEnabled { get; }

    public string AccountDisplayName { get; }

    public DateTimeOffset? LastCheckedAt { get; }

    public string Message { get; }

    public bool IsConnected => IsEnabled && State == MicrosoftToDoConnectionState.Connected;

    public bool CanSync => IsConnected;

    public bool CanTestConnection => IsEnabled && State != MicrosoftToDoConnectionState.Testing;

    public bool CanStartSignIn => IsEnabled
        && State is MicrosoftToDoConnectionState.NotConnected
            or MicrosoftToDoConnectionState.NeedsSignIn
            or MicrosoftToDoConnectionState.Failed;

    public bool NeedsAttention => IsEnabled
        && State is MicrosoftToDoConnectionState.NeedsSignIn
            or MicrosoftToDoConnectionState.Unavailable
            or MicrosoftToDoConnectionState.Failed;

    public string StatusText => State switch
    {
        MicrosoftToDoConnectionState.Disabled => "To Do is off",
        MicrosoftToDoConnectionState.NotConnected => "Ready to connect",
        MicrosoftToDoConnectionState.Testing => "Checking To Do",
        MicrosoftToDoConnectionState.Connected => "To Do connected",
        MicrosoftToDoConnectionState.NeedsSignIn => "Sign in needed",
        MicrosoftToDoConnectionState.Unavailable => "To Do unavailable",
        MicrosoftToDoConnectionState.Failed => "Connection failed",
        _ => "To Do status unknown"
    };

    public string DetailText => State switch
    {
        MicrosoftToDoConnectionState.Disabled => "Microsoft To Do is turned off in Settings.",
        MicrosoftToDoConnectionState.NotConnected => "Connect Microsoft To Do to sync personal tasks.",
        MicrosoftToDoConnectionState.Testing => UseMessageOrFallback(
            "Checking whether Microsoft To Do is available."),
        MicrosoftToDoConnectionState.Connected => BuildConnectedDetail(),
        MicrosoftToDoConnectionState.NeedsSignIn => UseMessageOrFallback(
            "Sign in again to use Microsoft To Do."),
        MicrosoftToDoConnectionState.Unavailable => UseMessageOrFallback(
            "Microsoft To Do is not available for this account right now."),
        MicrosoftToDoConnectionState.Failed => UseMessageOrFallback(
            "The Microsoft To Do connection test did not finish."),
        _ => "Microsoft To Do status is unknown."
    };

    public static MicrosoftToDoConnectionStatus Disabled()
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.Disabled,
            isEnabled: false,
            accountDisplayName: string.Empty,
            lastCheckedAt: null,
            message: string.Empty);
    }

    public static MicrosoftToDoConnectionStatus NotConnected()
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.NotConnected,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: null,
            message: string.Empty);
    }

    public static MicrosoftToDoConnectionStatus Testing(DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.Testing,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: string.Empty);
    }

    public static MicrosoftToDoConnectionStatus Connected(
        string? accountDisplayName = null,
        DateTimeOffset? checkedAt = null,
        string? message = null)
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.Connected,
            isEnabled: true,
            accountDisplayName: accountDisplayName ?? string.Empty,
            lastCheckedAt: checkedAt,
            message: message ?? string.Empty);
    }

    public static MicrosoftToDoConnectionStatus NeedsSignIn(
        string? message = null,
        DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.NeedsSignIn,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: message ?? string.Empty);
    }

    public static MicrosoftToDoConnectionStatus Unavailable(
        string? message = null,
        DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.Unavailable,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: message ?? string.Empty);
    }

    public static MicrosoftToDoConnectionStatus Failed(
        string failureMessage,
        DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftToDoConnectionStatus(
            MicrosoftToDoConnectionState.Failed,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: NormalizeRequired(failureMessage, nameof(failureMessage)));
    }

    private string BuildConnectedDetail()
    {
        if (!string.IsNullOrWhiteSpace(Message))
        {
            return Message;
        }

        if (!string.IsNullOrWhiteSpace(AccountDisplayName))
        {
            return $"Connected as {AccountDisplayName}.";
        }

        return "Microsoft To Do is connected.";
    }

    private string UseMessageOrFallback(string fallback)
    {
        return string.IsNullOrWhiteSpace(Message) ? fallback : Message;
    }

    private static void ValidateState(MicrosoftToDoConnectionState state)
    {
        if (!Enum.IsDefined(typeof(MicrosoftToDoConnectionState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Microsoft To Do connection state is not valid.");
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
