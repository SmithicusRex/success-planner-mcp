using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class MoveViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string ReadyStatus = "Ready to plan movement.";
    private static readonly TimeSpan DefaultScheduleOffset = TimeSpan.FromHours(1);

    private readonly Func<DateTimeOffset> _nowProvider;
    private string _statusText = ReadyStatus;
    private string _movementPanelTitle = "Choose Movement";
    private string _movementPanelText = "Pick a small physical activity before you schedule or start.";
    private string _selectedActivityText = "No movement selected.";
    private string _timingText = "Not scheduled yet.";
    private string _mindOccupierText = "No mind occupier selected.";
    private string _spouseText = "Solo movement.";
    private string _movementDraftStatusText = "No movement plan created yet.";
    private int _plannedMinutes = MovementSession.DefaultPlannedMinutes;
    private MovementActivityType? _selectedActivityType;
    private MovementTimingChoice? _selectedTimingChoice;
    private MovementMindOccupierChoice? _selectedMindOccupierChoice;
    private MovementSpouseChoice? _selectedSpouseChoice;
    private DateTimeOffset? _selectedScheduledFor;

    public MoveViewModel(Func<DateTimeOffset>? nowProvider = null)
        : base(ScreenCatalog.Move)
    {
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
        ChooseWalkCommand = new AsyncRelayCommand(() => ChooseActivityAsync(MovementActivityType.Walk));
        ChooseWorkoutCommand = new AsyncRelayCommand(() => ChooseActivityAsync(MovementActivityType.Workout));
        ChooseStretchCommand = new AsyncRelayCommand(() => ChooseActivityAsync(MovementActivityType.Stretch));
        ChooseNowCommand = new AsyncRelayCommand(
            ChooseNowAsync,
            () => CanChooseTiming);
        ChooseScheduleCommand = new AsyncRelayCommand(
            ChooseScheduleAsync,
            () => CanChooseTiming);
        ChooseMusicCommand = new AsyncRelayCommand(
            () => ChooseMindOccupierAsync(MovementMindOccupierChoice.Music),
            () => CanChooseMindOccupier);
        ChoosePodcastCommand = new AsyncRelayCommand(
            () => ChooseMindOccupierAsync(MovementMindOccupierChoice.Podcast),
            () => CanChooseMindOccupier);
        ChooseAudiobookCommand = new AsyncRelayCommand(
            () => ChooseMindOccupierAsync(MovementMindOccupierChoice.Audiobook),
            () => CanChooseMindOccupier);
        ChooseSoloCommand = new AsyncRelayCommand(
            () => ChooseSpouseAsync(MovementSpouseChoice.Solo),
            () => CanChooseSpouseOption);
        ChooseWithSpouseCommand = new AsyncRelayCommand(
            () => ChooseSpouseAsync(MovementSpouseChoice.WithSpouse),
            () => CanChooseSpouseOption);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public AsyncRelayCommand ChooseWalkCommand { get; }

    public AsyncRelayCommand ChooseWorkoutCommand { get; }

    public AsyncRelayCommand ChooseStretchCommand { get; }

    public AsyncRelayCommand ChooseNowCommand { get; }

    public AsyncRelayCommand ChooseScheduleCommand { get; }

    public AsyncRelayCommand ChooseMusicCommand { get; }

    public AsyncRelayCommand ChoosePodcastCommand { get; }

    public AsyncRelayCommand ChooseAudiobookCommand { get; }

    public AsyncRelayCommand ChooseSoloCommand { get; }

    public AsyncRelayCommand ChooseWithSpouseCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string MovementPanelTitle
    {
        get => _movementPanelTitle;
        private set => SetProperty(ref _movementPanelTitle, value);
    }

    public string MovementPanelText
    {
        get => _movementPanelText;
        private set => SetProperty(ref _movementPanelText, value);
    }

    public string SelectedActivityText
    {
        get => _selectedActivityText;
        private set => SetProperty(ref _selectedActivityText, value);
    }

    public string TimingText
    {
        get => _timingText;
        private set => SetProperty(ref _timingText, value);
    }

    public string MindOccupierText
    {
        get => _mindOccupierText;
        private set => SetProperty(ref _mindOccupierText, value);
    }

    public string SpouseText
    {
        get => _spouseText;
        private set => SetProperty(ref _spouseText, value);
    }

    public string MovementDraftStatusText
    {
        get => _movementDraftStatusText;
        private set => SetProperty(ref _movementDraftStatusText, value);
    }

    public int PlannedMinutes
    {
        get => _plannedMinutes;
        private set
        {
            if (SetProperty(ref _plannedMinutes, value))
            {
                OnPropertyChanged(nameof(PlannedMinutesText));
            }
        }
    }

    public string PlannedMinutesText => $"{PlannedMinutes} minute movement";

    public MovementActivityType? SelectedActivityType
    {
        get => _selectedActivityType;
        private set
        {
            if (SetProperty(ref _selectedActivityType, value))
            {
                OnPropertyChanged(nameof(HasSelectedActivity));
                OnPropertyChanged(nameof(HasMovementDraft));
                OnPropertyChanged(nameof(IsWalkSelected));
                OnPropertyChanged(nameof(IsWorkoutSelected));
                OnPropertyChanged(nameof(IsStretchSelected));
                OnPropertyChanged(nameof(WalkChoiceStatusText));
                OnPropertyChanged(nameof(WorkoutChoiceStatusText));
                OnPropertyChanged(nameof(StretchChoiceStatusText));
                OnPropertyChanged(nameof(EmptyStateText));
                OnPropertyChanged(nameof(CanChooseTiming));
                OnPropertyChanged(nameof(NowChoiceStatusText));
                OnPropertyChanged(nameof(ScheduleChoiceStatusText));
                OnPropertyChanged(nameof(MusicChoiceStatusText));
                OnPropertyChanged(nameof(PodcastChoiceStatusText));
                OnPropertyChanged(nameof(AudiobookChoiceStatusText));
                OnPropertyChanged(nameof(SoloChoiceStatusText));
                OnPropertyChanged(nameof(WithSpouseChoiceStatusText));
                ChooseNowCommand.RaiseCanExecuteChanged();
                ChooseScheduleCommand.RaiseCanExecuteChanged();
                RaiseMindOccupierCommandStatesChanged();
                RaiseSpouseCommandStatesChanged();
            }
        }
    }

    public MovementTimingChoice? SelectedTimingChoice
    {
        get => _selectedTimingChoice;
        private set
        {
            if (SetProperty(ref _selectedTimingChoice, value))
            {
                OnPropertyChanged(nameof(HasSelectedTiming));
                OnPropertyChanged(nameof(IsNowSelected));
                OnPropertyChanged(nameof(IsScheduleSelected));
                OnPropertyChanged(nameof(NowChoiceStatusText));
                OnPropertyChanged(nameof(ScheduleChoiceStatusText));
                OnPropertyChanged(nameof(CanChooseMindOccupier));
                OnPropertyChanged(nameof(MusicChoiceStatusText));
                OnPropertyChanged(nameof(PodcastChoiceStatusText));
                OnPropertyChanged(nameof(AudiobookChoiceStatusText));
                OnPropertyChanged(nameof(SoloChoiceStatusText));
                OnPropertyChanged(nameof(WithSpouseChoiceStatusText));
                OnPropertyChanged(nameof(EmptyStateText));
                RaiseMindOccupierCommandStatesChanged();
                RaiseSpouseCommandStatesChanged();
            }
        }
    }

    public MovementMindOccupierChoice? SelectedMindOccupierChoice
    {
        get => _selectedMindOccupierChoice;
        private set
        {
            if (SetProperty(ref _selectedMindOccupierChoice, value))
            {
                OnPropertyChanged(nameof(HasSelectedMindOccupier));
                OnPropertyChanged(nameof(IsMusicSelected));
                OnPropertyChanged(nameof(IsPodcastSelected));
                OnPropertyChanged(nameof(IsAudiobookSelected));
                OnPropertyChanged(nameof(MusicChoiceStatusText));
                OnPropertyChanged(nameof(PodcastChoiceStatusText));
                OnPropertyChanged(nameof(AudiobookChoiceStatusText));
                OnPropertyChanged(nameof(CanChooseSpouseOption));
                OnPropertyChanged(nameof(SoloChoiceStatusText));
                OnPropertyChanged(nameof(WithSpouseChoiceStatusText));
                RaiseSpouseCommandStatesChanged();
                OnPropertyChanged(nameof(EmptyStateText));
            }
        }
    }

    public MovementSpouseChoice? SelectedSpouseChoice
    {
        get => _selectedSpouseChoice;
        private set
        {
            if (SetProperty(ref _selectedSpouseChoice, value))
            {
                OnPropertyChanged(nameof(HasSelectedSpouseOption));
                OnPropertyChanged(nameof(IsSoloSelected));
                OnPropertyChanged(nameof(IsWithSpouseSelected));
                OnPropertyChanged(nameof(SoloChoiceStatusText));
                OnPropertyChanged(nameof(WithSpouseChoiceStatusText));
                OnPropertyChanged(nameof(EmptyStateText));
            }
        }
    }

    public DateTimeOffset? SelectedScheduledFor
    {
        get => _selectedScheduledFor;
        private set => SetProperty(ref _selectedScheduledFor, value);
    }

    public bool HasSelectedActivity => SelectedActivityType.HasValue;

    public bool HasMovementDraft => HasSelectedActivity;

    public bool HasSelectedTiming => SelectedTimingChoice.HasValue;

    public bool HasSelectedMindOccupier => SelectedMindOccupierChoice.HasValue;

    public bool HasSelectedSpouseOption => SelectedSpouseChoice.HasValue;

    public bool CanChooseTiming => HasSelectedActivity;

    public bool CanChooseMindOccupier => HasSelectedTiming;

    public bool CanChooseSpouseOption => HasSelectedMindOccupier;

    public bool IsWalkSelected => SelectedActivityType == MovementActivityType.Walk;

    public bool IsWorkoutSelected => SelectedActivityType == MovementActivityType.Workout;

    public bool IsStretchSelected => SelectedActivityType == MovementActivityType.Stretch;

    public bool IsNowSelected => SelectedTimingChoice == MovementTimingChoice.Now;

    public bool IsScheduleSelected => SelectedTimingChoice == MovementTimingChoice.Schedule;

    public bool IsMusicSelected => SelectedMindOccupierChoice == MovementMindOccupierChoice.Music;

    public bool IsPodcastSelected => SelectedMindOccupierChoice == MovementMindOccupierChoice.Podcast;

    public bool IsAudiobookSelected => SelectedMindOccupierChoice == MovementMindOccupierChoice.Audiobook;

    public bool IsSoloSelected => SelectedSpouseChoice == MovementSpouseChoice.Solo;

    public bool IsWithSpouseSelected => SelectedSpouseChoice == MovementSpouseChoice.WithSpouse;

    public string WalkChoiceStatusText => IsWalkSelected ? "Selected" : "Choose";

    public string WorkoutChoiceStatusText => IsWorkoutSelected ? "Selected" : "Choose";

    public string StretchChoiceStatusText => IsStretchSelected ? "Selected" : "Choose";

    public string NowChoiceStatusText => IsNowSelected
        ? "Selected"
        : HasSelectedActivity ? "Choose" : "Pick activity";

    public string ScheduleChoiceStatusText => IsScheduleSelected
        ? "Selected"
        : HasSelectedActivity ? "Choose" : "Pick activity";

    public string MusicChoiceStatusText => IsMusicSelected
        ? "Selected"
        : HasSelectedTiming ? "Choose" : "Pick timing";

    public string PodcastChoiceStatusText => IsPodcastSelected
        ? "Selected"
        : HasSelectedTiming ? "Choose" : "Pick timing";

    public string AudiobookChoiceStatusText => IsAudiobookSelected
        ? "Selected"
        : HasSelectedTiming ? "Choose" : "Pick timing";

    public string SoloChoiceStatusText => IsSoloSelected
        ? "Selected"
        : HasSelectedMindOccupier ? "Choose" : "Pick mind";

    public string WithSpouseChoiceStatusText => IsWithSpouseSelected
        ? "Selected"
        : HasSelectedMindOccupier ? "Choose" : "Pick mind";

    public string EmptyStateText => HasSelectedActivity switch
    {
        false => "Choose one small movement activity.",
        true when !HasSelectedTiming => "Choose Now or Schedule next.",
        true when !HasSelectedMindOccupier => "Choose a mind occupier next.",
        true when !HasSelectedSpouseOption => "Choose spouse option next.",
        _ => "Ready to save movement activity."
    };

    public string SaveStatusText => "Movement is local-first and not saved yet.";

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StatusText = ReadyStatus;
        return Task.CompletedTask;
    }

    private Task ChooseActivityAsync(MovementActivityType activityType)
    {
        SelectedActivityType = activityType;
        RefreshMovementDraftText();

        return Task.CompletedTask;
    }

    private Task ChooseNowAsync()
    {
        if (!HasSelectedActivity)
        {
            return Task.CompletedTask;
        }

        SelectedScheduledFor = _nowProvider();
        SelectedTimingChoice = MovementTimingChoice.Now;
        RefreshMovementDraftText();

        return Task.CompletedTask;
    }

    private Task ChooseScheduleAsync()
    {
        if (!HasSelectedActivity)
        {
            return Task.CompletedTask;
        }

        SelectedScheduledFor = _nowProvider().Add(DefaultScheduleOffset);
        SelectedTimingChoice = MovementTimingChoice.Schedule;
        RefreshMovementDraftText();

        return Task.CompletedTask;
    }

    private Task ChooseMindOccupierAsync(MovementMindOccupierChoice mindOccupierChoice)
    {
        if (!HasSelectedTiming)
        {
            return Task.CompletedTask;
        }

        SelectedMindOccupierChoice = mindOccupierChoice;
        RefreshMovementDraftText();

        return Task.CompletedTask;
    }

    private Task ChooseSpouseAsync(MovementSpouseChoice spouseChoice)
    {
        if (!HasSelectedMindOccupier)
        {
            return Task.CompletedTask;
        }

        SelectedSpouseChoice = spouseChoice;
        RefreshMovementDraftText();

        return Task.CompletedTask;
    }

    private void RefreshMovementDraftText()
    {
        if (!SelectedActivityType.HasValue)
        {
            return;
        }

        string activityName = GetActivityName(SelectedActivityType.Value);
        SelectedActivityText = $"{activityName} selected.";
        string mindOccupierName = SelectedMindOccupierChoice.HasValue
            ? GetMindOccupierName(SelectedMindOccupierChoice.Value)
            : string.Empty;
        if (SelectedMindOccupierChoice.HasValue)
        {
            MindOccupierText = $"{mindOccupierName} selected.";
        }
        if (SelectedSpouseChoice.HasValue)
        {
            SpouseText = GetSpouseText(SelectedSpouseChoice.Value);
        }

        if (SelectedTimingChoice == MovementTimingChoice.Now)
        {
            TimingText = "Now selected.";
            MovementPanelTitle = $"{activityName} Now";
            MovementPanelText = SelectedMindOccupierChoice.HasValue
                ? $"{activityName} is ready to start now {BuildMindOccupierAndSpousePhrase(mindOccupierName)}."
                : $"{activityName} is ready to start now for {PlannedMinutes} minutes.";
            MovementDraftStatusText = SelectedMindOccupierChoice.HasValue
                ? $"{activityName} now {BuildDraftSupportPhrase(mindOccupierName)} ready."
                : $"{activityName} now draft ready.";
            StatusText = SelectedSpouseChoice.HasValue
                ? GetSpouseStatusText(SelectedSpouseChoice.Value)
                : SelectedMindOccupierChoice.HasValue
                ? $"{mindOccupierName} selected."
                : $"{activityName} set for now.";
            return;
        }

        if (SelectedTimingChoice == MovementTimingChoice.Schedule && SelectedScheduledFor.HasValue)
        {
            string scheduledText = FormatScheduledFor(SelectedScheduledFor.Value);
            TimingText = $"Scheduled for {scheduledText}.";
            MovementPanelTitle = $"{activityName} Scheduled";
            MovementPanelText = SelectedMindOccupierChoice.HasValue
                ? $"{activityName} is scheduled for {scheduledText} {BuildMindOccupierAndSpousePhrase(mindOccupierName)}."
                : $"{activityName} is scheduled for {scheduledText}.";
            MovementDraftStatusText = SelectedMindOccupierChoice.HasValue
                ? $"{activityName} scheduled {BuildDraftSupportPhrase(mindOccupierName)} ready."
                : $"{activityName} scheduled draft ready.";
            StatusText = SelectedSpouseChoice.HasValue
                ? GetSpouseStatusText(SelectedSpouseChoice.Value)
                : SelectedMindOccupierChoice.HasValue
                ? $"{mindOccupierName} selected."
                : $"{activityName} scheduled.";
            return;
        }

        TimingText = "Not scheduled yet.";
        MovementPanelTitle = $"{activityName} Ready";
        MovementPanelText = $"{activityName} is ready for a {PlannedMinutes} minute movement plan.";
        MovementDraftStatusText = $"{activityName} draft ready.";
        StatusText = $"{activityName} selected.";
    }

    private void RaiseMindOccupierCommandStatesChanged()
    {
        ChooseMusicCommand.RaiseCanExecuteChanged();
        ChoosePodcastCommand.RaiseCanExecuteChanged();
        ChooseAudiobookCommand.RaiseCanExecuteChanged();
    }

    private void RaiseSpouseCommandStatesChanged()
    {
        ChooseSoloCommand.RaiseCanExecuteChanged();
        ChooseWithSpouseCommand.RaiseCanExecuteChanged();
    }

    private string BuildMindOccupierAndSpousePhrase(string mindOccupierName)
    {
        string mindOccupier = mindOccupierName.ToLowerInvariant();
        return SelectedSpouseChoice switch
        {
            MovementSpouseChoice.Solo => $"with {mindOccupier} as a solo movement",
            MovementSpouseChoice.WithSpouse => $"with {mindOccupier} and spouse support",
            _ => $"with {mindOccupier}"
        };
    }

    private string BuildDraftSupportPhrase(string mindOccupierName)
    {
        return SelectedSpouseChoice switch
        {
            MovementSpouseChoice.Solo => $"with {mindOccupierName} solo",
            MovementSpouseChoice.WithSpouse => $"with {mindOccupierName} and spouse",
            _ => $"with {mindOccupierName}"
        };
    }

    private static string FormatScheduledFor(DateTimeOffset scheduledFor)
    {
        return scheduledFor.ToString("MMM d, h:mm tt", CultureInfo.InvariantCulture);
    }

    private static string GetActivityName(MovementActivityType activityType)
    {
        return activityType switch
        {
            MovementActivityType.Walk => "Walk",
            MovementActivityType.Workout => "Workout",
            MovementActivityType.Stretch => "Stretch",
            _ => throw new ArgumentOutOfRangeException(nameof(activityType), activityType, "Unsupported movement activity.")
        };
    }

    private static string GetMindOccupierName(MovementMindOccupierChoice mindOccupierChoice)
    {
        return mindOccupierChoice switch
        {
            MovementMindOccupierChoice.Music => "Music",
            MovementMindOccupierChoice.Podcast => "Podcast",
            MovementMindOccupierChoice.Audiobook => "Audiobook",
            _ => throw new ArgumentOutOfRangeException(nameof(mindOccupierChoice), mindOccupierChoice, "Unsupported mind occupier.")
        };
    }

    private static string GetSpouseText(MovementSpouseChoice spouseChoice)
    {
        return spouseChoice switch
        {
            MovementSpouseChoice.Solo => "Solo movement selected.",
            MovementSpouseChoice.WithSpouse => "With spouse selected.",
            _ => throw new ArgumentOutOfRangeException(nameof(spouseChoice), spouseChoice, "Unsupported spouse option.")
        };
    }

    private static string GetSpouseStatusText(MovementSpouseChoice spouseChoice)
    {
        return spouseChoice switch
        {
            MovementSpouseChoice.Solo => "Solo selected.",
            MovementSpouseChoice.WithSpouse => "With spouse selected.",
            _ => throw new ArgumentOutOfRangeException(nameof(spouseChoice), spouseChoice, "Unsupported spouse option.")
        };
    }

    private bool SetProperty<T>(
        ref T field,
        T value,
        [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
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
