using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    public MoveViewModel()
        : base(ScreenCatalog.Move)
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

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

    public MovementActivityType? SelectedActivityType => null;

    public bool HasSelectedActivity => SelectedActivityType.HasValue;

    public bool HasMovementDraft => false;

    public string EmptyStateText => "Choose one small movement activity.";

    public string SaveStatusText => "Movement is local-first and not saved yet.";

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StatusText = ReadyStatus;
        return Task.CompletedTask;
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
