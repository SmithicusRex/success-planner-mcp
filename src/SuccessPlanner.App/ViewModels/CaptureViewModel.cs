using System.Collections.ObjectModel;
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
    private const int MaxCapturedThoughts = 30;

    private readonly Func<TaskItem, CancellationToken, Task> _saveTaskAsync;
    private readonly Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadCapturedThoughtsAsync;
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
    private bool _isLoadingCapturedThoughts;
    private string _capturedThoughtsStatusText = "Captured thoughts not loaded yet.";
    private string _capturedThoughtCountText = "0 captured thoughts";

    public CaptureViewModel()
        : this(MissingTaskRepositorySaveAsync, NoCapturedThoughtsLoadAsync)
    {
    }

    public CaptureViewModel(Func<TaskItem, CancellationToken, Task> saveTaskAsync)
        : this(saveTaskAsync, NoCapturedThoughtsLoadAsync)
    {
    }

    public CaptureViewModel(
        Func<TaskItem, CancellationToken, Task> saveTaskAsync,
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadCapturedThoughtsAsync)
        : base(ScreenCatalog.Capture)
    {
        ArgumentNullException.ThrowIfNull(saveTaskAsync);
        ArgumentNullException.ThrowIfNull(loadCapturedThoughtsAsync);
        _saveTaskAsync = saveTaskAsync;
        _loadCapturedThoughtsAsync = loadCapturedThoughtsAsync;

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
        RefreshCapturedThoughtsCommand = new AsyncRelayCommand(
            () => LoadCapturedThoughtsAsync(CancellationToken.None),
            () => !IsLoadingCapturedThoughts);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ObservableCollection<CapturedThoughtViewModel> CapturedThoughts { get; } = [];

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

    public AsyncRelayCommand RefreshCapturedThoughtsCommand { get; }

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

    public bool IsLoadingCapturedThoughts
    {
        get => _isLoadingCapturedThoughts;
        private set
        {
            if (SetProperty(ref _isLoadingCapturedThoughts, value))
            {
                RefreshCapturedThoughtsCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string CapturedThoughtsStatusText
    {
        get => _capturedThoughtsStatusText;
        private set => SetProperty(ref _capturedThoughtsStatusText, value);
    }

    public string CapturedThoughtCountText
    {
        get => _capturedThoughtCountText;
        private set => SetProperty(ref _capturedThoughtCountText, value);
    }

    public bool HasCapturedThoughts => CapturedThoughts.Count > 0;

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return LoadCapturedThoughtsAsync(cancellationToken);
    }

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
            await LoadCapturedThoughtsAsync(cancellationToken);
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

    public async Task LoadCapturedThoughtsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsLoadingCapturedThoughts)
        {
            return;
        }

        try
        {
            IsLoadingCapturedThoughts = true;
            CapturedThoughtsStatusText = "Checking captured thoughts.";

            IReadOnlyList<TaskItem> capturedThoughts = await _loadCapturedThoughtsAsync(cancellationToken);
            CapturedThoughts.Clear();
            foreach (TaskItem thought in capturedThoughts.Take(MaxCapturedThoughts))
            {
                CapturedThoughts.Add(CapturedThoughtViewModel.FromTask(thought));
            }

            UpdateCapturedThoughtsSummary();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            CapturedThoughts.Clear();
            UpdateCapturedThoughtCount();
            CapturedThoughtsStatusText = "Captured thoughts could not load.";
        }
        finally
        {
            IsLoadingCapturedThoughts = false;
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

    private static Task<IReadOnlyList<TaskItem>> NoCapturedThoughtsLoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<TaskItem>>([]);
    }

    private void UpdateCapturedThoughtsSummary()
    {
        UpdateCapturedThoughtCount();
        CapturedThoughtsStatusText = HasCapturedThoughts
            ? "Recent captured thoughts are ready."
            : "No captured thoughts yet.";
    }

    private void UpdateCapturedThoughtCount()
    {
        CapturedThoughtCountText = CapturedThoughts.Count == 1
            ? "1 captured thought"
            : $"{CapturedThoughts.Count} captured thoughts";
        OnPropertyChanged(nameof(HasCapturedThoughts));
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

public sealed class CapturedThoughtViewModel
{
    private CapturedThoughtViewModel(TaskItem task)
    {
        Id = task.Id;
        Title = task.Title;
        NotesPreview = BuildNotesPreview(task.Notes);
        HasNotes = !string.IsNullOrWhiteSpace(NotesPreview);
        CreatedText = $"Captured {task.CreatedAt.LocalDateTime:MMM d, h:mm tt}";
        StatusText = BuildStatusText(task);
        CardToolTip = HasNotes ? $"{Title} - {NotesPreview}" : $"{Title} - {StatusText}";
    }

    public Guid Id { get; }

    public string Title { get; }

    public string NotesPreview { get; }

    public bool HasNotes { get; }

    public string CreatedText { get; }

    public string StatusText { get; }

    public string CardToolTip { get; }

    public static CapturedThoughtViewModel FromTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new CapturedThoughtViewModel(task);
    }

    private static string BuildNotesPreview(string notes)
    {
        string trimmed = notes.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return string.Empty;
        }

        return trimmed.Length <= 90 ? trimmed : $"{trimmed[..87]}...";
    }

    private static string BuildStatusText(TaskItem task)
    {
        if (task.Status == TaskItemStatus.Planned && task.DueDate.HasValue)
        {
            return $"Planned {task.DueDate.Value:MMM d}";
        }

        return task.Status switch
        {
            TaskItemStatus.Captured => "Captured",
            TaskItemStatus.Planned => "Planned",
            TaskItemStatus.InProgress => "Started",
            TaskItemStatus.Blocked => "Blocked",
            _ => task.Status.ToString()
        };
    }
}
