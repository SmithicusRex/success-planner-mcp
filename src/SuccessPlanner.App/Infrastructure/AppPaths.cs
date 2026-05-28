namespace SuccessPlanner.App.Infrastructure;

public sealed class AppPaths
{
    public AppPaths(string appDataDirectory)
    {
        AppDataDirectory = appDataDirectory;
        LogDirectory = Path.Combine(AppDataDirectory, "logs");
        SettingsPath = Path.Combine(AppDataDirectory, "settings.json");
        DatabasePath = Path.Combine(AppDataDirectory, "success-planner.localdb");
    }

    public string AppDataDirectory { get; }

    public string LogDirectory { get; }

    public string SettingsPath { get; }

    public string DatabasePath { get; }

    public static AppPaths CreateDefault()
    {
        string root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return new AppPaths(Path.Combine(root, "SuccessPlannerMCP"));
    }
}
