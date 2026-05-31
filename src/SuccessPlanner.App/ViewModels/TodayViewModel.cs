using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class TodayViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string ReadyStatus = "Ready to load today.";
    private readonly Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadTodayTasksAsync;
    private readonly Func<DateOnly> _todayProvider;
    private bool _isLoading;
    private string _statusText = ReadyStatus;
    private string _emptyStateText = "No tasks due today.";
    private string _taskCountText = "0 tasks";

    public TodayViewModel()
        : this((_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public TodayViewModel(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : this(CreateTodayLoader(loadTasksAsync), todayProvider)
    {
    }

    public TodayViewModel(
        Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTodayTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : base(ScreenCatalog.Today)
    {
        ArgumentNullException.ThrowIfNull(loadTodayTasksAsync);
        _loadTodayTasksAsync = loadTodayTasksAsync;
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

    public ObservableCollection<TodayTaskCardViewModel> TaskCards { get; } = [];

    public ObservableCollection<TodayTaskCardViewModel> Tasks => TaskCards;

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

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return LoadTasksAsync(cancellationToken);
    }

    public async Task LoadTasksAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsLoading = true;
        StatusText = "Loading today.";

        try
        {
            DateOnly today = _todayProvider();
            IReadOnlyList<TaskItem> loadedTasks = await _loadTodayTasksAsync(today, cancellationToken);
            IReadOnlyList<TodayTaskCardViewModel> todayTasks = loadedTasks
                .Where(task => ShouldShowTask(task, today))
                .OrderBy(task => TodaySortDate(task, today))
                .ThenBy(task => PrioritySortValue(task.Priority))
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => TodayTaskCardViewModel.FromTask(task, today))
                .ToList();

            TaskCards.Clear();
            foreach (TodayTaskCardViewModel task in todayTasks)
            {
                TaskCards.Add(task);
            }

            UpdateTaskSummary();
            StatusText = HasTasks ? "Today is ready." : "Today is clear.";
            EmptyStateText = HasTasks
                ? "Choose one small action."
                : "No tasks due today. Capture or plan the next tiny step.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            TaskCards.Clear();
            UpdateTaskSummary();
            StatusText = "Today could not load.";
            EmptyStateText = "Try Refresh, or return Home and open Today again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public static bool ShouldShowTask(TaskItem task, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status == TaskItemStatus.Done)
        {
            return false;
        }

        return task.Status == TaskItemStatus.InProgress
            || (task.DueDate.HasValue && task.DueDate.Value <= today)
            || (task.StartDate.HasValue && task.StartDate.Value <= today);
    }

    private static Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> CreateTodayLoader(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync)
    {
        ArgumentNullException.ThrowIfNull(loadTasksAsync);
        return (_, cancellationToken) => loadTasksAsync(cancellationToken);
    }

    private static DateOnly TodaySortDate(TaskItem task, DateOnly today)
    {
        if (task.DueDate.HasValue && task.DueDate.Value <= today)
        {
            return task.DueDate.Value;
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return task.StartDate.Value;
        }

        return DateOnly.MaxValue;
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

public sealed class TodayTaskCardViewModel
{
    private TodayTaskCardViewModel(TaskItem task, DateOnly today)
    {
        Id = task.Id;
        Title = task.Title;
        Notes = task.Notes;
        Status = task.Status;
        Priority = task.Priority;
        DueDate = task.DueDate;
        StartDate = task.StartDate;
        IsOverdue = task.DueDate.HasValue && task.DueDate.Value < today;
        IsDueToday = task.DueDate.HasValue && task.DueDate.Value == today;
        IsSelectedForToday = task.StartDate.HasValue && task.StartDate.Value <= today;
        IsInProgress = task.Status == TaskItemStatus.InProgress;
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
        CardToolTip = BuildCardToolTip(task, today);
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

    public bool IsSelectedForToday { get; }

    public bool IsInProgress { get; }

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

    public static TodayTaskCardViewModel FromTask(TaskItem task, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new TodayTaskCardViewModel(task, today);
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

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected for today";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "In progress";
        }

        return "Today";
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

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected today";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "In progress";
        }

        return "Today";
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

        if (task.StartDate.HasValue && task.StartDate.Value <= today && task.DueDate != today)
        {
            return "#EAF4FF";
        }

        return "#ECF8EE";
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

        if (task.StartDate.HasValue && task.StartDate.Value <= today && task.DueDate != today)
        {
            return "#245EC9";
        }

        return "#1E6B3A";
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
            TaskPriority.Normal => "#F5F3EE",
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

        if (task.Priority is TaskPriority.Critical or TaskPriority.High)
        {
            return "#FFE08A";
        }

        return "#A8E6B1";
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

        return "#DDE6DF";
    }

    private static string BuildCardIconGlyph(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "\uE768";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "\uE823";
        }

        return "\uE787";
    }

    private static string BuildCardToolTip(TaskItem task, DateOnly today)
    {
        return $"{task.Title} - {BuildDueText(task, today)} - {BuildStatusText(task.Status)}";
    }
}
