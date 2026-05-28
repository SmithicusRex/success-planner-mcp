namespace SuccessPlanner.App.Infrastructure;

public sealed class AppSettings
{
    public string ProfileName { get; set; } = "Personal Success Planner";

    public int DefaultFocusMinutes { get; set; } = 20;

    public bool StartSyncOnLaunch { get; set; } = true;
}
