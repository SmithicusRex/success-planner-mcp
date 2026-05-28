using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.Services;
using SuccessPlanner.App.ViewModels;

namespace SuccessPlanner.App.Bootstrap;

public sealed class AppBootstrapper
{
    private readonly AppPaths _paths;
    private readonly SettingsService _settingsService;
    private readonly DatabaseService _databaseService;
    private readonly BackgroundWorkerHost _backgroundWorkerHost;
    private bool _started;

    public AppBootstrapper()
        : this(AppPaths.CreateDefault())
    {
    }

    public AppBootstrapper(AppPaths paths)
    {
        _paths = paths;
        _settingsService = new SettingsService(_paths);
        _databaseService = new DatabaseService(_paths);
        _backgroundWorkerHost = new BackgroundWorkerHost();
    }

    public async Task<BootstrapResult> StartAsync(CancellationToken cancellationToken = default)
    {
        if (_started)
        {
            return BootstrapResult.Failed("Success Planner MCP is already running.");
        }

        try
        {
            Directory.CreateDirectory(_paths.AppDataDirectory);
            Directory.CreateDirectory(_paths.LogDirectory);

            AppSettings settings = await _settingsService.LoadOrCreateAsync(cancellationToken);
            await _databaseService.OpenAsync(cancellationToken);
            await _databaseService.MigrateAsync(cancellationToken);
            await _databaseService.HealthCheckAsync(cancellationToken);

            AppShellViewModel shellViewModel = new(
                statusText: "Ready",
                footerText: $"Local control center - {settings.ProfileName}");

            MainWindow mainWindow = new()
            {
                DataContext = shellViewModel
            };

            await _backgroundWorkerHost.StartAsync(cancellationToken);
            _started = true;

            return BootstrapResult.Ready(mainWindow);
        }
        catch (Exception ex)
        {
            await WriteStartupFailureLogAsync(ex, cancellationToken);
            return BootstrapResult.Failed(
                "Success Planner MCP could not start. Your local data was not changed. Check the log folder for details.");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_started)
        {
            return;
        }

        await _backgroundWorkerHost.StopAsync(cancellationToken);
        await _databaseService.CloseAsync(cancellationToken);
        _started = false;
    }

    private async Task WriteStartupFailureLogAsync(Exception exception, CancellationToken cancellationToken)
    {
        try
        {
            Directory.CreateDirectory(_paths.LogDirectory);
            string logPath = Path.Combine(_paths.LogDirectory, "startup-error.log");
            string message = $"[{DateTimeOffset.Now:u}] {exception}{Environment.NewLine}";
            await File.AppendAllTextAsync(logPath, message, cancellationToken);
        }
        catch
        {
            // Startup is already failing; avoid masking the original error with a logging problem.
        }
    }
}
