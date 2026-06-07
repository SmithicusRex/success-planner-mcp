using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerConnectionStatus
{
    private MicrosoftPlannerConnectionStatus(
        MicrosoftPlannerConnectionState state,
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

    public SourceSystem SourceSystem => SourceSystem.MicrosoftPlanner;

    public string DisplayName => "Microsoft Planner";

    public MicrosoftPlannerConnectionState State { get; }

    public bool IsEnabled { get; }

    public string AccountDisplayName { get; }

    public DateTimeOffset? LastCheckedAt { get; }

    public string Message { get; }

    public bool IsAvailable => IsEnabled && State == MicrosoftPlannerConnectionState.Available;

    public bool CanReadPlannerTasks => IsAvailable;

    public bool CanTestAvailability => IsEnabled && State != MicrosoftPlannerConnectionState.Testing;

    public bool CanStartSignIn => IsEnabled
        && State is MicrosoftPlannerConnectionState.NotConnected
            or MicrosoftPlannerConnectionState.NeedsSignIn
            or MicrosoftPlannerConnectionState.Failed;

    public bool NeedsAttention => IsEnabled
        && State is MicrosoftPlannerConnectionState.NeedsSignIn
            or MicrosoftPlannerConnectionState.Unavailable
            or MicrosoftPlannerConnectionState.Failed;

    public string StatusText => State switch
    {
        MicrosoftPlannerConnectionState.Disabled => "Planner is off",
        MicrosoftPlannerConnectionState.NotConnected => "Ready to check Planner",
        MicrosoftPlannerConnectionState.Testing => "Checking Planner",
        MicrosoftPlannerConnectionState.Available => "Planner available",
        MicrosoftPlannerConnectionState.NeedsSignIn => "Sign in needed",
        MicrosoftPlannerConnectionState.Unavailable => "Planner unavailable",
        MicrosoftPlannerConnectionState.Failed => "Planner check failed",
        _ => "Planner status unknown"
    };

    public string DetailText => State switch
    {
        MicrosoftPlannerConnectionState.Disabled => "Microsoft Planner is turned off in Settings.",
        MicrosoftPlannerConnectionState.NotConnected => "Check whether Planner data is available for this account.",
        MicrosoftPlannerConnectionState.Testing => UseMessageOrFallback(
            "Checking whether Microsoft Planner plans and tasks are available."),
        MicrosoftPlannerConnectionState.Available => BuildAvailableDetail(),
        MicrosoftPlannerConnectionState.NeedsSignIn => UseMessageOrFallback(
            "Sign in again to check Microsoft Planner access."),
        MicrosoftPlannerConnectionState.Unavailable => UseMessageOrFallback(
            "Planner may require a work or school account, Planner license, or accessible plans."),
        MicrosoftPlannerConnectionState.Failed => UseMessageOrFallback(
            "The Microsoft Planner availability check did not finish."),
        _ => "Microsoft Planner status is unknown."
    };

    public static MicrosoftPlannerConnectionStatus Disabled()
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.Disabled,
            isEnabled: false,
            accountDisplayName: string.Empty,
            lastCheckedAt: null,
            message: string.Empty);
    }

    public static MicrosoftPlannerConnectionStatus NotConnected()
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.NotConnected,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: null,
            message: string.Empty);
    }

    public static MicrosoftPlannerConnectionStatus Testing(DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.Testing,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: string.Empty);
    }

    public static MicrosoftPlannerConnectionStatus Available(
        string? accountDisplayName = null,
        DateTimeOffset? checkedAt = null,
        string? message = null)
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.Available,
            isEnabled: true,
            accountDisplayName: accountDisplayName ?? string.Empty,
            lastCheckedAt: checkedAt,
            message: message ?? string.Empty);
    }

    public static MicrosoftPlannerConnectionStatus NeedsSignIn(
        string? message = null,
        DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.NeedsSignIn,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: message ?? string.Empty);
    }

    public static MicrosoftPlannerConnectionStatus Unavailable(
        string? message = null,
        DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.Unavailable,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: message ?? string.Empty);
    }

    public static MicrosoftPlannerConnectionStatus Failed(
        string failureMessage,
        DateTimeOffset? checkedAt = null)
    {
        return new MicrosoftPlannerConnectionStatus(
            MicrosoftPlannerConnectionState.Failed,
            isEnabled: true,
            accountDisplayName: string.Empty,
            lastCheckedAt: checkedAt,
            message: NormalizeRequired(failureMessage, nameof(failureMessage)));
    }

    private string BuildAvailableDetail()
    {
        if (!string.IsNullOrWhiteSpace(Message))
        {
            return Message;
        }

        if (!string.IsNullOrWhiteSpace(AccountDisplayName))
        {
            return $"Planner is available for {AccountDisplayName}.";
        }

        return "Microsoft Planner plans and tasks are available.";
    }

    private string UseMessageOrFallback(string fallback)
    {
        return string.IsNullOrWhiteSpace(Message) ? fallback : Message;
    }

    private static void ValidateState(MicrosoftPlannerConnectionState state)
    {
        if (!Enum.IsDefined(typeof(MicrosoftPlannerConnectionState), state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Microsoft Planner connection state is not valid.");
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
