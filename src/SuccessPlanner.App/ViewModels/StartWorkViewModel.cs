using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class StartWorkViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string ReadyStatus = "Ready to choose focus.";
    private const int ShortSessionMinutes = 10;
    private const int MediumSessionMinutes = 15;
    private readonly Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadTasksAsync;
    private readonly Func<DateOnly> _todayProvider;
    private readonly Dictionary<Guid, TaskItem> _loadedTasksById = [];
    private bool _isLoading;
    private int _plannedMinutes = FocusSession.DefaultPlannedMinutes;
    private string _statusText = ReadyStatus;
    private string _emptyStateText = "No focus options loaded.";
    private string _taskCountText = "0 options";
    private Guid? _selectedTaskId;
    private bool _selectedTaskIsSuggested;
    private string _selectedTaskTitle = "No focus selected.";
    private string _focusPanelTitle = "Choose Focus";
    private string _focusPanelText = "Pick one small action for a 20 minute focus session.";
    private string _focusIntention = "Choose one small action.";
    private Guid? _suggestedTaskId;
    private int _suggestedTaskScore;
    private string _suggestedTaskTitle = "No suggestion yet.";
    private string _suggestionPanelTitle = "Best Next";
    private string _suggestionPanelText = "Load focus options to see the next small action.";
    private string _suggestionReasonText = "No focus options loaded.";
    private string _suggestionBadgeText = "Waiting";

    public StartWorkViewModel()
        : this((_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public StartWorkViewModel(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : this(CreateFocusLoader(loadTasksAsync), todayProvider)
    {
    }

    public StartWorkViewModel(
        Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : base(ScreenCatalog.StartWork)
    {
        ArgumentNullException.ThrowIfNull(loadTasksAsync);
        _loadTasksAsync = loadTasksAsync;
        _todayProvider = todayProvider ?? (() => DateOnly.FromDateTime(DateTime.Today));
        RefreshCommand = new AsyncRelayCommand(
            () => LoadTasksAsync(CancellationToken.None),
            () => !IsLoading);
        UseSuggestionCommand = new AsyncRelayCommand(
            () => UseSuggestedTaskAsync(CancellationToken.None),
            () => HasSuggestedTask && !IsLoading);
        ChooseTenMinuteSessionCommand = new AsyncRelayCommand(() => ChooseSessionLengthAsync(ShortSessionMinutes));
        ChooseFifteenMinuteSessionCommand = new AsyncRelayCommand(() => ChooseSessionLengthAsync(MediumSessionMinutes));
        ChooseTwentyMinuteSessionCommand = new AsyncRelayCommand(() => ChooseSessionLengthAsync(FocusSession.DefaultPlannedMinutes));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ObservableCollection<StartWorkTaskOptionViewModel> TaskOptions { get; } = [];

    public ObservableCollection<StartWorkTaskOptionViewModel> Tasks => TaskOptions;

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand UseSuggestionCommand { get; }

    public AsyncRelayCommand ChooseTenMinuteSessionCommand { get; }

    public AsyncRelayCommand ChooseFifteenMinuteSessionCommand { get; }

    public AsyncRelayCommand ChooseTwentyMinuteSessionCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                UseSuggestionCommand.RaiseCanExecuteChanged();
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

    public bool HasTaskOptions => TaskOptions.Count > 0;

    public bool HasTasks => HasTaskOptions;

    public Guid? SelectedTaskId
    {
        get => _selectedTaskId;
        private set
        {
            if (SetProperty(ref _selectedTaskId, value))
            {
                OnPropertyChanged(nameof(HasSelectedTask));
            }
        }
    }

    public bool HasSelectedTask => SelectedTaskId.HasValue;

    public string SelectedTaskTitle
    {
        get => _selectedTaskTitle;
        private set => SetProperty(ref _selectedTaskTitle, value);
    }

    public string FocusPanelTitle
    {
        get => _focusPanelTitle;
        private set => SetProperty(ref _focusPanelTitle, value);
    }

    public string FocusPanelText
    {
        get => _focusPanelText;
        private set => SetProperty(ref _focusPanelText, value);
    }

    public string FocusIntention
    {
        get => _focusIntention;
        private set => SetProperty(ref _focusIntention, value);
    }

    public Guid? SuggestedTaskId
    {
        get => _suggestedTaskId;
        private set
        {
            if (SetProperty(ref _suggestedTaskId, value))
            {
                OnPropertyChanged(nameof(HasSuggestedTask));
                UseSuggestionCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasSuggestedTask => SuggestedTaskId.HasValue;

    public int SuggestedTaskScore
    {
        get => _suggestedTaskScore;
        private set => SetProperty(ref _suggestedTaskScore, value);
    }

    public string SuggestedTaskTitle
    {
        get => _suggestedTaskTitle;
        private set => SetProperty(ref _suggestedTaskTitle, value);
    }

    public string SuggestionPanelTitle
    {
        get => _suggestionPanelTitle;
        private set => SetProperty(ref _suggestionPanelTitle, value);
    }

    public string SuggestionPanelText
    {
        get => _suggestionPanelText;
        private set => SetProperty(ref _suggestionPanelText, value);
    }

    public string SuggestionReasonText
    {
        get => _suggestionReasonText;
        private set => SetProperty(ref _suggestionReasonText, value);
    }

    public string SuggestionBadgeText
    {
        get => _suggestionBadgeText;
        private set => SetProperty(ref _suggestionBadgeText, value);
    }

    public int PlannedMinutes
    {
        get => _plannedMinutes;
        private set
        {
            if (SetProperty(ref _plannedMinutes, value))
            {
                OnPropertyChanged(nameof(PlannedMinutesText));
                OnPropertyChanged(nameof(SessionChoiceSummaryText));
                OnPropertyChanged(nameof(IsTenMinuteSessionSelected));
                OnPropertyChanged(nameof(IsFifteenMinuteSessionSelected));
                OnPropertyChanged(nameof(IsTwentyMinuteSessionSelected));
            }
        }
    }

    public string PlannedMinutesText => $"{PlannedMinutes} minute focus";

    public string SessionChoiceSummaryText => $"{PlannedMinutes} minute session selected.";

    public bool IsTenMinuteSessionSelected => PlannedMinutes == ShortSessionMinutes;

    public bool IsFifteenMinuteSessionSelected => PlannedMinutes == MediumSessionMinutes;

    public bool IsTwentyMinuteSessionSelected => PlannedMinutes == FocusSession.DefaultPlannedMinutes;

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return LoadTasksAsync(cancellationToken);
    }

    public async Task LoadTasksAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsLoading = true;
        StatusText = "Loading focus options.";

        try
        {
            DateOnly today = _todayProvider();
            IReadOnlyList<TaskItem> loadedTasks = await _loadTasksAsync(today, cancellationToken);
            IReadOnlyList<TaskItem> focusItems = loadedTasks
                .Where(task => ShouldShowTask(task, today))
                .OrderBy(task => StatusSortValue(task.Status))
                .ThenBy(task => FocusSortDate(task, today))
                .ThenBy(task => PrioritySortValue(task.Priority))
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
            StartWorkSuggestion? suggestion = ChooseBestNextAction(focusItems, today);
            IReadOnlyList<StartWorkTaskOptionViewModel> focusOptions = focusItems
                .Select(task => StartWorkTaskOptionViewModel.FromTask(
                    task,
                    today,
                    SelectTaskAsync,
                    isSuggestedAction: suggestion?.Task.Id == task.Id))
                .ToList();

            _loadedTasksById.Clear();
            foreach (TaskItem task in focusItems)
            {
                _loadedTasksById[task.Id] = task;
            }

            TaskOptions.Clear();
            foreach (StartWorkTaskOptionViewModel task in focusOptions)
            {
                TaskOptions.Add(task);
            }

            ApplySuggestion(suggestion);

            if (SelectedTaskId.HasValue && !_loadedTasksById.ContainsKey(SelectedTaskId.Value))
            {
                ClearSelection();
            }

            UpdateTaskSummary();
            StatusText = HasSuggestedTask ? "Best next action suggested." : "No focus options ready.";
            EmptyStateText = HasTaskOptions
                ? "Pick one small action and start when ready."
                : "Capture or plan one tiny task before starting focus.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            _loadedTasksById.Clear();
            TaskOptions.Clear();
            ClearSelection();
            ClearSuggestion();
            UpdateTaskSummary();
            StatusText = "Start could not load.";
            EmptyStateText = "Try Refresh, or return Home and open Start again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SelectTask(StartWorkTaskOptionViewModel taskOption)
    {
        ArgumentNullException.ThrowIfNull(taskOption);

        SelectedTaskId = taskOption.Id;
        _selectedTaskIsSuggested = taskOption.IsSuggestedAction;
        SelectedTaskTitle = taskOption.Title;
        FocusIntention = taskOption.Title;
        FocusPanelTitle = "Focus Selected";
        FocusPanelText = BuildSelectedFocusPanelText(taskOption.Title, taskOption.IsSuggestedAction);
        StatusText = taskOption.IsSuggestedAction ? "Suggested focus selected." : "Focus selected.";
    }

    public Task ChooseSessionLengthAsync(int minutes)
    {
        SetSessionLength(minutes);
        return Task.CompletedTask;
    }

    public void SetSessionLength(int minutes)
    {
        if (minutes is not ShortSessionMinutes and not MediumSessionMinutes and not FocusSession.DefaultPlannedMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(minutes), "Start focus sessions must be 10, 15, or 20 minutes.");
        }

        PlannedMinutes = minutes;
        if (HasSelectedTask)
        {
            FocusPanelText = BuildSelectedFocusPanelText(SelectedTaskTitle, _selectedTaskIsSuggested);
        }
        else
        {
            FocusPanelText = $"Pick one small action for a {PlannedMinutes} minute focus session.";
        }

        StatusText = $"{PlannedMinutes} minute focus selected.";
    }

    public Task UseSuggestedTaskAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!SuggestedTaskId.HasValue)
        {
            StatusText = "No suggestion available.";
            return Task.CompletedTask;
        }

        StartWorkTaskOptionViewModel? suggestion = TaskOptions
            .FirstOrDefault(task => task.Id == SuggestedTaskId.Value);
        if (suggestion is null)
        {
            StatusText = "Suggestion needs refresh.";
            return Task.CompletedTask;
        }

        SelectTask(suggestion);
        return Task.CompletedTask;
    }

    public static bool ShouldShowTask(TaskItem task, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status is TaskItemStatus.Done or TaskItemStatus.Blocked)
        {
            return false;
        }

        return task.Status == TaskItemStatus.InProgress
            || (task.DueDate.HasValue && task.DueDate.Value <= today)
            || (task.StartDate.HasValue && task.StartDate.Value <= today);
    }

    private static Func<DateOnly, CancellationToken, Task<IReadOnlyList<TaskItem>>> CreateFocusLoader(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadTasksAsync)
    {
        ArgumentNullException.ThrowIfNull(loadTasksAsync);
        return (_, cancellationToken) => loadTasksAsync(cancellationToken);
    }

    private Task SelectTaskAsync(StartWorkTaskOptionViewModel taskOption)
    {
        SelectTask(taskOption);
        return Task.CompletedTask;
    }

    private void ClearSelection()
    {
        SelectedTaskId = null;
        _selectedTaskIsSuggested = false;
        SelectedTaskTitle = "No focus selected.";
        FocusIntention = "Choose one small action.";
        FocusPanelTitle = "Choose Focus";
        FocusPanelText = $"Pick one small action for a {PlannedMinutes} minute focus session.";
    }

    private string BuildSelectedFocusPanelText(string taskTitle, bool isSuggestedAction)
    {
        return isSuggestedAction
            ? $"{taskTitle} is the suggested next action for a {PlannedMinutes} minute focus session."
            : $"{taskTitle} is ready for a {PlannedMinutes} minute focus session.";
    }

    private void ApplySuggestion(StartWorkSuggestion? suggestion)
    {
        if (suggestion is null)
        {
            ClearSuggestion();
            return;
        }

        SuggestedTaskId = suggestion.Task.Id;
        SuggestedTaskTitle = suggestion.Task.Title;
        SuggestedTaskScore = suggestion.Score;
        SuggestionPanelTitle = "Best Next";
        SuggestionPanelText = $"Suggested: {suggestion.Task.Title}";
        SuggestionReasonText = suggestion.ReasonText;
        SuggestionBadgeText = suggestion.BadgeText;
    }

    private void ClearSuggestion()
    {
        SuggestedTaskId = null;
        SuggestedTaskTitle = "No suggestion yet.";
        SuggestedTaskScore = 0;
        SuggestionPanelTitle = "Best Next";
        SuggestionPanelText = "Load focus options to see the next small action.";
        SuggestionReasonText = "No focus options loaded.";
        SuggestionBadgeText = "Waiting";
    }

    private static StartWorkSuggestion? ChooseBestNextAction(IReadOnlyList<TaskItem> focusItems, DateOnly today)
    {
        if (focusItems.Count == 0)
        {
            return null;
        }

        return focusItems
            .Select(task => new StartWorkSuggestion(
                task,
                CalculateSuggestionScore(task, today),
                BuildSuggestionBadgeText(task, today),
                BuildSuggestionReasonText(task, today)))
            .OrderByDescending(suggestion => suggestion.Score)
            .ThenBy(suggestion => FocusSortDate(suggestion.Task, today))
            .ThenBy(suggestion => PrioritySortValue(suggestion.Task.Priority))
            .ThenBy(suggestion => suggestion.Task.Title, StringComparer.OrdinalIgnoreCase)
            .First();
    }

    private static int CalculateSuggestionScore(TaskItem task, DateOnly today)
    {
        int score = 0;

        if (task.Status == TaskItemStatus.InProgress)
        {
            score += 100;
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            score += 70 + Math.Min(14, today.DayNumber - task.DueDate.Value.DayNumber);
        }
        else if (task.DueDate.HasValue && task.DueDate.Value == today)
        {
            score += 60;
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            score += 40;
        }

        score += task.Priority switch
        {
            TaskPriority.Critical => 30,
            TaskPriority.High => 20,
            TaskPriority.Normal => 10,
            TaskPriority.Low => 3,
            _ => 0
        };

        if (task.IsTinyStep)
        {
            score += 20;
        }

        if (task.EstimatedMinutes.HasValue)
        {
            score += task.EstimatedMinutes.Value switch
            {
                <= FocusSession.DefaultPlannedMinutes => 15,
                <= FocusSession.DefaultPlannedMinutes * 2 => 5,
                _ => -10
            };
        }
        else
        {
            score += 5;
        }

        if (string.Equals(task.EnergyLevel, "Low", StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        return score;
    }

    private static string BuildSuggestionBadgeText(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "Continue";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "Overdue";
        }

        if (task.DueDate.HasValue && task.DueDate.Value == today)
        {
            return "Due Today";
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected Today";
        }

        if (task.IsTinyStep)
        {
            return "Tiny Step";
        }

        return "Ready";
    }

    private static string BuildSuggestionReasonText(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "Already in progress, so continuing is the cleanest next step.";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "Overdue and ready now; one short focus block can move it forward.";
        }

        if (task.DueDate.HasValue && task.DueDate.Value == today)
        {
            return "Due today, so it is the clearest next commitment.";
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected for today, so it belongs in this focus block.";
        }

        if (task.IsTinyStep)
        {
            return "Tiny enough to begin without extra preparation.";
        }

        return "Ready now and small enough to begin.";
    }

    private static int StatusSortValue(TaskItemStatus status)
    {
        return status switch
        {
            TaskItemStatus.InProgress => 0,
            TaskItemStatus.Planned => 1,
            TaskItemStatus.Captured => 2,
            _ => 3
        };
    }

    private static DateOnly FocusSortDate(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return DateOnly.MinValue;
        }

        DateOnly sortDate = DateOnly.MaxValue;
        if (task.DueDate.HasValue && task.DueDate.Value <= today)
        {
            sortDate = task.DueDate.Value;
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today && task.StartDate.Value < sortDate)
        {
            sortDate = task.StartDate.Value;
        }

        return sortDate;
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
        TaskCountText = TaskOptions.Count == 1 ? "1 option" : $"{TaskOptions.Count} options";
        OnPropertyChanged(nameof(HasTaskOptions));
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

    private sealed record StartWorkSuggestion(
        TaskItem Task,
        int Score,
        string BadgeText,
        string ReasonText);
}

public sealed class StartWorkTaskOptionViewModel
{
    private readonly Func<StartWorkTaskOptionViewModel, Task> _selectTaskAsync;

    private StartWorkTaskOptionViewModel(
        TaskItem task,
        DateOnly today,
        Func<StartWorkTaskOptionViewModel, Task>? selectTaskAsync,
        bool isSuggestedAction)
    {
        _selectTaskAsync = selectTaskAsync ?? (_ => Task.CompletedTask);
        Id = task.Id;
        Title = task.Title;
        Notes = task.Notes;
        Status = task.Status;
        Priority = task.Priority;
        DueDate = task.DueDate;
        StartDate = task.StartDate;
        EstimatedMinutes = task.EstimatedMinutes;
        EnergyLevel = task.EnergyLevel;
        IsTinyStep = task.IsTinyStep;
        IsOverdue = task.DueDate.HasValue && task.DueDate.Value < today;
        IsDueToday = task.DueDate.HasValue && task.DueDate.Value == today;
        IsSelectedForToday = task.StartDate.HasValue && task.StartDate.Value <= today;
        IsInProgress = task.Status == TaskItemStatus.InProgress;
        IsSuggestedAction = isSuggestedAction;
        HasNotes = !string.IsNullOrWhiteSpace(task.Notes);
        NotesPreview = HasNotes ? BuildNotesPreview(task.Notes) : string.Empty;
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
        EstimateText = task.EstimatedMinutes.HasValue ? $"{task.EstimatedMinutes.Value} min" : "No estimate";
        SuggestionBadgeText = isSuggestedAction ? "Suggested" : string.Empty;
        FocusBadgeText = BuildFocusBadgeText(task, today);
        FocusReasonText = BuildFocusReasonText(task, today);
        CardAccentColor = BuildCardAccentColor(task, today);
        CardBorderColor = BuildCardBorderColor(task, today, isSuggestedAction);
        CardIconGlyph = BuildCardIconGlyph(task, today);
        CardToolTip = $"{task.Title} - {FocusReasonText} - {StatusText}";
        SelectCommand = new AsyncRelayCommand(SelectTaskAsync);
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Notes { get; }

    public string NotesPreview { get; }

    public TaskItemStatus Status { get; }

    public TaskPriority Priority { get; }

    public DateOnly? DueDate { get; }

    public DateOnly? StartDate { get; }

    public int? EstimatedMinutes { get; }

    public string EstimateText { get; }

    public string EnergyLevel { get; }

    public bool IsTinyStep { get; }

    public bool IsOverdue { get; }

    public bool IsDueToday { get; }

    public bool IsSelectedForToday { get; }

    public bool IsInProgress { get; }

    public bool IsSuggestedAction { get; }

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

    public string SuggestionBadgeText { get; }

    public string FocusBadgeText { get; }

    public string FocusReasonText { get; }

    public string CardAccentColor { get; }

    public string CardBorderColor { get; }

    public string CardIconGlyph { get; }

    public string CardToolTip { get; }

    public AsyncRelayCommand SelectCommand { get; }

    public static StartWorkTaskOptionViewModel FromTask(
        TaskItem task,
        DateOnly today,
        Func<StartWorkTaskOptionViewModel, Task>? selectTaskAsync = null,
        bool isSuggestedAction = false)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new StartWorkTaskOptionViewModel(task, today, selectTaskAsync, isSuggestedAction);
    }

    private Task SelectTaskAsync()
    {
        return _selectTaskAsync(this);
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

        return "Ready for focus";
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
            TaskItemStatus.Planned => "#1E6B3A",
            TaskItemStatus.Captured => "#245EC9",
            _ => "#5F656C"
        };
    }

    private static string BuildFocusBadgeText(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "Keep going";
        }

        if (task.IsTinyStep)
        {
            return "Tiny step";
        }

        if (task.DueDate.HasValue && task.DueDate.Value <= today)
        {
            return "Ready now";
        }

        return "Ready";
    }

    private static string BuildFocusReasonText(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "Already in progress";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "Overdue and ready for focus";
        }

        if (task.DueDate.HasValue && task.DueDate.Value == today)
        {
            return "Due today";
        }

        if (task.StartDate.HasValue && task.StartDate.Value <= today)
        {
            return "Selected for today";
        }

        return "Ready for focus";
    }

    private static string BuildNotesPreview(string notes)
    {
        string trimmed = notes.Trim();
        return trimmed.Length <= 120 ? trimmed : $"{trimmed[..117]}...";
    }

    private static string BuildCardAccentColor(TaskItem task, DateOnly today)
    {
        if (task.Status == TaskItemStatus.InProgress)
        {
            return "#8DDAD5";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "#FFBE7A";
        }

        if (task.Priority is TaskPriority.Critical or TaskPriority.High)
        {
            return "#FFE08A";
        }

        return "#A8E6B1";
    }

    private static string BuildCardBorderColor(TaskItem task, DateOnly today, bool isSuggestedAction)
    {
        if (isSuggestedAction)
        {
            return "#65BFB8";
        }

        if (task.Status == TaskItemStatus.InProgress)
        {
            return "#B7DEDB";
        }

        if (task.DueDate.HasValue && task.DueDate.Value < today)
        {
            return "#E6BE95";
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

        if (task.IsTinyStep)
        {
            return "\uE73E";
        }

        return "\uE768";
    }
}
