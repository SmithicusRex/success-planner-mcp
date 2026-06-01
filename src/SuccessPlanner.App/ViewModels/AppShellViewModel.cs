using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.ViewModels;

public sealed class AppShellViewModel : INotifyPropertyChanged
{
    private readonly INavigationService _navigationService;
    private readonly Func<CancellationToken, Task<SyncQueueStatus>> _loadSyncStatusAsync;
    private readonly AsyncRelayCommand _goBackCommand;
    private readonly AsyncRelayCommand _goHomeCommand;
    private readonly AsyncRelayCommand _openFindCommand;
    private readonly AsyncRelayCommand _openSettingsCommand;
    private readonly AsyncRelayCommand _refreshSyncStatusCommand;
    private IScreenViewModel _currentScreen;
    private string _syncStatusText = "Sync clear";
    private string _syncStatusDetailText = "Sync queue is clear.";
    private string _syncStatusBackgroundColor = "#E6F4EA";
    private string _syncStatusBorderColor = "#CDEAD5";
    private string _syncStatusDotColor = "#24984E";
    private string _syncStatusForegroundColor = "#1E6B3A";

    public AppShellViewModel(string statusText, string footerText, INavigationService navigationService)
        : this(
            statusText,
            footerText,
            navigationService,
            _ => Task.FromResult(new SyncQueueStatus(0, 0, 0, 0, 0, 0)))
    {
    }

    public AppShellViewModel(
        string statusText,
        string footerText,
        INavigationService navigationService,
        Func<CancellationToken, Task<SyncQueueStatus>> loadSyncStatusAsync)
    {
        _navigationService = navigationService;
        _loadSyncStatusAsync = loadSyncStatusAsync ?? throw new ArgumentNullException(nameof(loadSyncStatusAsync));
        StatusText = statusText;
        FooterText = footerText;
        _currentScreen = navigationService.CurrentScreen
            ?? throw new InvalidOperationException("Navigation must have a current screen before the shell is created.");

        _goBackCommand = new AsyncRelayCommand(
            () => _navigationService.GoBackAsync(),
            () => CanGoBack);

        _goHomeCommand = new AsyncRelayCommand(
            () => _navigationService.GoHomeAsync(),
            () => !IsHome);

        _openFindCommand = new AsyncRelayCommand(() => _navigationService.NavigateToAsync(AppScreen.Find));
        _openSettingsCommand = new AsyncRelayCommand(() => _navigationService.NavigateToAsync(AppScreen.Settings));
        _refreshSyncStatusCommand = new AsyncRelayCommand(() => RefreshSyncStatusAsync());

        navigationService.Navigated += OnNavigated;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string StatusText { get; }

    public string FooterText { get; }

    public ICommand GoBackCommand => _goBackCommand;

    public ICommand GoHomeCommand => _goHomeCommand;

    public ICommand OpenFindCommand => _openFindCommand;

    public ICommand OpenSettingsCommand => _openSettingsCommand;

    public ICommand RefreshSyncStatusCommand => _refreshSyncStatusCommand;

    public bool CanGoBack => _navigationService.CanGoBack;

    public bool IsHome => CurrentScreen.Descriptor.Screen == AppScreen.Home;

    public string SyncStatusText
    {
        get => _syncStatusText;
        private set => SetProperty(ref _syncStatusText, value);
    }

    public string SyncStatusDetailText
    {
        get => _syncStatusDetailText;
        private set => SetProperty(ref _syncStatusDetailText, value);
    }

    public string SyncStatusBackgroundColor
    {
        get => _syncStatusBackgroundColor;
        private set => SetProperty(ref _syncStatusBackgroundColor, value);
    }

    public string SyncStatusBorderColor
    {
        get => _syncStatusBorderColor;
        private set => SetProperty(ref _syncStatusBorderColor, value);
    }

    public string SyncStatusDotColor
    {
        get => _syncStatusDotColor;
        private set => SetProperty(ref _syncStatusDotColor, value);
    }

    public string SyncStatusForegroundColor
    {
        get => _syncStatusForegroundColor;
        private set => SetProperty(ref _syncStatusForegroundColor, value);
    }

    public IScreenViewModel CurrentScreen
    {
        get => _currentScreen;
        private set
        {
            if (ReferenceEquals(_currentScreen, value))
            {
                return;
            }

            _currentScreen = value;
            OnPropertyChanged();
        }
    }

    public async Task RefreshSyncStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            SyncQueueStatus status = await _loadSyncStatusAsync(cancellationToken);
            ApplySyncStatus(status);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            SyncStatusText = "Sync unavailable";
            SyncStatusDetailText = "Sync status could not be read. Local data is still stored safely.";
            SyncStatusBackgroundColor = "#FFF1D6";
            SyncStatusBorderColor = "#F5C16C";
            SyncStatusDotColor = "#B85C00";
            SyncStatusForegroundColor = "#6F3C00";
        }
    }

    private void OnNavigated(object? sender, NavigationChangedEventArgs e)
    {
        CurrentScreen = e.CurrentScreen;
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(IsHome));
        _goBackCommand.RaiseCanExecuteChanged();
        _goHomeCommand.RaiseCanExecuteChanged();
    }

    private void ApplySyncStatus(SyncQueueStatus status)
    {
        SyncStatusDetailText = BuildSyncStatusDetailText(status);

        if (status.NeedsAttentionCount > 0)
        {
            SyncStatusText = "Sync needs attention";
            SyncStatusBackgroundColor = "#FFF1D6";
            SyncStatusBorderColor = "#F5C16C";
            SyncStatusDotColor = "#B85C00";
            SyncStatusForegroundColor = "#6F3C00";
            return;
        }

        if (status.SyncingCount > 0)
        {
            SyncStatusText = "Syncing now";
            SyncStatusBackgroundColor = "#EAF2FF";
            SyncStatusBorderColor = "#B8CEF8";
            SyncStatusDotColor = "#2F6FED";
            SyncStatusForegroundColor = "#1F4FA8";
            return;
        }

        if (status.PendingCount > 0)
        {
            SyncStatusText = "Sync waiting";
            SyncStatusBackgroundColor = "#FFF8DB";
            SyncStatusBorderColor = "#F1D675";
            SyncStatusDotColor = "#A97800";
            SyncStatusForegroundColor = "#654A00";
            return;
        }

        if (status.DisabledCount > 0)
        {
            SyncStatusText = "Sync paused";
            SyncStatusBackgroundColor = "#F1F3F5";
            SyncStatusBorderColor = "#D5DAE1";
            SyncStatusDotColor = "#68707A";
            SyncStatusForegroundColor = "#454B53";
            return;
        }

        SyncStatusText = "Sync clear";
        SyncStatusBackgroundColor = "#E6F4EA";
        SyncStatusBorderColor = "#CDEAD5";
        SyncStatusDotColor = "#24984E";
        SyncStatusForegroundColor = "#1E6B3A";
    }

    private static string BuildSyncStatusDetailText(SyncQueueStatus status)
    {
        return $"{status.SummaryText} Pending: {status.PendingCount}, Syncing: {status.SyncingCount}, "
            + $"Failed: {status.FailedCount}, Conflicts: {status.ConflictCount}, Disabled: {status.DisabledCount}.";
    }

    private bool SetProperty(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (field == value)
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
