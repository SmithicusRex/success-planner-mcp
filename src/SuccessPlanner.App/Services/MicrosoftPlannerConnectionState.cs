namespace SuccessPlanner.App.Services;

public enum MicrosoftPlannerConnectionState
{
    Disabled,
    NotConnected,
    Testing,
    Available,
    NeedsSignIn,
    Unavailable,
    Failed
}
