using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class SettingsViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private readonly SettingsService _settingsService;
    private AppSettings _lastSavedSettings;
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
        string settingsFileStatus = "Loaded settings")
        : base(ScreenCatalog.Settings)
    {
        _settingsService = settingsService;
        _lastSavedSettings = CopySettings(settings);

        DestinationRules = [];
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => HasChanges);
        CancelCommand = new AsyncRelayCommand(CancelAsync, () => HasChanges);

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
        set => SetProperty(ref _enableMicrosoftToDo, value);
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
