namespace SuccessPlanner.App.Infrastructure;

public sealed class AppSettings
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    public string ProfileName { get; set; } = "Personal Success Planner";

    public int DefaultFocusMinutes { get; set; } = 20;

    public bool StartSyncOnLaunch { get; set; } = true;

    public DisplaySettings Display { get; set; } = new();

    public ConnectionSettings Connections { get; set; } = new();

    public ProjectDesktopSettings ProjectDesktop { get; set; } = new();

    public PhoneCompanionSettings PhoneCompanion { get; set; } = new();

    public List<DestinationRuleSettings> DestinationRules { get; set; } = [];

    public static AppSettings CreateDefault()
    {
        return new AppSettings
        {
            DestinationRules =
            [
                new DestinationRuleSettings
                {
                    Name = "Default personal task lane",
                    Condition = "When no project destination is selected",
                    DestinationSystem = "ToDo",
                    DestinationName = "Tasks"
                }
            ]
        };
    }
}

public sealed class DisplaySettings
{
    public string ThemeName { get; set; } = "Light";

    public string AccentColor { get; set; } = "#2F6FED";

    public bool UseLargeControls { get; set; } = true;
}

public sealed class ConnectionSettings
{
    public bool EnableMicrosoftToDo { get; set; } = true;

    public bool EnablePlanner { get; set; }

    public bool EnableProjectDesktop { get; set; } = true;

    public bool EnablePhoneCompanion { get; set; }
}

public sealed class ProjectDesktopSettings
{
    public string LocalProjectFilePath { get; set; } = string.Empty;
}

public sealed class PhoneCompanionSettings
{
    public string SharedCaptureFolderPath { get; set; } = string.Empty;
}

public sealed class DestinationRuleSettings
{
    public string Name { get; set; } = string.Empty;

    public string Condition { get; set; } = string.Empty;

    public string DestinationSystem { get; set; } = "Local";

    public string DestinationName { get; set; } = "Inbox";
}
