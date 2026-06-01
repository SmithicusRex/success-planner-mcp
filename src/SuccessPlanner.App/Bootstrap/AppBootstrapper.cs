using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;
using SuccessPlanner.App.ViewModels;

namespace SuccessPlanner.App.Bootstrap;

public sealed class AppBootstrapper
{
    private readonly AppPaths _paths;
    private readonly SettingsService _settingsService;
    private readonly DatabaseService _databaseService;
    private readonly DatabaseStartupMigrationService _databaseStartupMigrationService;
    private readonly TaskRepository _taskRepository;
    private readonly NoteRepository _noteRepository;
    private readonly FocusSessionRepository _focusSessionRepository;
    private readonly MovementSessionRepository _movementSessionRepository;
    private readonly BackgroundWorkerHost _backgroundWorkerHost;
    private readonly NavigationService _navigationService;
    private AppSettings? _loadedSettings;
    private string _settingsFileStatus = "Settings not loaded yet";
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
        _databaseStartupMigrationService = new DatabaseStartupMigrationService(_databaseService);
        _taskRepository = new TaskRepository(_paths);
        _noteRepository = new NoteRepository(_paths);
        _focusSessionRepository = new FocusSessionRepository(_paths);
        _movementSessionRepository = new MovementSessionRepository(_paths);
        _backgroundWorkerHost = new BackgroundWorkerHost();
        _navigationService = new NavigationService();
        RegisterScreens();
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

            SettingsLoadResult settingsLoadResult = await _settingsService.LoadOrCreateWithStatusAsync(cancellationToken);
            AppSettings settings = settingsLoadResult.Settings;
            _loadedSettings = settings;
            _settingsFileStatus = settingsLoadResult.StatusText;

            DatabaseStartupMigrationResult databaseStartup =
                await _databaseStartupMigrationService.RunAsync(cancellationToken);

            await _navigationService.GoHomeAsync(cancellationToken);

            AppShellViewModel shellViewModel = new(
                statusText: databaseStartup.StatusText,
                footerText: $"Local control center - {settings.ProfileName}",
                navigationService: _navigationService);

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
            await CleanupFailedStartAsync();
            await WriteStartupFailureLogAsync(ex, cancellationToken);
            return BootstrapResult.Failed(
                "Success Planner MCP could not start. Your local data was not changed. Check the log folder for details.");
        }
    }

    private void RegisterScreens()
    {
        _navigationService.Register(AppScreen.Home, () => new HomeScreenViewModel(_navigationService));
        _navigationService.Register(AppScreen.Capture, () => new CaptureViewModel(_taskRepository.AddAsync));
        _navigationService.Register(AppScreen.Today, () => new TodayViewModel(
            _taskRepository.GetTodayAsync,
            _taskRepository.SaveAsync));
        _navigationService.Register(AppScreen.Plan, () => new PlanViewModel(_taskRepository.GetUnplannedAsync));
        _navigationService.Register(AppScreen.StartWork, () => new StartWorkViewModel(
            _taskRepository.GetTodayAsync,
            _focusSessionRepository.SaveAsync,
            _taskRepository.SaveAsync));
        _navigationService.Register(AppScreen.Done, () => new DoneViewModel(
            _taskRepository.GetRecentActiveAsync,
            _taskRepository.SaveAsync,
            _noteRepository.AddTaskSmallWinAsync));
        _navigationService.Register(AppScreen.Move, () => new MoveViewModel(_movementSessionRepository.SaveAsync));
        _navigationService.Register(AppScreen.Review, () => new InitialScreenViewModel(ScreenCatalog.Review));
        _navigationService.Register(AppScreen.Find, () => new InitialScreenViewModel(ScreenCatalog.Find));
        _navigationService.Register(AppScreen.Settings, CreateSettingsViewModel);
    }

    private SettingsViewModel CreateSettingsViewModel()
    {
        AppSettings settings = _loadedSettings ?? LoadSettingsForView();

        return new SettingsViewModel(
            _settingsService,
            settings,
            settingsFileStatus: _settingsFileStatus);
    }

    private AppSettings LoadSettingsForView()
    {
        SettingsLoadResult result = _settingsService
            .LoadOrCreateWithStatusAsync(CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        _loadedSettings = result.Settings;
        _settingsFileStatus = result.StatusText;
        return result.Settings;
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

    private async Task CleanupFailedStartAsync()
    {
        try
        {
            await _backgroundWorkerHost.StopAsync(CancellationToken.None);
            await _databaseService.CloseAsync(CancellationToken.None);
        }
        catch
        {
            // Startup is already failing; avoid masking the original error with cleanup problems.
        }
    }
}
