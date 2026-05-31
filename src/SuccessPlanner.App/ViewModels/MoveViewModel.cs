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
                ChooseNowCommand.RaiseCanExecuteChanged();
                ChooseScheduleCommand.RaiseCanExecuteChanged();
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

    public bool CanChooseTiming => HasSelectedActivity;

    public bool IsWalkSelected => SelectedActivityType == MovementActivityType.Walk;

    public bool IsWorkoutSelected => SelectedActivityType == MovementActivityType.Workout;

    public bool IsStretchSelected => SelectedActivityType == MovementActivityType.Stretch;

    public bool IsNowSelected => SelectedTimingChoice == MovementTimingChoice.Now;

    public bool IsScheduleSelected => SelectedTimingChoice == MovementTimingChoice.Schedule;

    public string WalkChoiceStatusText => IsWalkSelected ? "Selected" : "Choose";

    public string WorkoutChoiceStatusText => IsWorkoutSelected ? "Selected" : "Choose";

    public string StretchChoiceStatusText => IsStretchSelected ? "Selected" : "Choose";

    public string NowChoiceStatusText => IsNowSelected
        ? "Selected"
        : HasSelectedActivity ? "Choose" : "Pick activity";

    public string ScheduleChoiceStatusText => IsScheduleSelected
        ? "Selected"
        : HasSelectedActivity ? "Choose" : "Pick activity";

    public string EmptyStateText => HasSelectedActivity switch
    {
        false => "Choose one small movement activity.",
        true when !HasSelectedTiming => "Choose Now or Schedule next.",
        _ => "Choose a mind occupier next."
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

    private void RefreshMovementDraftText()
    {
        if (!SelectedActivityType.HasValue)
        {
            return;
        }

        string activityName = GetActivityName(SelectedActivityType.Value);
        SelectedActivityText = $"{activityName} selected.";

        if (SelectedTimingChoice == MovementTimingChoice.Now)
        {
            TimingText = "Now selected.";
            MovementPanelTitle = $"{activityName} Now";
            MovementPanelText = $"{activityName} is ready to start now for {PlannedMinutes} minutes.";
            MovementDraftStatusText = $"{activityName} now draft ready.";
            StatusText = $"{activityName} set for now.";
            return;
        }

        if (SelectedTimingChoice == MovementTimingChoice.Schedule && SelectedScheduledFor.HasValue)
        {
            string scheduledText = FormatScheduledFor(SelectedScheduledFor.Value);
            TimingText = $"Scheduled for {scheduledText}.";
            MovementPanelTitle = $"{activityName} Scheduled";
            MovementPanelText = $"{activityName} is scheduled for {scheduledText}.";
            MovementDraftStatusText = $"{activityName} scheduled draft ready.";
            StatusText = $"{activityName} scheduled.";
            return;
        }

        TimingText = "Not scheduled yet.";
        MovementPanelTitle = $"{activityName} Ready";
        MovementPanelText = $"{activityName} is ready for a {PlannedMinutes} minute movement plan.";
        MovementDraftStatusText = $"{activityName} draft ready.";
        StatusText = $"{activityName} selected.";
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
