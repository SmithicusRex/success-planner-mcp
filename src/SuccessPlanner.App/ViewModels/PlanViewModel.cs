using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class PlanViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string ReadyStatus = "Ready to plan.";
    private const string NoPlanDateMessage = "No plan date selected.";
    private const string NoProjectMessage = "No project selected.";
    private readonly Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadInboxTasksAsync;
    private readonly Func<DateOnly> _todayProvider;
    private readonly Dictionary<Guid, TaskItem> _loadedTasksById = [];
    private string _statusText = ReadyStatus;
    private string _planPanelTitle = "Plan Small";
    private string _planPanelText = "Turn a loose capture into one realistic next action.";
    private string _inboxStatusText = "Unplanned inbox not loaded yet.";
    private string _selectedInboxItemText = "No inbox item selected.";
    private string _planningStatusText = "No planning changes yet.";
    private string _minimumWinText = "No minimum win selected.";
    private string _saveStatusText = "Plan is local-first and not saved yet.";
    private string _emptyStateText = "Load unplanned inbox next.";
    private string _inboxCountText = "0 unplanned";
    private string _selectedInboxItemTitle = string.Empty;
    private string _dateHintText = NoPlanDateMessage;
    private string _projectName = string.Empty;
    private string _projectText = NoProjectMessage;
    private string _minimumWinDraft = string.Empty;
    private TaskPriority? _selectedPriority;
    private DateOnly? _selectedDueDate;
    private Guid? _selectedInboxItemId;
    private bool _isLoading;
    private bool _hasPlanningChanges;

    public PlanViewModel()
        : this(_ => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public PlanViewModel(
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadInboxTasksAsync,
        Func<DateOnly>? todayProvider = null)
        : base(ScreenCatalog.Plan)
    {
        ArgumentNullException.ThrowIfNull(loadInboxTasksAsync);
        _loadInboxTasksAsync = loadInboxTasksAsync;
        _todayProvider = todayProvider ?? (() => DateOnly.FromDateTime(DateTime.Today));
        RefreshCommand = new AsyncRelayCommand(
            () => LoadInboxAsync(CancellationToken.None),
            () => !IsLoading);
        ChooseLowPriorityCommand = new AsyncRelayCommand(
            () => ChoosePriorityAsync(TaskPriority.Low),
            CanUsePlanningControls);
        ChooseNormalPriorityCommand = new AsyncRelayCommand(
            () => ChoosePriorityAsync(TaskPriority.Normal),
            CanUsePlanningControls);
        ChooseHighPriorityCommand = new AsyncRelayCommand(
            () => ChoosePriorityAsync(TaskPriority.High),
            CanUsePlanningControls);
        ChooseCriticalPriorityCommand = new AsyncRelayCommand(
            () => ChoosePriorityAsync(TaskPriority.Critical),
            CanUsePlanningControls);
        ClearDateCommand = new AsyncRelayCommand(
            () => ChoosePlanDateAsync(null, string.Empty),
            CanUsePlanningControls);
        TodayDateCommand = new AsyncRelayCommand(
            () => ChoosePlanDateAsync(_todayProvider(), "Today"),
            CanUsePlanningControls);
        TomorrowDateCommand = new AsyncRelayCommand(
            () => ChoosePlanDateAsync(_todayProvider().AddDays(1), "Tomorrow"),
            CanUsePlanningControls);
        ThisWeekDateCommand = new AsyncRelayCommand(
            () => ChoosePlanDateAsync(_todayProvider().AddDays(7), "This week"),
            CanUsePlanningControls);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ObservableCollection<PlanInboxTaskViewModel> InboxItems { get; } = [];

    public ObservableCollection<PlanInboxTaskViewModel> Tasks => InboxItems;

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand ChooseLowPriorityCommand { get; }

    public AsyncRelayCommand ChooseNormalPriorityCommand { get; }

    public AsyncRelayCommand ChooseHighPriorityCommand { get; }

    public AsyncRelayCommand ChooseCriticalPriorityCommand { get; }

    public AsyncRelayCommand ClearDateCommand { get; }

    public AsyncRelayCommand TodayDateCommand { get; }

    public AsyncRelayCommand TomorrowDateCommand { get; }

    public AsyncRelayCommand ThisWeekDateCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string PlanPanelTitle
    {
        get => _planPanelTitle;
        private set => SetProperty(ref _planPanelTitle, value);
    }

    public string PlanPanelText
    {
        get => _planPanelText;
        private set => SetProperty(ref _planPanelText, value);
    }

    public string InboxStatusText
    {
        get => _inboxStatusText;
        private set => SetProperty(ref _inboxStatusText, value);
    }

    public string SelectedInboxItemText
    {
        get => _selectedInboxItemText;
        private set => SetProperty(ref _selectedInboxItemText, value);
    }

    public string PlanningStatusText
    {
        get => _planningStatusText;
        private set => SetProperty(ref _planningStatusText, value);
    }

    public string MinimumWinText
    {
        get => _minimumWinText;
        private set => SetProperty(ref _minimumWinText, value);
    }

    public string SaveStatusText
    {
        get => _saveStatusText;
        private set => SetProperty(ref _saveStatusText, value);
    }

    public string EmptyStateText
    {
        get => _emptyStateText;
        private set => SetProperty(ref _emptyStateText, value);
    }

    public string InboxCountText
    {
        get => _inboxCountText;
        private set => SetProperty(ref _inboxCountText, value);
    }

    public TaskPriority? SelectedPriority
    {
        get => _selectedPriority;
        private set
        {
            if (SetProperty(ref _selectedPriority, value))
            {
                OnPropertyChanged(nameof(PriorityText));
                OnPropertyChanged(nameof(IsLowPrioritySelected));
                OnPropertyChanged(nameof(IsNormalPrioritySelected));
                OnPropertyChanged(nameof(IsHighPrioritySelected));
                OnPropertyChanged(nameof(IsCriticalPrioritySelected));
            }
        }
    }

    public string PriorityText => SelectedPriority.HasValue
        ? $"{BuildPriorityText(SelectedPriority.Value)} priority"
        : "No priority selected.";

    public bool IsLowPrioritySelected => SelectedPriority == TaskPriority.Low;

    public bool IsNormalPrioritySelected => SelectedPriority == TaskPriority.Normal;

    public bool IsHighPrioritySelected => SelectedPriority == TaskPriority.High;

    public bool IsCriticalPrioritySelected => SelectedPriority == TaskPriority.Critical;

    public DateOnly? SelectedDueDate
    {
        get => _selectedDueDate;
        private set => SetProperty(ref _selectedDueDate, value);
    }

    public string DateHintText
    {
        get => _dateHintText;
        private set => SetProperty(ref _dateHintText, value);
    }

    public string ProjectName
    {
        get => _projectName;
        set => SetProjectName(value, updatePlanningState: true);
    }

    public string ProjectText
    {
        get => _projectText;
        private set => SetProperty(ref _projectText, value);
    }

    public bool HasProjectName => !string.IsNullOrWhiteSpace(ProjectName);

    public string MinimumWinDraft
    {
        get => _minimumWinDraft;
        set => SetMinimumWinDraft(value, updatePlanningState: true);
    }

    public bool HasMinimumWin => !string.IsNullOrWhiteSpace(MinimumWinDraft);

    public Guid? SelectedInboxItemId
    {
        get => _selectedInboxItemId;
        private set
        {
            if (SetProperty(ref _selectedInboxItemId, value))
            {
                OnPropertyChanged(nameof(HasSelectedInboxItem));
                OnPropertyChanged(nameof(HasPlanningControls));
                OnPropertyChanged(nameof(CanSavePlan));
                RaisePlanningCommandStatesChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
                RaisePlanningCommandStatesChanged();
            }
        }
    }

    public bool HasInboxItems => InboxItems.Count > 0;

    public bool HasSelectedInboxItem => SelectedInboxItemId.HasValue;

    public bool HasPlanningControls => HasSelectedInboxItem;

    public bool HasPlanningChanges => _hasPlanningChanges;

    public bool CanSavePlan => HasSelectedInboxItem && HasPlanningChanges && HasMinimumWin;

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return LoadInboxAsync(cancellationToken);
    }

    public async Task LoadInboxAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsLoading = true;
        StatusText = "Loading plan inbox.";
        InboxStatusText = "Loading unplanned items.";

        try
        {
            IReadOnlyList<TaskItem> loadedTasks = await _loadInboxTasksAsync(cancellationToken);
            IReadOnlyList<TaskItem> inboxItems = loadedTasks
                .Where(ShouldShowTask)
                .OrderBy(task => PrioritySortValue(task.Priority))
                .ThenBy(task => task.CreatedAt)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
            IReadOnlyList<PlanInboxTaskViewModel> inboxCards = inboxItems
                .Select(task => PlanInboxTaskViewModel.FromTask(task, SelectInboxItemAsync))
                .ToList();

            _loadedTasksById.Clear();
            foreach (TaskItem task in inboxItems)
            {
                _loadedTasksById[task.Id] = task;
            }

            InboxItems.Clear();
            foreach (PlanInboxTaskViewModel card in inboxCards)
            {
                InboxItems.Add(card);
            }

            if (SelectedInboxItemId.HasValue && !_loadedTasksById.ContainsKey(SelectedInboxItemId.Value))
            {
                ClearSelection();
            }
            else if (!SelectedInboxItemId.HasValue)
            {
                ApplyUnselectedPlanningState();
            }

            UpdateInboxSummary();
            StatusText = HasInboxItems ? "Inbox ready." : "Inbox is clear.";
            EmptyStateText = HasInboxItems
                ? "Choose one loose capture to plan."
                : "Capture a loose thought, then come back to Plan.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            _loadedTasksById.Clear();
            InboxItems.Clear();
            ClearSelection();
            UpdateInboxSummary();
            StatusText = "Plan could not load.";
            InboxStatusText = "Unplanned inbox could not load.";
            EmptyStateText = "Try Refresh, or return Home and open Plan again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SelectInboxItem(PlanInboxTaskViewModel inboxItem)
    {
        ArgumentNullException.ThrowIfNull(inboxItem);

        _selectedInboxItemTitle = inboxItem.Title;
        SelectedInboxItemId = inboxItem.Id;
        SelectedInboxItemText = $"Selected: {inboxItem.Title}";
        SelectedPriority = inboxItem.Priority;
        SelectedDueDate = null;
        DateHintText = NoPlanDateMessage;
        SetProjectName(string.Empty, updatePlanningState: false);
        SetMinimumWinDraft(string.Empty, updatePlanningState: false);
        SetPlanningChanges(false);
        PlanningStatusText = $"Ready to plan {inboxItem.Title}.";
        MinimumWinText = $"Minimum win pending for {inboxItem.Title}.";
        SaveStatusText = "Planning changes not saved yet.";
        StatusText = "Inbox item selected.";
    }

    public static bool ShouldShowTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.Status == TaskItemStatus.Captured
            && !task.DueDate.HasValue
            && !task.StartDate.HasValue
            && !task.ProjectId.HasValue;
    }

    private Task SelectInboxItemAsync(PlanInboxTaskViewModel inboxItem)
    {
        SelectInboxItem(inboxItem);
        return Task.CompletedTask;
    }

    private Task ChoosePriorityAsync(TaskPriority priority)
    {
        if (!HasSelectedInboxItem)
        {
            return Task.CompletedTask;
        }

        SelectedPriority = priority;
        MarkPlanningChanged($"{BuildPriorityText(priority)} priority selected.");
        return Task.CompletedTask;
    }

    private Task ChoosePlanDateAsync(DateOnly? dueDate, string label)
    {
        if (!HasSelectedInboxItem)
        {
            return Task.CompletedTask;
        }

        SelectedDueDate = dueDate;
        DateHintText = dueDate.HasValue
            ? $"{label}: {dueDate.Value:MMM d}"
            : NoPlanDateMessage;
        MarkPlanningChanged(dueDate.HasValue ? $"Plan date set for {label.ToLowerInvariant()}." : "Plan date cleared.");
        return Task.CompletedTask;
    }

    private bool CanUsePlanningControls()
    {
        return HasSelectedInboxItem && !IsLoading;
    }

    private void ClearSelection()
    {
        _selectedInboxItemTitle = string.Empty;
        SelectedInboxItemId = null;
        SelectedPriority = null;
        SelectedDueDate = null;
        DateHintText = NoPlanDateMessage;
        SetProjectName(string.Empty, updatePlanningState: false);
        SetMinimumWinDraft(string.Empty, updatePlanningState: false);
        SetPlanningChanges(false);
        SelectedInboxItemText = "No inbox item selected.";
        ApplyUnselectedPlanningState();
    }

    private void ApplyUnselectedPlanningState()
    {
        PlanningStatusText = HasInboxItems
            ? "Choose an inbox item to start planning."
            : "No unplanned items to plan.";
        MinimumWinText = "No minimum win selected.";
        SaveStatusText = "Plan is local-first and not saved yet.";
        ProjectText = NoProjectMessage;
    }

    private void UpdateInboxSummary()
    {
        InboxCountText = InboxItems.Count == 1 ? "1 unplanned" : $"{InboxItems.Count} unplanned";
        InboxStatusText = InboxItems.Count == 1
            ? "1 unplanned item ready."
            : HasInboxItems
                ? $"{InboxItems.Count} unplanned items ready."
                : "No unplanned inbox items.";
        OnPropertyChanged(nameof(HasInboxItems));
    }

    private void SetProjectName(string value, bool updatePlanningState)
    {
        if (!SetProperty(ref _projectName, value))
        {
            return;
        }

        OnPropertyChanged(nameof(HasProjectName));
        ProjectText = string.IsNullOrWhiteSpace(ProjectName)
            ? NoProjectMessage
            : $"Project: {ProjectName.Trim()}";

        if (updatePlanningState && HasSelectedInboxItem)
        {
            MarkPlanningChanged("Project updated.");
        }
    }

    private void SetMinimumWinDraft(string value, bool updatePlanningState)
    {
        if (!SetProperty(ref _minimumWinDraft, value))
        {
            return;
        }

        OnPropertyChanged(nameof(HasMinimumWin));
        OnPropertyChanged(nameof(CanSavePlan));
        UpdateMinimumWinText();

        if (updatePlanningState && HasSelectedInboxItem)
        {
            MarkPlanningChanged("Minimum win updated.");
        }
    }

    private void UpdateMinimumWinText()
    {
        if (!HasSelectedInboxItem)
        {
            MinimumWinText = "No minimum win selected.";
            return;
        }

        MinimumWinText = string.IsNullOrWhiteSpace(MinimumWinDraft)
            ? $"Minimum win pending for {_selectedInboxItemTitle}."
            : $"Minimum win: {MinimumWinDraft.Trim()}";
    }

    private void MarkPlanningChanged(string statusText)
    {
        SetPlanningChanges(true);
        PlanningStatusText = BuildPlanningStatusText();
        UpdateSaveStatus();
        StatusText = statusText;
    }

    private string BuildPlanningStatusText()
    {
        if (!HasSelectedInboxItem)
        {
            return "No planning changes yet.";
        }

        return $"{_selectedInboxItemTitle}: {PriorityText}; {DateHintText}; {ProjectText}.";
    }

    private void UpdateSaveStatus()
    {
        if (!HasSelectedInboxItem)
        {
            SaveStatusText = "Plan is local-first and not saved yet.";
            return;
        }

        SaveStatusText = HasPlanningChanges
            ? HasMinimumWin
                ? "Draft ready for local save."
                : "Add a minimum win before saving."
            : "Planning changes not saved yet.";
    }

    private void SetPlanningChanges(bool value)
    {
        if (_hasPlanningChanges == value)
        {
            OnPropertyChanged(nameof(CanSavePlan));
            return;
        }

        _hasPlanningChanges = value;
        OnPropertyChanged(nameof(HasPlanningChanges));
        OnPropertyChanged(nameof(CanSavePlan));
    }

    private void RaisePlanningCommandStatesChanged()
    {
        ChooseLowPriorityCommand.RaiseCanExecuteChanged();
        ChooseNormalPriorityCommand.RaiseCanExecuteChanged();
        ChooseHighPriorityCommand.RaiseCanExecuteChanged();
        ChooseCriticalPriorityCommand.RaiseCanExecuteChanged();
        ClearDateCommand.RaiseCanExecuteChanged();
        TodayDateCommand.RaiseCanExecuteChanged();
        TomorrowDateCommand.RaiseCanExecuteChanged();
        ThisWeekDateCommand.RaiseCanExecuteChanged();
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

public sealed class PlanInboxTaskViewModel
{
    private readonly Func<PlanInboxTaskViewModel, Task> _selectAsync;

    private PlanInboxTaskViewModel(
        TaskItem task,
        Func<PlanInboxTaskViewModel, Task>? selectAsync)
    {
        _selectAsync = selectAsync ?? (_ => Task.CompletedTask);
        Id = task.Id;
        Title = task.Title;
        Notes = task.Notes;
        Priority = task.Priority;
        Status = task.Status;
        CreatedAt = task.CreatedAt;
        HasNotes = !string.IsNullOrWhiteSpace(task.Notes);
        NotesPreview = HasNotes ? BuildNotesPreview(task.Notes) : string.Empty;
        CreatedText = $"Captured {task.CreatedAt.LocalDateTime:MMM d, h:mm tt}";
        PriorityText = BuildPriorityText(task.Priority);
        PriorityBadgeText = PriorityText;
        PriorityBadgeBackground = BuildPriorityBadgeBackground(task.Priority);
        PriorityBadgeForeground = BuildPriorityBadgeForeground(task.Priority);
        StatusText = "Unplanned";
        StatusBadgeText = StatusText;
        StatusBadgeBackground = "#EEF6FF";
        StatusBadgeForeground = "#245EC9";
        CardAccentColor = BuildCardAccentColor(task.Priority);
        CardBorderColor = BuildCardBorderColor(task.Priority);
        CardIconGlyph = "\uE9D5";
        CardToolTip = HasNotes
            ? $"{task.Title} - {NotesPreview}"
            : $"{task.Title} - ready to plan";
        SelectCommand = new AsyncRelayCommand(SelectAsync);
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Notes { get; }

    public string NotesPreview { get; }

    public TaskPriority Priority { get; }

    public TaskItemStatus Status { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool HasNotes { get; }

    public string CreatedText { get; }

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

    public AsyncRelayCommand SelectCommand { get; }

    public static PlanInboxTaskViewModel FromTask(
        TaskItem task,
        Func<PlanInboxTaskViewModel, Task>? selectAsync = null)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new PlanInboxTaskViewModel(task, selectAsync);
    }

    private Task SelectAsync()
    {
        return _selectAsync(this);
    }

    private static string BuildNotesPreview(string notes)
    {
        string trimmed = notes.Trim();
        return trimmed.Length <= 120 ? trimmed : $"{trimmed[..117]}...";
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

    private static string BuildCardAccentColor(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "#F8A8A8",
            TaskPriority.High => "#FFE08A",
            TaskPriority.Low => "#9DCCFF",
            _ => "#C8B6FF"
        };
    }

    private static string BuildCardBorderColor(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "#EAB1B1",
            TaskPriority.High => "#E4CD75",
            TaskPriority.Low => "#B7D8FF",
            _ => "#D7DADF"
        };
    }
}
