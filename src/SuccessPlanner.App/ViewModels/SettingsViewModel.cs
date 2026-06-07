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
    private readonly MicrosoftPlannerAvailabilityTestService _microsoftPlannerAvailabilityTestService;
    private readonly Func<CancellationToken, Task<MicrosoftPlannerImportResult>> _importMicrosoftPlannerTasksAsync;
    private readonly MicrosoftProjectDesktopDetector _microsoftProjectDesktopDetector;
    private readonly IMicrosoftProjectFilePicker _microsoftProjectFilePicker;
    private readonly Func<CancellationToken, Task<MicrosoftProjectImportResult>> _importMicrosoftProjectTasksAsync;
    private readonly PhoneCompanionStatusService _phoneCompanionStatusService;
    private AppSettings _lastSavedSettings;
    private MicrosoftToDoConnectionStatus _microsoftToDoConnectionStatus;
    private MicrosoftPlannerConnectionStatus _microsoftPlannerConnectionStatus;
    private PhoneCompanionConnectionStatus _phoneCompanionConnectionStatus;
    private MicrosoftPlannerImportResult? _microsoftPlannerImportResult;
    private MicrosoftProjectDesktopDetectionResult? _microsoftProjectDesktopDetectionResult;
    private MicrosoftProjectImportResult? _microsoftProjectImportResult;
    private string _microsoftProjectDesktopDetectionFailure = string.Empty;
    private bool _isImportingMicrosoftPlannerTasks;
    private bool _isDetectingMicrosoftProjectDesktop;
    private bool _isImportingMicrosoftProjectTasks;
    private string _profileName;
    private int _defaultFocusMinutes;
    private bool _startSyncOnLaunch;
    private string _themeName;
    private string _accentColor;
    private string _microsoftProjectFilePath;
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
        MicrosoftToDoConnectionTestService? microsoftToDoConnectionTestService = null,
        MicrosoftPlannerAvailabilityTestService? microsoftPlannerAvailabilityTestService = null,
        Func<CancellationToken, Task<MicrosoftPlannerImportResult>>? importMicrosoftPlannerTasksAsync = null,
        MicrosoftProjectDesktopDetector? microsoftProjectDesktopDetector = null,
        IMicrosoftProjectFilePicker? microsoftProjectFilePicker = null,
        Func<CancellationToken, Task<MicrosoftProjectImportResult>>? importMicrosoftProjectTasksAsync = null,
        PhoneCompanionStatusService? phoneCompanionStatusService = null)
        : base(ScreenCatalog.Settings)
    {
        _settingsService = settingsService;
        _microsoftToDoConnectionTestService = microsoftToDoConnectionTestService
            ?? new MicrosoftToDoConnectionTestService();
        _microsoftPlannerAvailabilityTestService = microsoftPlannerAvailabilityTestService
            ?? new MicrosoftPlannerAvailabilityTestService();
        _importMicrosoftPlannerTasksAsync = importMicrosoftPlannerTasksAsync
            ?? (_ => Task.FromResult(MicrosoftPlannerImportResult.Failed(
                _microsoftPlannerAvailabilityTestService.GetInitialStatus(settings.Connections),
                "Planner import unavailable",
                "Planner import is not configured for this session.")));
        _microsoftProjectDesktopDetector = microsoftProjectDesktopDetector
            ?? new MicrosoftProjectDesktopDetector();
        _microsoftProjectFilePicker = microsoftProjectFilePicker
            ?? new MicrosoftProjectFilePicker();
        _importMicrosoftProjectTasksAsync = importMicrosoftProjectTasksAsync
            ?? (_ => Task.FromResult(MicrosoftProjectImportResult.Failed(
                settings.ProjectDesktop.LocalProjectFilePath,
                "Project import unavailable",
                "Project import is not configured for this session.")));
        _phoneCompanionStatusService = phoneCompanionStatusService
            ?? new PhoneCompanionStatusService();
        _lastSavedSettings = CopySettings(settings);
        _microsoftToDoConnectionStatus =
            _microsoftToDoConnectionTestService.GetInitialStatus(settings.Connections);
        _microsoftPlannerConnectionStatus =
            _microsoftPlannerAvailabilityTestService.GetInitialStatus(settings.Connections);
        _phoneCompanionConnectionStatus =
            _phoneCompanionStatusService.GetInitialStatus(settings.Connections);

        DestinationRules = [];
        SaveCommand = new AsyncRelayCommand(SaveAsync, () => HasChanges);
        CancelCommand = new AsyncRelayCommand(CancelAsync, () => HasChanges);
        TestMicrosoftToDoConnectionCommand = new AsyncRelayCommand(
            () => TestMicrosoftToDoConnectionAsync(CancellationToken.None),
            () => CanTestMicrosoftToDoConnection);
        TestMicrosoftPlannerAvailabilityCommand = new AsyncRelayCommand(
            () => TestMicrosoftPlannerAvailabilityAsync(CancellationToken.None),
            () => CanTestMicrosoftPlannerAvailability);
        ImportMicrosoftPlannerTasksCommand = new AsyncRelayCommand(
            () => ImportMicrosoftPlannerTasksAsync(CancellationToken.None),
            () => CanImportMicrosoftPlannerTasks);
        DetectMicrosoftProjectDesktopCommand = new AsyncRelayCommand(
            () => DetectMicrosoftProjectDesktopAsync(CancellationToken.None),
            () => CanDetectMicrosoftProjectDesktop);
        SelectMicrosoftProjectFileCommand = new AsyncRelayCommand(
            () => SelectMicrosoftProjectFileAsync(CancellationToken.None),
            () => CanSelectMicrosoftProjectFile);
        ClearMicrosoftProjectFileCommand = new AsyncRelayCommand(
            () => ClearMicrosoftProjectFileAsync(),
            () => CanClearMicrosoftProjectFile);
        ImportMicrosoftProjectTasksCommand = new AsyncRelayCommand(
            () => ImportMicrosoftProjectTasksAsync(CancellationToken.None),
            () => CanImportMicrosoftProjectTasks);

        _profileName = string.Empty;
        _themeName = string.Empty;
        _accentColor = string.Empty;
        _microsoftProjectFilePath = string.Empty;
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

    public string MicrosoftProjectFilePath
    {
        get => _microsoftProjectFilePath;
        private set
        {
            string normalized = value?.Trim() ?? string.Empty;
            if (SetProperty(ref _microsoftProjectFilePath, normalized))
            {
                RaiseMicrosoftProjectFileProperties();
            }
        }
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
        set
        {
            if (SetProperty(ref _enablePlanner, value))
            {
                SetMicrosoftPlannerConnectionStatus(
                    _microsoftPlannerAvailabilityTestService.GetInitialStatus(BuildCurrentConnectionSettings()));
                _microsoftPlannerImportResult = null;
                RaiseMicrosoftPlannerImportProperties();
            }
        }
    }

    public bool EnableProjectDesktop
    {
        get => _enableProjectDesktop;
        set
        {
            if (SetProperty(ref _enableProjectDesktop, value))
            {
                ResetMicrosoftProjectDesktopDetection();
                RaiseMicrosoftProjectFileProperties();
            }
        }
    }

    public bool EnablePhoneCompanion
    {
        get => _enablePhoneCompanion;
        set
        {
            if (SetProperty(ref _enablePhoneCompanion, value))
            {
                SetPhoneCompanionConnectionStatus(
                    _phoneCompanionStatusService.GetInitialStatus(BuildCurrentConnectionSettings()));
            }
        }
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

    public AsyncRelayCommand TestMicrosoftPlannerAvailabilityCommand { get; }

    public AsyncRelayCommand ImportMicrosoftPlannerTasksCommand { get; }

    public AsyncRelayCommand DetectMicrosoftProjectDesktopCommand { get; }

    public AsyncRelayCommand SelectMicrosoftProjectFileCommand { get; }

    public AsyncRelayCommand ClearMicrosoftProjectFileCommand { get; }

    public AsyncRelayCommand ImportMicrosoftProjectTasksCommand { get; }

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

    public string MicrosoftPlannerStatusText => _microsoftPlannerConnectionStatus.StatusText;

    public string MicrosoftPlannerStatusDetailText => _microsoftPlannerConnectionStatus.DetailText;

    public string MicrosoftPlannerStatusBackgroundColor => _microsoftPlannerConnectionStatus.State switch
    {
        MicrosoftPlannerConnectionState.Available => "#E7F8EE",
        MicrosoftPlannerConnectionState.NeedsSignIn => "#FFF1D6",
        MicrosoftPlannerConnectionState.Unavailable or MicrosoftPlannerConnectionState.Failed => "#FFE7E0",
        MicrosoftPlannerConnectionState.Testing => "#EAF2FF",
        MicrosoftPlannerConnectionState.Disabled => "#EEF0F3",
        _ => "#F4F7FB"
    };

    public string MicrosoftPlannerStatusAccentColor => _microsoftPlannerConnectionStatus.State switch
    {
        MicrosoftPlannerConnectionState.Available => "#1E6B3A",
        MicrosoftPlannerConnectionState.NeedsSignIn => "#946200",
        MicrosoftPlannerConnectionState.Unavailable or MicrosoftPlannerConnectionState.Failed => "#B8331F",
        MicrosoftPlannerConnectionState.Testing => "#2F6FED",
        MicrosoftPlannerConnectionState.Disabled => "#6A717A",
        _ => "#4E5965"
    };

    public bool CanTestMicrosoftPlannerAvailability => _microsoftPlannerConnectionStatus.CanTestAvailability;

    public bool MicrosoftPlannerNeedsAttention => _microsoftPlannerConnectionStatus.NeedsAttention;

    public string MicrosoftPlannerImportStatusText
    {
        get
        {
            if (!EnablePlanner)
            {
                return "Planner import off";
            }

            if (_isImportingMicrosoftPlannerTasks)
            {
                return "Importing Planner tasks";
            }

            return _microsoftPlannerImportResult?.StatusText ?? "Ready to import Planner";
        }
    }

    public string MicrosoftPlannerImportDetailText
    {
        get
        {
            if (!EnablePlanner)
            {
                return "Turn on Planner to import assigned Planner tasks.";
            }

            if (_isImportingMicrosoftPlannerTasks)
            {
                return "Reading assigned Planner tasks and saving local read-only copies.";
            }

            return _microsoftPlannerImportResult?.DetailText
                ?? "Import assigned Planner tasks as local read-only Success Planner tasks.";
        }
    }

    public string MicrosoftPlannerImportStatusBackgroundColor
    {
        get
        {
            if (!EnablePlanner)
            {
                return "#EEF0F3";
            }

            if (_isImportingMicrosoftPlannerTasks)
            {
                return "#EAF2FF";
            }

            return _microsoftPlannerImportResult switch
            {
                { WasSuccessful: true } => "#E7F8EE",
                { WasSuccessful: false } => "#FFE7E0",
                _ => "#F4F7FB"
            };
        }
    }

    public string MicrosoftPlannerImportStatusAccentColor
    {
        get
        {
            if (!EnablePlanner)
            {
                return "#6A717A";
            }

            if (_isImportingMicrosoftPlannerTasks)
            {
                return "#2F6FED";
            }

            return _microsoftPlannerImportResult switch
            {
                { WasSuccessful: true } => "#1E6B3A",
                { WasSuccessful: false } => "#B8331F",
                _ => "#4E5965"
            };
        }
    }

    public bool CanImportMicrosoftPlannerTasks => EnablePlanner
        && !_isImportingMicrosoftPlannerTasks;

    public bool MicrosoftPlannerImportNeedsAttention => EnablePlanner
        && _microsoftPlannerImportResult is { WasSuccessful: false };

    public string MicrosoftProjectDesktopStatusText
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "Project detection off";
            }

            if (_isDetectingMicrosoftProjectDesktop)
            {
                return "Checking Project";
            }

            if (!string.IsNullOrWhiteSpace(_microsoftProjectDesktopDetectionFailure))
            {
                return "Project detection failed";
            }

            return _microsoftProjectDesktopDetectionResult?.StatusText ?? "Ready to detect";
        }
    }

    public string MicrosoftProjectDesktopStatusDetailText
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "Microsoft Project Desktop is turned off in Settings.";
            }

            if (_isDetectingMicrosoftProjectDesktop)
            {
                return $"Looking for {MicrosoftProjectDesktopDetectionResult.ExecutableName}.";
            }

            if (!string.IsNullOrWhiteSpace(_microsoftProjectDesktopDetectionFailure))
            {
                return _microsoftProjectDesktopDetectionFailure;
            }

            return _microsoftProjectDesktopDetectionResult?.DetailText
                ?? $"Detect Project to find {MicrosoftProjectDesktopDetectionResult.ExecutableName} on this PC.";
        }
    }

    public string MicrosoftProjectDesktopStatusBackgroundColor
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "#EEF0F3";
            }

            if (_isDetectingMicrosoftProjectDesktop)
            {
                return "#EAF2FF";
            }

            if (!string.IsNullOrWhiteSpace(_microsoftProjectDesktopDetectionFailure)
                || _microsoftProjectDesktopDetectionResult is { IsDetected: false })
            {
                return "#FFF1D6";
            }

            return _microsoftProjectDesktopDetectionResult is { IsDetected: true }
                ? "#E7F8EE"
                : "#F4F7FB";
        }
    }

    public string MicrosoftProjectDesktopStatusAccentColor
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "#6A717A";
            }

            if (_isDetectingMicrosoftProjectDesktop)
            {
                return "#2F6FED";
            }

            if (!string.IsNullOrWhiteSpace(_microsoftProjectDesktopDetectionFailure)
                || _microsoftProjectDesktopDetectionResult is { IsDetected: false })
            {
                return "#946200";
            }

            return _microsoftProjectDesktopDetectionResult is { IsDetected: true }
                ? "#1E6B3A"
                : "#4E5965";
        }
    }

    public bool CanDetectMicrosoftProjectDesktop => EnableProjectDesktop
        && !_isDetectingMicrosoftProjectDesktop;

    public bool MicrosoftProjectDesktopNeedsAttention => EnableProjectDesktop
        && (!string.IsNullOrWhiteSpace(_microsoftProjectDesktopDetectionFailure)
            || _microsoftProjectDesktopDetectionResult is { IsDetected: false });

    public bool HasMicrosoftProjectFileSelection => !string.IsNullOrWhiteSpace(MicrosoftProjectFilePath);

    public string MicrosoftProjectFileStatusText
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "Project file selection off";
            }

            return HasMicrosoftProjectFileSelection
                ? "Project file selected"
                : "No Project file selected";
        }
    }

    public string MicrosoftProjectFileDetailText
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "Turn on Project Desktop to choose a local Project file.";
            }

            return HasMicrosoftProjectFileSelection
                ? MicrosoftProjectFilePath
                : "Choose a local .mpp file before importing Project tasks.";
        }
    }

    public string MicrosoftProjectFileName => HasMicrosoftProjectFileSelection
        ? Path.GetFileName(MicrosoftProjectFilePath)
        : "None";

    public string MicrosoftProjectFileStatusBackgroundColor
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "#EEF0F3";
            }

            return HasMicrosoftProjectFileSelection ? "#E7F8EE" : "#F4F7FB";
        }
    }

    public string MicrosoftProjectFileStatusAccentColor
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "#6A717A";
            }

            return HasMicrosoftProjectFileSelection ? "#1E6B3A" : "#4E5965";
        }
    }

    public bool CanSelectMicrosoftProjectFile => EnableProjectDesktop;

    public bool CanClearMicrosoftProjectFile => EnableProjectDesktop
        && HasMicrosoftProjectFileSelection;

    public string MicrosoftProjectImportStatusText
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "Project import off";
            }

            if (_isImportingMicrosoftProjectTasks)
            {
                return "Importing Project tasks";
            }

            return _microsoftProjectImportResult?.StatusText ?? "Ready to import";
        }
    }

    public string MicrosoftProjectImportDetailText
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "Turn on Project Desktop to import tasks.";
            }

            if (_isImportingMicrosoftProjectTasks)
            {
                return "Reading the selected Project file and saving local tasks.";
            }

            if (_microsoftProjectImportResult is not null)
            {
                return _microsoftProjectImportResult.DetailText;
            }

            return HasMicrosoftProjectFileSelection
                ? $"Import {MicrosoftProjectFileName} into local Success Planner tasks."
                : "Choose a local .mpp file, then select Import Tasks.";
        }
    }

    public string MicrosoftProjectImportStatusBackgroundColor
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "#EEF0F3";
            }

            if (_isImportingMicrosoftProjectTasks)
            {
                return "#EAF2FF";
            }

            return _microsoftProjectImportResult switch
            {
                { WasSuccessful: true } => "#E7F8EE",
                { WasSuccessful: false } => "#FFE7E0",
                _ => "#F4F7FB"
            };
        }
    }

    public string MicrosoftProjectImportStatusAccentColor
    {
        get
        {
            if (!EnableProjectDesktop)
            {
                return "#6A717A";
            }

            if (_isImportingMicrosoftProjectTasks)
            {
                return "#2F6FED";
            }

            return _microsoftProjectImportResult switch
            {
                { WasSuccessful: true } => "#1E6B3A",
                { WasSuccessful: false } => "#B8331F",
                _ => "#4E5965"
            };
        }
    }

    public bool CanImportMicrosoftProjectTasks => EnableProjectDesktop
        && !_isImportingMicrosoftProjectTasks;

    public bool MicrosoftProjectImportNeedsAttention => EnableProjectDesktop
        && _microsoftProjectImportResult is { WasSuccessful: false };

    public string PhoneCompanionStatusText => _phoneCompanionConnectionStatus.StatusText;

    public string PhoneCompanionStatusDetailText => _phoneCompanionConnectionStatus.DetailText;

    public string PhoneCompanionStatusBackgroundColor => _phoneCompanionConnectionStatus.State switch
    {
        PhoneCompanionConnectionState.Ready => "#E7F8EE",
        PhoneCompanionConnectionState.NotConfigured => "#F4F7FB",
        PhoneCompanionConnectionState.Unavailable => "#FFF1D6",
        PhoneCompanionConnectionState.Failed => "#FFE7E0",
        PhoneCompanionConnectionState.Disabled => "#EEF0F3",
        _ => "#F4F7FB"
    };

    public string PhoneCompanionStatusAccentColor => _phoneCompanionConnectionStatus.State switch
    {
        PhoneCompanionConnectionState.Ready => "#1E6B3A",
        PhoneCompanionConnectionState.NotConfigured => "#4E5965",
        PhoneCompanionConnectionState.Unavailable => "#946200",
        PhoneCompanionConnectionState.Failed => "#B8331F",
        PhoneCompanionConnectionState.Disabled => "#6A717A",
        _ => "#4E5965"
    };

    public bool PhoneCompanionNeedsAttention => _phoneCompanionConnectionStatus.NeedsAttention;

    public bool CanImportPhoneCompanionCaptures => _phoneCompanionConnectionStatus.CanImportCaptures;

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

    public async Task TestMicrosoftPlannerAvailabilityAsync(CancellationToken cancellationToken = default)
    {
        if (!CanTestMicrosoftPlannerAvailability)
        {
            return;
        }

        SetMicrosoftPlannerConnectionStatus(MicrosoftPlannerConnectionStatus.Testing(DateTimeOffset.Now));

        MicrosoftPlannerConnectionStatus testedStatus =
            await _microsoftPlannerAvailabilityTestService.TestAvailabilityAsync(
                BuildCurrentConnectionSettings(),
                cancellationToken);

        SetMicrosoftPlannerConnectionStatus(testedStatus);
    }

    public async Task ImportMicrosoftPlannerTasksAsync(CancellationToken cancellationToken = default)
    {
        if (!CanImportMicrosoftPlannerTasks)
        {
            return;
        }

        _isImportingMicrosoftPlannerTasks = true;
        _microsoftPlannerImportResult = null;
        RaiseMicrosoftPlannerImportProperties();

        try
        {
            if (HasChanges)
            {
                await SaveAsync();
            }

            if (HasChanges)
            {
                _microsoftPlannerImportResult = MicrosoftPlannerImportResult.Failed(
                    _microsoftPlannerConnectionStatus,
                    "Settings not saved",
                    "Save settings before importing Planner tasks, then try again.");
                return;
            }

            _microsoftPlannerImportResult =
                await _importMicrosoftPlannerTasksAsync(cancellationToken);
            SetMicrosoftPlannerConnectionStatus(_microsoftPlannerImportResult.ConnectionStatus);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            MicrosoftPlannerConnectionStatus failed =
                MicrosoftPlannerConnectionStatus.Failed(BuildFailureMessage(ex), DateTimeOffset.Now);
            _microsoftPlannerImportResult = MicrosoftPlannerImportResult.Failed(
                failed,
                "Planner import failed",
                $"Check Planner availability and try again: {BuildFailureMessage(ex)}");
            SetMicrosoftPlannerConnectionStatus(failed);
        }
        finally
        {
            _isImportingMicrosoftPlannerTasks = false;
            RaiseMicrosoftPlannerImportProperties();
        }
    }

    public async Task DetectMicrosoftProjectDesktopAsync(CancellationToken cancellationToken = default)
    {
        if (!CanDetectMicrosoftProjectDesktop)
        {
            return;
        }

        _isDetectingMicrosoftProjectDesktop = true;
        _microsoftProjectDesktopDetectionFailure = string.Empty;
        RaiseMicrosoftProjectDesktopStatusProperties();

        try
        {
            _microsoftProjectDesktopDetectionResult =
                await _microsoftProjectDesktopDetector.DetectAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _microsoftProjectDesktopDetectionResult = null;
            _microsoftProjectDesktopDetectionFailure =
                $"Project detection could not finish: {BuildFailureMessage(ex)}";
        }
        finally
        {
            _isDetectingMicrosoftProjectDesktop = false;
            RaiseMicrosoftProjectDesktopStatusProperties();
        }
    }

    public async Task SelectMicrosoftProjectFileAsync(CancellationToken cancellationToken = default)
    {
        if (!CanSelectMicrosoftProjectFile)
        {
            return;
        }

        string? selectedPath = await _microsoftProjectFilePicker.PickProjectFileAsync(
            MicrosoftProjectFilePath,
            cancellationToken);
        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        MicrosoftProjectFilePath = selectedPath;
        _microsoftProjectImportResult = null;
        RaiseMicrosoftProjectImportProperties();
        SaveStatus = "Project file selected. Save settings to keep it.";
    }

    public Task ClearMicrosoftProjectFileAsync()
    {
        if (!CanClearMicrosoftProjectFile)
        {
            return Task.CompletedTask;
        }

        MicrosoftProjectFilePath = string.Empty;
        _microsoftProjectImportResult = null;
        RaiseMicrosoftProjectImportProperties();
        SaveStatus = "Project file cleared. Save settings to keep it.";
        return Task.CompletedTask;
    }

    public async Task ImportMicrosoftProjectTasksAsync(CancellationToken cancellationToken = default)
    {
        if (!CanImportMicrosoftProjectTasks)
        {
            return;
        }

        _isImportingMicrosoftProjectTasks = true;
        _microsoftProjectImportResult = null;
        RaiseMicrosoftProjectImportProperties();

        try
        {
            if (HasChanges)
            {
                await SaveAsync();
            }

            if (HasChanges)
            {
                _microsoftProjectImportResult = MicrosoftProjectImportResult.Failed(
                    MicrosoftProjectFilePath,
                    "Settings not saved",
                    "Save settings before importing Project tasks, then try again.");
                return;
            }

            _microsoftProjectImportResult =
                await _importMicrosoftProjectTasksAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _microsoftProjectImportResult = MicrosoftProjectImportResult.Failed(
                MicrosoftProjectFilePath,
                "Project import failed",
                $"Check the selected Project file and try again: {BuildFailureMessage(ex)}");
        }
        finally
        {
            _isImportingMicrosoftProjectTasks = false;
            RaiseMicrosoftProjectImportProperties();
        }
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
            SetMicrosoftPlannerConnectionStatus(
                _microsoftPlannerAvailabilityTestService.GetInitialStatus(settings.Connections));
            SetPhoneCompanionConnectionStatus(
                _phoneCompanionStatusService.GetInitialStatus(settings.Connections));
            _microsoftPlannerImportResult = null;
            RaiseMicrosoftPlannerImportProperties();
            RaiseMicrosoftProjectImportProperties();
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
        MicrosoftProjectFilePath = settings.ProjectDesktop.LocalProjectFilePath;
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
            ProjectDesktop = new ProjectDesktopSettings
            {
                LocalProjectFilePath = MicrosoftProjectFilePath
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

    private void SetMicrosoftPlannerConnectionStatus(MicrosoftPlannerConnectionStatus status)
    {
        _microsoftPlannerConnectionStatus = status;
        OnPropertyChanged(nameof(MicrosoftPlannerStatusText));
        OnPropertyChanged(nameof(MicrosoftPlannerStatusDetailText));
        OnPropertyChanged(nameof(MicrosoftPlannerStatusBackgroundColor));
        OnPropertyChanged(nameof(MicrosoftPlannerStatusAccentColor));
        OnPropertyChanged(nameof(CanTestMicrosoftPlannerAvailability));
        OnPropertyChanged(nameof(MicrosoftPlannerNeedsAttention));
        TestMicrosoftPlannerAvailabilityCommand.RaiseCanExecuteChanged();
    }

    private void SetPhoneCompanionConnectionStatus(PhoneCompanionConnectionStatus status)
    {
        _phoneCompanionConnectionStatus = status;
        OnPropertyChanged(nameof(PhoneCompanionStatusText));
        OnPropertyChanged(nameof(PhoneCompanionStatusDetailText));
        OnPropertyChanged(nameof(PhoneCompanionStatusBackgroundColor));
        OnPropertyChanged(nameof(PhoneCompanionStatusAccentColor));
        OnPropertyChanged(nameof(PhoneCompanionNeedsAttention));
        OnPropertyChanged(nameof(CanImportPhoneCompanionCaptures));
    }

    private void RaiseMicrosoftPlannerImportProperties()
    {
        OnPropertyChanged(nameof(MicrosoftPlannerImportStatusText));
        OnPropertyChanged(nameof(MicrosoftPlannerImportDetailText));
        OnPropertyChanged(nameof(MicrosoftPlannerImportStatusBackgroundColor));
        OnPropertyChanged(nameof(MicrosoftPlannerImportStatusAccentColor));
        OnPropertyChanged(nameof(CanImportMicrosoftPlannerTasks));
        OnPropertyChanged(nameof(MicrosoftPlannerImportNeedsAttention));
        ImportMicrosoftPlannerTasksCommand.RaiseCanExecuteChanged();
    }

    private void ResetMicrosoftProjectDesktopDetection()
    {
        _microsoftProjectDesktopDetectionResult = null;
        _microsoftProjectDesktopDetectionFailure = string.Empty;
        _isDetectingMicrosoftProjectDesktop = false;
        RaiseMicrosoftProjectDesktopStatusProperties();
        RaiseMicrosoftProjectImportProperties();
    }

    private void RaiseMicrosoftProjectDesktopStatusProperties()
    {
        OnPropertyChanged(nameof(MicrosoftProjectDesktopStatusText));
        OnPropertyChanged(nameof(MicrosoftProjectDesktopStatusDetailText));
        OnPropertyChanged(nameof(MicrosoftProjectDesktopStatusBackgroundColor));
        OnPropertyChanged(nameof(MicrosoftProjectDesktopStatusAccentColor));
        OnPropertyChanged(nameof(CanDetectMicrosoftProjectDesktop));
        OnPropertyChanged(nameof(MicrosoftProjectDesktopNeedsAttention));
        DetectMicrosoftProjectDesktopCommand.RaiseCanExecuteChanged();
    }

    private void RaiseMicrosoftProjectFileProperties()
    {
        OnPropertyChanged(nameof(HasMicrosoftProjectFileSelection));
        OnPropertyChanged(nameof(MicrosoftProjectFileStatusText));
        OnPropertyChanged(nameof(MicrosoftProjectFileDetailText));
        OnPropertyChanged(nameof(MicrosoftProjectFileName));
        OnPropertyChanged(nameof(MicrosoftProjectFileStatusBackgroundColor));
        OnPropertyChanged(nameof(MicrosoftProjectFileStatusAccentColor));
        OnPropertyChanged(nameof(CanSelectMicrosoftProjectFile));
        OnPropertyChanged(nameof(CanClearMicrosoftProjectFile));
        SelectMicrosoftProjectFileCommand.RaiseCanExecuteChanged();
        ClearMicrosoftProjectFileCommand.RaiseCanExecuteChanged();
        RaiseMicrosoftProjectImportProperties();
    }

    private void RaiseMicrosoftProjectImportProperties()
    {
        OnPropertyChanged(nameof(MicrosoftProjectImportStatusText));
        OnPropertyChanged(nameof(MicrosoftProjectImportDetailText));
        OnPropertyChanged(nameof(MicrosoftProjectImportStatusBackgroundColor));
        OnPropertyChanged(nameof(MicrosoftProjectImportStatusAccentColor));
        OnPropertyChanged(nameof(CanImportMicrosoftProjectTasks));
        OnPropertyChanged(nameof(MicrosoftProjectImportNeedsAttention));
        ImportMicrosoftProjectTasksCommand.RaiseCanExecuteChanged();
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
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
            ProjectDesktop = new ProjectDesktopSettings
            {
                LocalProjectFilePath = settings.ProjectDesktop.LocalProjectFilePath
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
