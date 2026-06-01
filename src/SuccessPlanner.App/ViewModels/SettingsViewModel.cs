using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.ViewModels;

public sealed class SettingsViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    private readonly MicrosoftToDoConnectionTestService _microsoftToDoConnectionTestService;
    private AppSettings _lastSavedSettings;
    private MicrosoftToDoConnectionStatus _microsoftToDoConnectionStatus;
    private string _profileName;
    private int _defaultFocusMinutes;
    private bool _startSyncOnLaunch;
    private string _themeName;
    private string _accentColor;
    private bool _useLargeControls;
    private bool _enableMicrosoftToDo;
    private bool _enablePlanner;
    private bool _enableProjectDesktop;
    private bool _enablePhoneCompanion;
    private string _settingsFileStatus;
    private string _saveStatus = "No changes saved yet.";
    private bool _hasChanges;

    public SettingsViewModel(
        SettingsService settingsService,
        AppSettings settings,
        string settingsFileStatus = "Loaded settings",
        MicrosoftToDoConnectionTestService? microsoftToDoConnectionTestService = null)
        : base(ScreenCatalog.Settings)
    {
        _settingsService = settingsService;
        _microsoftToDoConnectionTestService = microsoftToDoConnectionTestService
            ?? new MicrosoftToDoConnectionTestService();
        _lastSavedSettings = CopySettings(settings);
        _microsoftToDoConnectionStatus =
            _microsoftToDoConnectionTestService.GetInitialStatus(settings.Connections);

        DestinationRules = [];
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => HasChanges);
        CancelCommand = new AsyncRelayCommand(CancelAsync, () => HasChanges);
        TestMicrosoftToDoConnectionCommand = new AsyncRelayCommand(
            () => TestMicrosoftToDoConnectionAsync(CancellationToken.None),
            () => CanTestMicrosoftToDoConnection);

        _profileName = string.Empty;
        _themeName = string.Empty;
        _accentColor = string.Empty;
        _settingsFileStatus = settingsFileStatus;

        LoadFrom(settings, markClean: true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ProfileName
    {
        get => _profileName;
        set => SetProperty(ref _profileName, value);
    }

    public int DefaultFocusMinutes
    {
        get => _defaultFocusMinutes;
        set => SetProperty(ref _defaultFocusMinutes, Math.Clamp(value, 5, 60));
    }

    public bool StartSyncOnLaunch
    {
        get => _startSyncOnLaunch;
        set => SetProperty(ref _startSyncOnLaunch, value);
    }

    public string ThemeName
    {
        get => _themeName;
        set => SetProperty(ref _themeName, value);
    }

    public string AccentColor
    {
        get => _accentColor;
        set => SetProperty(ref _accentColor, value);
    }

    public bool UseLargeControls
    {
        get => _useLargeControls;
        set => SetProperty(ref _useLargeControls, value);
    }

    public bool EnableMicrosoftToDo
    {
        get => _enableMicrosoftToDo;
        set
        {
            if (SetProperty(ref _enableMicrosoftToDo, value))
            {
                SetMicrosoftToDoConnectionStatus(
                    _microsoftToDoConnectionTestService.GetInitialStatus(BuildCurrentConnectionSettings()));
            }
        }
    }

    public bool EnablePlanner
    {
        get => _enablePlanner;
        set => SetProperty(ref _enablePlanner, value);
    }

    public bool EnableProjectDesktop
    {
        get => _enableProjectDesktop;
        set => SetProperty(ref _enableProjectDesktop, value);
    }

    public bool EnablePhoneCompanion
    {
        get => _enablePhoneCompanion;
        set => SetProperty(ref _enablePhoneCompanion, value);
    }

    public string SettingsFileStatus
    {
        get => _settingsFileStatus;
        private set => SetProperty(ref _settingsFileStatus, value, markChanged: false);
    }

    public string SaveStatus
    {
        get => _saveStatus;
        private set => SetProperty(ref _saveStatus, value, markChanged: false);
    }

    public bool HasChanges
    {
        get => _hasChanges;
        private set
        {
            if (_hasChanges == value)
            {
                return;
            }

            _hasChanges = value;
            OnPropertyChanged();
            SaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }

    public ObservableCollection<DestinationRuleSummaryViewModel> DestinationRules { get; }

    public AsyncRelayCommand SaveCommand { get; }

    public AsyncRelayCommand CancelCommand { get; }

    public AsyncRelayCommand TestMicrosoftToDoConnectionCommand { get; }

    public string MicrosoftToDoStatusText => _microsoftToDoConnectionStatus.StatusText;

    public string MicrosoftToDoStatusDetailText => _microsoftToDoConnectionStatus.DetailText;

    public string MicrosoftToDoStatusBackgroundColor => _microsoftToDoConnectionStatus.State switch
    {
        MicrosoftToDoConnectionState.Connected => "#E7F8EE",
        MicrosoftToDoConnectionState.NeedsSignIn => "#FFF1D6",
        MicrosoftToDoConnectionState.Unavailable or MicrosoftToDoConnectionState.Failed => "#FFE7E0",
        MicrosoftToDoConnectionState.Testing => "#EAF2FF",
        MicrosoftToDoConnectionState.Disabled => "#EEF0F3",
        _ => "#F4F7FB"
    };

    public string MicrosoftToDoStatusAccentColor => _microsoftToDoConnectionStatus.State switch
    {
        MicrosoftToDoConnectionState.Connected => "#1E6B3A",
        MicrosoftToDoConnectionState.NeedsSignIn => "#946200",
        MicrosoftToDoConnectionState.Unavailable or MicrosoftToDoConnectionState.Failed => "#B8331F",
        MicrosoftToDoConnectionState.Testing => "#2F6FED",
        MicrosoftToDoConnectionState.Disabled => "#6A717A",
        _ => "#4E5965"
    };

    public bool CanTestMicrosoftToDoConnection => _microsoftToDoConnectionStatus.CanTestConnection;

    public bool MicrosoftToDoNeedsAttention => _microsoftToDoConnectionStatus.NeedsAttention;

    public async Task TestMicrosoftToDoConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!CanTestMicrosoftToDoConnection)
        {
            return;
        }

        SetMicrosoftToDoConnectionStatus(MicrosoftToDoConnectionStatus.Testing(DateTimeOffset.Now));

        MicrosoftToDoConnectionStatus testedStatus =
            await _microsoftToDoConnectionTestService.TestConnectionAsync(
                BuildCurrentConnectionSettings(),
                cancellationToken);

        SetMicrosoftToDoConnectionStatus(testedStatus);
    }

    private async Task SaveAsync()
    {
        AppSettings settings = ToSettings();

        try
        {
            await _settingsService.SaveAsync(settings, CancellationToken.None);
            _lastSavedSettings = CopySettings(settings);
            SettingsFileStatus = "Saved to local settings file";
            SaveStatus = "Settings saved.";
            HasChanges = false;
            SetMicrosoftToDoConnectionStatus(
                _microsoftToDoConnectionTestService.GetInitialStatus(settings.Connections));
        }
        catch (SettingsValidationException ex)
        {
            SaveStatus = ex.Message;
        }
        catch (IOException)
        {
            SaveStatus = "Settings could not be saved. Please try again.";
        }
    }

    private Task CancelAsync()
    {
        LoadFrom(_lastSavedSettings, markClean: true);
        SaveStatus = "Changes canceled.";
        return Task.CompletedTask;
    }

    private void LoadFrom(AppSettings settings, bool markClean)
    {
        ProfileName = settings.ProfileName;
        DefaultFocusMinutes = settings.DefaultFocusMinutes;
        StartSyncOnLaunch = settings.StartSyncOnLaunch;
        ThemeName = settings.Display.ThemeName;
        AccentColor = settings.Display.AccentColor;
        UseLargeControls = settings.Display.UseLargeControls;
        EnableMicrosoftToDo = settings.Connections.EnableMicrosoftToDo;
        EnablePlanner = settings.Connections.EnablePlanner;
        EnableProjectDesktop = settings.Connections.EnableProjectDesktop;
        EnablePhoneCompanion = settings.Connections.EnablePhoneCompanion;

        DestinationRules.Clear();
        foreach (DestinationRuleSettings rule in settings.DestinationRules)
        {
            DestinationRules.Add(new DestinationRuleSummaryViewModel(rule));
        }

        if (markClean)
        {
            HasChanges = false;
        }
    }

    private AppSettings ToSettings()
    {
        return new AppSettings
        {
            ProfileName = ProfileName,
            DefaultFocusMinutes = DefaultFocusMinutes,
            StartSyncOnLaunch = StartSyncOnLaunch,
            Display = new DisplaySettings
            {
                ThemeName = ThemeName,
                AccentColor = AccentColor,
                UseLargeControls = UseLargeControls
            },
            Connections = new ConnectionSettings
            {
                EnableMicrosoftToDo = EnableMicrosoftToDo,
                EnablePlanner = EnablePlanner,
                EnableProjectDesktop = EnableProjectDesktop,
                EnablePhoneCompanion = EnablePhoneCompanion
            },
            DestinationRules = _lastSavedSettings.DestinationRules
                .Select(rule => new DestinationRuleSettings
                {
                    Name = rule.Name,
                    Condition = rule.Condition,
                    DestinationSystem = rule.DestinationSystem,
                    DestinationName = rule.DestinationName
                })
                .ToList()
        };
    }

    private ConnectionSettings BuildCurrentConnectionSettings()
    {
        return new ConnectionSettings
        {
            EnableMicrosoftToDo = EnableMicrosoftToDo,
            EnablePlanner = EnablePlanner,
            EnableProjectDesktop = EnableProjectDesktop,
            EnablePhoneCompanion = EnablePhoneCompanion
        };
    }

    private void SetMicrosoftToDoConnectionStatus(MicrosoftToDoConnectionStatus status)
    {
        _microsoftToDoConnectionStatus = status;
        OnPropertyChanged(nameof(MicrosoftToDoStatusText));
        OnPropertyChanged(nameof(MicrosoftToDoStatusDetailText));
        OnPropertyChanged(nameof(MicrosoftToDoStatusBackgroundColor));
        OnPropertyChanged(nameof(MicrosoftToDoStatusAccentColor));
        OnPropertyChanged(nameof(CanTestMicrosoftToDoConnection));
        OnPropertyChanged(nameof(MicrosoftToDoNeedsAttention));
        TestMicrosoftToDoConnectionCommand.RaiseCanExecuteChanged();
    }

    private static AppSettings CopySettings(AppSettings settings)
    {
        return new AppSettings
        {
            SchemaVersion = settings.SchemaVersion,
            ProfileName = settings.ProfileName,
            DefaultFocusMinutes = settings.DefaultFocusMinutes,
            StartSyncOnLaunch = settings.StartSyncOnLaunch,
            Display = new DisplaySettings
            {
                ThemeName = settings.Display.ThemeName,
                AccentColor = settings.Display.AccentColor,
                UseLargeControls = settings.Display.UseLargeControls
            },
            Connections = new ConnectionSettings
            {
                EnableMicrosoftToDo = settings.Connections.EnableMicrosoftToDo,
                EnablePlanner = settings.Connections.EnablePlanner,
                EnableProjectDesktop = settings.Connections.EnableProjectDesktop,
                EnablePhoneCompanion = settings.Connections.EnablePhoneCompanion
            },
            DestinationRules = settings.DestinationRules
                .Select(rule => new DestinationRuleSettings
                {
                    Name = rule.Name,
                    Condition = rule.Condition,
                    DestinationSystem = rule.DestinationSystem,
                    DestinationName = rule.DestinationName
                })
                .ToList()
        };
    }

    private bool SetProperty<T>(
        ref T field,
        T value,
        bool markChanged = true,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);

        if (markChanged)
        {
            HasChanges = true;
        }

        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
