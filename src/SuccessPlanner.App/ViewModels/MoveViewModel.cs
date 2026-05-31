using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class MoveViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string ReadyStatus = "Ready to plan movement.";
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

    public MoveViewModel()
        : base(ScreenCatalog.Move)
    {
        ChooseWalkCommand = new AsyncRelayCommand(() => ChooseActivityAsync(MovementActivityType.Walk));
        ChooseWorkoutCommand = new AsyncRelayCommand(() => ChooseActivityAsync(MovementActivityType.Workout));
        ChooseStretchCommand = new AsyncRelayCommand(() => ChooseActivityAsync(MovementActivityType.Stretch));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public AsyncRelayCommand ChooseWalkCommand { get; }

    public AsyncRelayCommand ChooseWorkoutCommand { get; }

    public AsyncRelayCommand ChooseStretchCommand { get; }

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
            }
        }
    }

    public bool HasSelectedActivity => SelectedActivityType.HasValue;

    public bool HasMovementDraft => HasSelectedActivity;

    public bool IsWalkSelected => SelectedActivityType == MovementActivityType.Walk;

    public bool IsWorkoutSelected => SelectedActivityType == MovementActivityType.Workout;

    public bool IsStretchSelected => SelectedActivityType == MovementActivityType.Stretch;

    public string WalkChoiceStatusText => IsWalkSelected ? "Selected" : "Choose";

    public string WorkoutChoiceStatusText => IsWorkoutSelected ? "Selected" : "Choose";

    public string StretchChoiceStatusText => IsStretchSelected ? "Selected" : "Choose";

    public string EmptyStateText => HasSelectedActivity
        ? "Choose Now or Schedule next."
        : "Choose one small movement activity.";

    public string SaveStatusText => "Movement is local-first and not saved yet.";

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StatusText = ReadyStatus;
        return Task.CompletedTask;
    }

    private Task ChooseActivityAsync(MovementActivityType activityType)
    {
        string activityName = GetActivityName(activityType);

        SelectedActivityType = activityType;
        SelectedActivityText = $"{activityName} selected.";
        MovementPanelTitle = $"{activityName} Ready";
        MovementPanelText = $"{activityName} is ready for a {PlannedMinutes} minute movement plan.";
        MovementDraftStatusText = $"{activityName} draft ready.";
        StatusText = $"{activityName} selected.";

        return Task.CompletedTask;
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
