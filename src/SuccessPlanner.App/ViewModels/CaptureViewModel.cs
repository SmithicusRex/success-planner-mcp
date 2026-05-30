using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class CaptureViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string EmptyTitleMessage = "Add one small action first.";
    private const string ReadyStatus = "Ready to capture.";
    private const string NoSavedTaskMessage = "No task saved yet.";

    private readonly Func<TaskItem, CancellationToken, Task> _saveTaskAsync;
    private string _taskTitle = string.Empty;
    private string _notes = string.Empty;
    private string _validationMessage = string.Empty;
    private string _statusText = ReadyStatus;
    private DateOnly? _dueDate;
    private string _dateHintText = "No date selected.";
    private CaptureDestinationPreference _selectedDestination = CaptureDestinationPreference.LetMcpChoose;
    private string _destinationHintText = "Let MCP Choose.";
    private bool _hasSavedTask;
    private Guid? _lastSavedTaskId;
    private string _successFeedbackText = NoSavedTaskMessage;

    public CaptureViewModel()
        : this(MissingTaskRepositorySaveAsync)
    {
    }

    public CaptureViewModel(Func<TaskItem, CancellationToken, Task> saveTaskAsync)
        : base(ScreenCatalog.Capture)
    {
        ArgumentNullException.ThrowIfNull(saveTaskAsync);
        _saveTaskAsync = saveTaskAsync;

        TodayDateCommand = new AsyncRelayCommand(() =>
        {
            SelectDueDate(DateOnly.FromDateTime(DateTime.Today), "Today");
            return Task.CompletedTask;
        });
        TomorrowDateCommand = new AsyncRelayCommand(() =>
        {
            SelectDueDate(DateOnly.FromDateTime(DateTime.Today).AddDays(1), "Tomorrow");
            return Task.CompletedTask;
        });
        ThisWeekDateCommand = new AsyncRelayCommand(() =>
        {
            SelectDueDate(DateOnly.FromDateTime(DateTime.Today).AddDays(7), "This week");
            return Task.CompletedTask;
        });
        ClearDateCommand = new AsyncRelayCommand(() =>
        {
            ClearDueDate();
            return Task.CompletedTask;
        });
        LetMcpChooseDestinationCommand = new AsyncRelayCommand(() =>
        {
            SelectDestination(CaptureDestinationPreference.LetMcpChoose, "Let MCP Choose");
            return Task.CompletedTask;
        });
        LocalInboxDestinationCommand = new AsyncRelayCommand(() =>
        {
            SelectDestination(CaptureDestinationPreference.LocalInbox, "Local");
            return Task.CompletedTask;
        });
        MicrosoftToDoDestinationCommand = new AsyncRelayCommand(() =>
        {
            SelectDestination(CaptureDestinationPreference.MicrosoftToDo, "To Do");
            return Task.CompletedTask;
        });
        MicrosoftPlannerDestinationCommand = new AsyncRelayCommand(() =>
        {
            SelectDestination(CaptureDestinationPreference.MicrosoftPlanner, "Planner");
            return Task.CompletedTask;
        });
        MicrosoftProjectDestinationCommand = new AsyncRelayCommand(() =>
        {
            SelectDestination(CaptureDestinationPreference.MicrosoftProject, "Project");
            return Task.CompletedTask;
        });
        SaveTaskCommand = new AsyncRelayCommand(
            () => SaveCapturedTaskAsync(CancellationToken.None),
            () => CanCreateTask && !HasSavedTask);
        CaptureAnotherCommand = new AsyncRelayCommand(
            () =>
            {
                ResetCaptureForm();
                return Task.CompletedTask;
            },
            () => HasSavedTask);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ICommand TodayDateCommand { get; }

    public ICommand TomorrowDateCommand { get; }

    public ICommand ThisWeekDateCommand { get; }

    public ICommand ClearDateCommand { get; }

    public ICommand LetMcpChooseDestinationCommand { get; }

    public ICommand LocalInboxDestinationCommand { get; }

    public ICommand MicrosoftToDoDestinationCommand { get; }

    public ICommand MicrosoftPlannerDestinationCommand { get; }

    public ICommand MicrosoftProjectDestinationCommand { get; }

    public AsyncRelayCommand SaveTaskCommand { get; }

    public AsyncRelayCommand CaptureAnotherCommand { get; }

    public string TaskTitle
    {
        get => _taskTitle;
        set
        {
            if (!SetProperty(ref _taskTitle, value))
            {
                return;
            }

            OnPropertyChanged(nameof(CanCreateTask));
            SaveTaskCommand.RaiseCanExecuteChanged();
            ClearSavedState();

            if (CanCreateTask)
            {
                ValidationMessage = string.Empty;
            }
        }
    }

    public string Notes
    {
        get => _notes;
        set
        {
            if (SetProperty(ref _notes, value))
            {
                ClearSavedState();
            }
        }
    }

    public string ValidationMessage
    {
        get => _validationMessage;
        private set => SetProperty(ref _validationMessage, value);
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public DateOnly? DueDate
    {
        get => _dueDate;
        private set => SetProperty(ref _dueDate, value);
    }

    public string DateHintText
    {
        get => _dateHintText;
        private set => SetProperty(ref _dateHintText, value);
    }

    public CaptureDestinationPreference SelectedDestination
    {
        get => _selectedDestination;
        private set => SetProperty(ref _selectedDestination, value);
    }

    public string DestinationHintText
    {
        get => _destinationHintText;
        private set => SetProperty(ref _destinationHintText, value);
    }

    public bool HasSavedTask
    {
        get => _hasSavedTask;
        private set
        {
            if (SetProperty(ref _hasSavedTask, value))
            {
                SaveTaskCommand.RaiseCanExecuteChanged();
                CaptureAnotherCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public Guid? LastSavedTaskId
    {
        get => _lastSavedTaskId;
        private set => SetProperty(ref _lastSavedTaskId, value);
    }

    public string SuccessFeedbackText
    {
        get => _successFeedbackText;
        private set => SetProperty(ref _successFeedbackText, value);
    }

    public bool CanCreateTask => !string.IsNullOrWhiteSpace(TaskTitle);

    public bool TryCreateCapturedTask(out TaskItem? task)
    {
        if (!CanCreateTask)
        {
            task = null;
            ValidationMessage = EmptyTitleMessage;
            StatusText = "Capture needs a task title.";
            return false;
        }

        task = TaskItem.Capture(TaskTitle);
        task.UpdateNotes(Notes);
        if (DueDate.HasValue)
        {
            task.Schedule(DueDate);
        }

        ValidationMessage = string.Empty;
        StatusText = "Task ready to save.";
        return true;
    }

    public async Task SaveCapturedTaskAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryCreateCapturedTask(out TaskItem? task) || task is null)
        {
            return;
        }

        try
        {
            await _saveTaskAsync(task, cancellationToken);
            LastSavedTaskId = task.Id;
            HasSavedTask = true;
            ValidationMessage = string.Empty;
            SuccessFeedbackText = $"Saved locally: {task.Title}";
            StatusText = "Saved locally.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            LastSavedTaskId = null;
            HasSavedTask = false;
            SuccessFeedbackText = NoSavedTaskMessage;
            ValidationMessage = "Could not save locally. Try again.";
            StatusText = "Save failed.";
        }
    }

    public void ResetCaptureForm()
    {
        TaskTitle = string.Empty;
        Notes = string.Empty;
        ClearDueDate();
        SelectDestination(CaptureDestinationPreference.LetMcpChoose, "Let MCP Choose", updateStatus: false);
        ClearSavedState();
        ValidationMessage = string.Empty;
        StatusText = ReadyStatus;
    }

    private void SelectDueDate(DateOnly dueDate, string label)
    {
        DueDate = dueDate;
        DateHintText = $"{label}: {dueDate:MMM d}";
        StatusText = $"Date set for {label.ToLowerInvariant()}.";
        ClearSavedState();
    }

    private void ClearDueDate()
    {
        DueDate = null;
        DateHintText = "No date selected.";
        StatusText = ReadyStatus;
        ClearSavedState();
    }

    private void SelectDestination(
        CaptureDestinationPreference destination,
        string label,
        bool updateStatus = true)
    {
        SelectedDestination = destination;
        DestinationHintText = $"{label}.";
        ClearSavedState();

        if (updateStatus)
        {
            StatusText = $"Destination set to {label}.";
        }
    }

    private void ClearSavedState()
    {
        LastSavedTaskId = null;
        HasSavedTask = false;
        SuccessFeedbackText = NoSavedTaskMessage;
    }

    private static Task MissingTaskRepositorySaveAsync(TaskItem task, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Task save service is not configured.");
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
