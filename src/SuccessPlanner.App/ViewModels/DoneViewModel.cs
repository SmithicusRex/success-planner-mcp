using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class DoneViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const int RecentActiveLookbackDays = 14;
    private const string ReadyStatus = "Ready to choose a win.";
    private readonly Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadTasksAsync;
    private readonly Func<TaskItem, CancellationToken, Task> _saveTaskAsync;
    private readonly Func<DateOnly> _todayProvider;
    private readonly Dictionary<Guid, TaskItem> _loadedTasksById = [];
    private bool _isLoading;
    private bool _isCompleting;
    private string _statusText = ReadyStatus;
    private string _emptyStateText = "No active tasks ready to finish.";
    private string _taskCountText = "0 tasks";
    private Guid? _selectedTaskId;
    private string _selectedTaskTitle = "No task selected.";
    private string _completionPanelText = "Choose a recent active task, then mark it complete.";

    public DoneViewModel()
        : this((_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public DoneViewModel(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : this(CreateRecentActiveLoader(loadTasksAsync), MissingTaskRepositorySaveAsync, todayProvider)
    {
    }

    public DoneViewModel(
        Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : this(loadTasksAsync, MissingTaskRepositorySaveAsync, todayProvider)
    {
    }

    public DoneViewModel(
        Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<TaskItem, CancellationToken, Task> saveTaskAsync,
        Func<DateOnly>? todayProvider = null)
        : base(ScreenCatalog.Done)
    {
        ArgumentNullException.ThrowIfNull(loadTasksAsync);
        ArgumentNullException.ThrowIfNull(saveTaskAsync);
        _loadTasksAsync = loadTasksAsync;
        _saveTaskAsync = saveTaskAsync;
        _todayProvider = todayProvider ?? (() => DateOnly.FromDateTime(DateTime.Today));
        RefreshCommand = new AsyncRelayCommand(
            () => LoadTasksAsync(CancellationToken.None),
            () => !IsLoading);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ObservableCollection<DoneTaskCardViewModel> TaskCards { get; } = [];

    public ObservableCollection<DoneTaskCardViewModel> Tasks => TaskCards;

    public AsyncRelayCommand RefreshCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string EmptyStateText
    {
        get => _emptyStateText;
        private set => SetProperty(ref _emptyStateText, value);
    }

    public string TaskCountText
    {
        get => _taskCountText;
        private set => SetProperty(ref _taskCountText, value);
    }

    public bool HasTasks => TaskCards.Count > 0;

    public bool IsCompleting
    {
        get => _isCompleting;
        private set => SetProperty(ref _isCompleting, value);
    }

    public Guid? SelectedTaskId
    {
        get => _selectedTaskId;
        private set => SetProperty(ref _selectedTaskId, value);
    }

    public string SelectedTaskTitle
    {
        get => _selectedTaskTitle;
        private set => SetProperty(ref _selectedTaskTitle, value);
    }

    public string CompletionPanelText
    {
        get => _completionPanelText;
        private set => SetProperty(ref _completionPanelText, value);
    }

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return LoadTasksAsync(cancellationToken);
    }

    public async Task LoadTasksAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsLoading = true;
        StatusText = "Loading possible wins.";

        try
        {
            DateOnly today = _todayProvider();
            IReadOnlyList<TaskItem> loadedTasks = await _loadTasksAsync(today, cancellationToken);
            IReadOnlyList<DoneTaskCardViewModel> readyTasks = loadedTasks
                .Where(task => ShouldShowTask(task, today))
                .OrderBy(task => StatusSortValue(task.Status))
                .ThenByDescending(task => RecentActivityDate(task, today))
                .ThenBy(task => PrioritySortValue(task.Priority))
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => DoneTaskCardViewModel.FromTask(task, today, card => CompleteSelectedTaskAsync(card)))
                .ToList();

            _loadedTasksById.Clear();
            foreach (TaskItem task in loadedTasks.Where(task => ShouldShowTask(task, today)))
            {
                _loadedTasksById[task.Id] = task;
            }

            TaskCards.Clear();
            foreach (DoneTaskCardViewModel task in readyTasks)
            {
                TaskCards.Add(task);
            }

            UpdateTaskSummary();
            StatusText = HasTasks ? "Choose one task to complete." : "No active tasks ready.";
            EmptyStateText = HasTasks
                ? "Pick one finished action and record the win."
                : "Recent active tasks will appear here when a small win is ready.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            _loadedTasksById.Clear();
            TaskCards.Clear();
            UpdateTaskSummary();
            StatusText = "Done could not load.";
            EmptyStateText = "Try Refresh, or return Home and open Done again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task CompleteSelectedTaskAsync(
        DoneTaskCardViewModel taskCard,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(taskCard);
        cancellationToken.ThrowIfCancellationRequested();

        SelectTask(taskCard);

        if (IsCompleting)
        {
            return;
        }

        if (!_loadedTasksById.TryGetValue(taskCard.Id, out TaskItem? task))
        {
            StatusText = "Task needs refresh.";
            CompletionPanelText = "Refresh Done and choose the task again.";
            return;
        }

        if (task.Status == TaskItemStatus.Done)
        {
            RemoveTaskCard(task.Id);
            StatusText = "Task already complete.";
            CompletionPanelText = $"{task.Title} was already marked complete.";
            return;
        }

        try
        {
            IsCompleting = true;
            task.Complete();
            await _saveTaskAsync(task, cancellationToken);
            RemoveTaskCard(task.Id);
            StatusText = "Task completed locally.";
            CompletionPanelText = $"{task.Title} was marked complete.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            StatusText = "Complete save failed.";
            CompletionPanelText = "The task could not be saved locally. Refresh Done and try again.";
        }
        finally
        {
            IsCompleting = false;
        }
    }

    public static bool ShouldShowTask(TaskItem task, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status == TaskItemStatus.Done)
        {
            return false;
        }

        if (task.Status is TaskItemStatus.InProgress or TaskItemStatus.Blocked)
        {
            return true;
        }

        DateOnly cutoff = today.AddDays(-RecentActiveLookbackDays);
        return IsInRecentWindow(task.DueDate, cutoff, today)
            || IsInRecentWindow(task.StartDate, cutoff, today);
    }

    private static int StatusSortValue(TaskItemStatus status)
    {
        return status switch
        {
            TaskItemStatus.InProgress => 0,
            TaskItemStatus.Planned => 1,
            TaskItemStatus.Blocked => 2,
            TaskItemStatus.Captured => 3,
            _ => 4
        };
    }

    private static DateOnly RecentActivityDate(TaskItem task, DateOnly today)
    {
        DateOnly latest = DateOnly.MinValue;
        if (task.DueDate.HasValue && task.DueDate.Value <= today)
        {
            latest = task.DueDate.Value;
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today && task.StartDate.Value > latest)
        {
            latest = task.StartDate.Value;
        }

        return latest;
    }

    private static int PrioritySortValue(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => 0,
            TaskPriority.High => 1,
            TaskPriority.Normal => 2,
            TaskPriority.Low => 3,
            _ => 4
        };
    }

    private static bool IsInRecentWindow(DateOnly? date, DateOnly cutoff, DateOnly today)
    {
        return date.HasValue
            && date.Value >= cutoff
            && date.Value <= today;
    }

    private static Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> CreateRecentActiveLoader(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync)
    {
        ArgumentNullException.ThrowIfNull(loadTasksAsync);
        return (_, cancellationToken) => loadTasksAsync(cancellationToken);
    }

    private static Task MissingTaskRepositorySaveAsync(TaskItem task, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Task save service is not configured.");
    }

    private void SelectTask(DoneTaskCardViewModel taskCard)
    {
        SelectedTaskId = taskCard.Id;
        SelectedTaskTitle = taskCard.Title;
        CompletionPanelText = $"{taskCard.Title} is selected.";
    }

    private void RemoveTaskCard(Guid taskId)
    {
        _loadedTasksById.Remove(taskId);

        for (int index = 0; index < TaskCards.Count; index++)
        {
            if (TaskCards[index].Id == taskId)
            {
                TaskCards.RemoveAt(index);
                break;
            }
        }

        UpdateTaskSummary();
        if (!HasTasks)
        {
            EmptyStateText = "No recent active tasks remain.";
        }
    }

    private void UpdateTaskSummary()
    {
        TaskCountText = TaskCards.Count == 1 ? "1 task" : $"{TaskCards.Count} tasks";
        OnPropertyChanged(nameof(HasTasks));
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

public sealed class DoneTaskCardViewModel
{
    private readonly Func<DoneTaskCardViewModel, Task> _completeTaskAsync;

    private DoneTaskCardViewModel(
        TaskItem task,
        DateOnly today,
        Func<DoneTaskCardViewModel, Task>? completeTaskAsync)
    {
        _completeTaskAsync = completeTaskAsync ?? (_ => Task.CompletedTask);
        Id = task.Id;
        Title = task.Title;
        Notes = task.Notes;
        Status = task.Status;
        Priority = task.Priority;
        DueDate = task.DueDate;
        StartDate = task.StartDate;
        IsOverdue = task.DueDate.HasValue && task.DueDate.Value < today;
        IsDueToday = task.DueDate.HasValue && task.DueDate.Value == today;
        IsInProgress = task.Status == TaskItemStatus.InProgress;
        IsBlocked = task.Status == TaskItemStatus.Blocked;
        DueText = BuildDueText(task, today);
        DueBadgeText = BuildDueBadgeText(task, today);
        DueBadgeBackground = BuildDueBadgeBackground(task, today);
        DueBadgeForeground = BuildDueBadgeForeground(task, today);
        PriorityText = BuildPriorityText(task.Priority);
        PriorityBadgeText = PriorityText;
        PriorityBadgeBackground = BuildPriorityBadgeBackground(task.Priority);
        PriorityBadgeForeground = BuildPriorityBadgeForeground(task.Priority);
        StatusText = BuildStatusText(task.Status);
        StatusBadgeText = StatusText;
        StatusBadgeBackground = BuildStatusBadgeBackground(task.Status);
        StatusBadgeForeground = BuildStatusBadgeForeground(task.Status);
        HasNotes = !string.IsNullOrWhiteSpace(task.Notes);
        NotesPreview = HasNotes ? BuildNotesPreview(task.Notes) : string.Empty;
        CardAccentColor = BuildCardAccentColor(task, today);
        CardBorderColor = BuildCardBorderColor(task, today);
        CardIconGlyph = BuildCardIconGlyph(task, today);
        CardToolTip = $"{task.Title} - {DueText} - {StatusText}";
        CompleteCommand = new AsyncRelayCommand(CompleteTaskAsync);
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Notes { get; }

    public string NotesPreview { get; }

    public TaskItemStatus Status { get; }

    public TaskPriority Priority { get; }

    public DateOnly? DueDate { get; }

    public DateOnly? StartDate { get; }

    public bool IsOverdue { get; }

    public bool IsDueToday { get; }

    public bool IsInProgress { get; }

    public bool IsBlocked { get; }

    public bool HasNotes { get; }

    public string DueText { get; }

    public string DueBadgeText { get; }

    public string DueBadgeBackground { get; }

    public string DueBadgeForeground { get; }

    public string PriorityText { get; }

    public string PriorityBadgeText { get; }

    public string PriorityBadgeBackground { get; }

    public string PriorityBadgeForeground { get; }

    public string StatusText { get; }

    public string StatusBadgeText { get; }

    public string StatusBadgeBackground { get; }

    public string StatusBadgeForeground { get; }

    public string CardAccentColor { get; }

    public string CardBorderColor { get; }

    public string CardIconGlyph { get; }

    public string CardToolTip { get; }

    public AsyncRelayCommand CompleteCommand { get; }

    public static DoneTaskCardViewModel FromTask(
        TaskItem task,
        DateOnly today,
        Func<DoneTaskCardViewModel, Task>? completeTaskAsync = null)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new DoneTaskCardViewModel(task, today, completeTaskAsync);
    }

    private Task CompleteTaskAsync()
    {
        return _completeTaskAsync(this);
    }

    private static string BuildDueText(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return $"Overdue: {task.DueDate:MMM d}";
        }

        if (task.DueDate.HasValue && task.DueDate.Value == today)
        {
            return "Due today";
        }

        if (task.DueDate.HasValue)
        {
            return $"Due {task.DueDate:MMM d}";
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected for today";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "In progress";
        }

        return "Ready when finished";
    }

    private static string BuildDueBadgeText(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "Overdue";
        }

        if (task.DueDate.HasValue && task.DueDate.Value == today)
        {
            return "Due today";
        }

        if (task.DueDate.HasValue)
        {
            return "Upcoming";
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected today";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "In progress";
        }

        return "Ready";
    }

    private static string BuildDueBadgeBackground(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "#FFF0DF";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "#E7F8F7";
        }

        if (task.DueDate.HasValue || task.StartDate.HasValue)
        {
            return "#ECF8EE";
        }

        return "#F5F3EE";
    }

    private static string BuildDueBadgeForeground(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "#8A3D00";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "#186D69";
        }

        if (task.DueDate.HasValue || task.StartDate.HasValue)
        {
            return "#1E6B3A";
        }

        return "#5F656C";
    }

    private static string BuildPriorityText(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "Critical",
            TaskPriority.High => "High",
            TaskPriority.Normal => "Normal",
            TaskPriority.Low => "Low",
            _ => priority.ToString()
        };
    }

    private static string BuildPriorityBadgeBackground(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "#FDECEC",
            TaskPriority.High => "#FFF6D6",
            TaskPriority.Low => "#EEF6FF",
            _ => "#F5F3EE"
        };
    }

    private static string BuildPriorityBadgeForeground(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "#9C2D2D",
            TaskPriority.High => "#765A00",
            TaskPriority.Low => "#245EC9",
            _ => "#5F656C"
        };
    }

    private static string BuildStatusText(TaskItemStatus status)
    {
        return status switch
        {
            TaskItemStatus.InProgress => "In progress",
            _ => status.ToString()
        };
    }

    private static string BuildStatusBadgeBackground(TaskItemStatus status)
    {
        return status switch
        {
            TaskItemStatus.InProgress => "#E7F8F7",
            TaskItemStatus.Blocked => "#FDECEC",
            TaskItemStatus.Planned => "#ECF8EE",
            TaskItemStatus.Captured => "#EEF6FF",
            _ => "#F5F3EE"
        };
    }

    private static string BuildStatusBadgeForeground(TaskItemStatus status)
    {
        return status switch
        {
            TaskItemStatus.InProgress => "#186D69",
            TaskItemStatus.Blocked => "#9C2D2D",
            TaskItemStatus.Planned => "#1E6B3A",
            TaskItemStatus.Captured => "#245EC9",
            _ => "#5F656C"
        };
    }

    private static string BuildNotesPreview(string notes)
    {
        string trimmed = notes.Trim();
        return trimmed.Length <= 140 ? trimmed : $"{trimmed[..137]}...";
    }

    private static string BuildCardAccentColor(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "#FFBE7A";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "#8DDAD5";
        }

        if (task.Status == TaskItemStatus.Blocked)
        {
            return "#FFB6B6";
        }

        return "#DADDE2";
    }

    private static string BuildCardBorderColor(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "#E6BE95";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "#B7DEDB";
        }

        if (task.Status == TaskItemStatus.Blocked)
        {
            return "#E4B2B2";
        }

        return "#D4D8DD";
    }

    private static string BuildCardIconGlyph(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "\uE768";
        }

        if (task.Status == TaskItemStatus.Blocked)
        {
            return "\uE783";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "\uE823";
        }

        return "\uE73E";
    }
}
