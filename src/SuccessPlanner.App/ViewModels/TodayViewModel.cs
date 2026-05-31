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
    private readonly Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadTasksAsync;
    private readonly Func<DateOnly> _todayProvider;
    private bool _isLoading;
    private string _statusText = ReadyStatus;
    private string _emptyStateText = "No tasks due today.";
    private string _taskCountText = "0 tasks";

    public TodayViewModel()
        : this(_ => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public TodayViewModel(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : base(ScreenCatalog.Today)
    {
        ArgumentNullException.ThrowIfNull(loadTasksAsync);
        _loadTasksAsync = loadTasksAsync;
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

    public ObservableCollection<TodayTaskViewModel> Tasks { get; } = [];

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

    public bool HasTasks => Tasks.Count > 0;

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
            IReadOnlyList<TaskItem> allTasks = await _loadTasksAsync(cancellationToken);
            IReadOnlyList<TodayTaskViewModel> todayTasks = allTasks
                .Where(task => ShouldShowTask(task, today))
                .OrderBy(task => TodaySortDate(task, today))
                .ThenBy(task => PrioritySortValue(task.Priority))
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => TodayTaskViewModel.FromTask(task, today))
                .ToList();

            Tasks.Clear();
            foreach (TodayTaskViewModel task in todayTasks)
            {
                Tasks.Add(task);
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
            Tasks.Clear();
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
        TaskCountText = Tasks.Count == 1 ? "1 task" : $"{Tasks.Count} tasks";
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

public sealed class TodayTaskViewModel
{
    private TodayTaskViewModel(TaskItem task, DateOnly today)
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
        PriorityText = task.Priority.ToString();
        StatusText = task.Status.ToString();
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Notes { get; }

    public TaskItemStatus Status { get; }

    public TaskPriority Priority { get; }

    public DateOnly? DueDate { get; }

    public DateOnly? StartDate { get; }

    public bool IsOverdue { get; }

    public bool IsDueToday { get; }

    public bool IsSelectedForToday { get; }

    public bool IsInProgress { get; }

    public string DueText { get; }

    public string PriorityText { get; }

    public string StatusText { get; }

    public static TodayTaskViewModel FromTask(TaskItem task, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new TodayTaskViewModel(task, today);
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
}
