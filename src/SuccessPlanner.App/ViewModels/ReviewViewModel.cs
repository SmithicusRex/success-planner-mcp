using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class ReviewViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private readonly Func<CancellationToken, Task<IReadOnlyList<NoteItem>>> _loadSmallWinsAsync;
    private readonly Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadReviewTasksAsync;
    private readonly Func<CancellationToken, Task<IReadOnlyList<FocusSession>>> _loadFocusWinsAsync;
    private readonly Func<CancellationToken, Task<IReadOnlyList<MovementSession>>> _loadMovementWinsAsync;
    private readonly Func<ReviewNextFocusSelection, CancellationToken, Task> _saveNextFocusAsync;
    private string _statusText = "Ready to review.";
    private string _reviewPanelTitle = "Review Gently";
    private string _reviewPanelText = "Notice small wins, stuck places, and one realistic next focus.";
    private string _weekSummaryText = "Week summary not loaded yet.";
    private string _smallWinsText = "Small wins not loaded yet.";
    private string _stuckItemsText = "Stuck items not loaded yet.";
    private string _needsDecisionText = "Needs-decision items not loaded yet.";
    private string _nextFocusText = "No next focus selected.";
    private string _saveReviewStatusText = "Review is local-first and not saved yet.";
    private string _emptyStateText = "Review will show progress after local activity is loaded.";
    private string _reviewCountText = "0 review items";
    private ReviewNextFocusSelection? _selectedNextFocus;
    private Guid? _lastSavedNextFocusId;
    private bool _isLoadingReview;
    private bool _isSavingReview;
    private bool _hasSavedNextFocus;

    public ReviewViewModel()
        : this(
            _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
            _ => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public ReviewViewModel(Func<CancellationToken, Task<IReadOnlyList<NoteItem>>> loadSmallWinsAsync)
        : this(loadSmallWinsAsync, _ => Task.FromResult<IReadOnlyList<TaskItem>>([]))
    {
    }

    public ReviewViewModel(
        Func<CancellationToken, Task<IReadOnlyList<NoteItem>>> loadSmallWinsAsync,
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadReviewTasksAsync)
        : this(loadSmallWinsAsync, loadReviewTasksAsync, MissingNextFocusSaveAsync)
    {
    }

    public ReviewViewModel(
        Func<CancellationToken, Task<IReadOnlyList<NoteItem>>> loadSmallWinsAsync,
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadReviewTasksAsync,
        Func<ReviewNextFocusSelection, CancellationToken, Task> saveNextFocusAsync)
        : this(
            loadSmallWinsAsync,
            loadReviewTasksAsync,
            _ => Task.FromResult<IReadOnlyList<FocusSession>>([]),
            _ => Task.FromResult<IReadOnlyList<MovementSession>>([]),
            saveNextFocusAsync)
    {
    }

    public ReviewViewModel(
        Func<CancellationToken, Task<IReadOnlyList<NoteItem>>> loadSmallWinsAsync,
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadReviewTasksAsync,
        Func<CancellationToken, Task<IReadOnlyList<FocusSession>>> loadFocusWinsAsync,
        Func<CancellationToken, Task<IReadOnlyList<MovementSession>>> loadMovementWinsAsync,
        Func<ReviewNextFocusSelection, CancellationToken, Task> saveNextFocusAsync)
        : base(ScreenCatalog.Review)
    {
        ArgumentNullException.ThrowIfNull(loadSmallWinsAsync);
        ArgumentNullException.ThrowIfNull(loadReviewTasksAsync);
        ArgumentNullException.ThrowIfNull(loadFocusWinsAsync);
        ArgumentNullException.ThrowIfNull(loadMovementWinsAsync);
        ArgumentNullException.ThrowIfNull(saveNextFocusAsync);
        _loadSmallWinsAsync = loadSmallWinsAsync;
        _loadReviewTasksAsync = loadReviewTasksAsync;
        _loadFocusWinsAsync = loadFocusWinsAsync;
        _loadMovementWinsAsync = loadMovementWinsAsync;
        _saveNextFocusAsync = saveNextFocusAsync;
        RefreshCommand = new AsyncRelayCommand(
            () => LoadReviewAsync(CancellationToken.None),
            () => !IsLoadingReview);
        SaveReviewCommand = new AsyncRelayCommand(
            () => SaveReviewAsync(CancellationToken.None),
            () => CanSaveReview);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ObservableCollection<ReviewSmallWinViewModel> SmallWins { get; } = [];

    public ObservableCollection<ReviewStuckItemViewModel> StuckItems { get; } = [];

    public ObservableCollection<ReviewNeedsDecisionItemViewModel> NeedsDecisionItems { get; } = [];

    public AsyncRelayCommand RefreshCommand { get; }

    public AsyncRelayCommand SaveReviewCommand { get; }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ReviewPanelTitle
    {
        get => _reviewPanelTitle;
        private set => SetProperty(ref _reviewPanelTitle, value);
    }

    public string ReviewPanelText
    {
        get => _reviewPanelText;
        private set => SetProperty(ref _reviewPanelText, value);
    }

    public string WeekSummaryText
    {
        get => _weekSummaryText;
        private set => SetProperty(ref _weekSummaryText, value);
    }

    public string SmallWinsText
    {
        get => _smallWinsText;
        private set => SetProperty(ref _smallWinsText, value);
    }

    public string StuckItemsText
    {
        get => _stuckItemsText;
        private set => SetProperty(ref _stuckItemsText, value);
    }

    public string NeedsDecisionText
    {
        get => _needsDecisionText;
        private set => SetProperty(ref _needsDecisionText, value);
    }

    public string NextFocusText
    {
        get => _nextFocusText;
        private set => SetProperty(ref _nextFocusText, value);
    }

    public string SaveReviewStatusText
    {
        get => _saveReviewStatusText;
        private set => SetProperty(ref _saveReviewStatusText, value);
    }

    public string EmptyStateText
    {
        get => _emptyStateText;
        private set => SetProperty(ref _emptyStateText, value);
    }

    public string ReviewCountText
    {
        get => _reviewCountText;
        private set => SetProperty(ref _reviewCountText, value);
    }

    public bool IsLoadingReview
    {
        get => _isLoadingReview;
        private set
        {
            if (SetProperty(ref _isLoadingReview, value))
            {
                RefreshCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsSavingReview
    {
        get => _isSavingReview;
        private set
        {
            if (SetProperty(ref _isSavingReview, value))
            {
                OnPropertyChanged(nameof(CanSaveReview));
                SaveReviewCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasReviewData => HasSmallWins || HasStuckItems || HasNeedsDecisionItems;

    public bool HasSmallWins => SmallWins.Count > 0;

    public bool HasStuckItems => StuckItems.Count > 0;

    public bool HasNeedsDecisionItems => NeedsDecisionItems.Count > 0;

    public bool HasNextFocus => _selectedNextFocus is not null;

    public ReviewNextFocusKind? SelectedNextFocusKind => _selectedNextFocus?.Kind;

    public Guid? SelectedNextFocusId => _selectedNextFocus?.ItemId;

    public string SelectedNextFocusTitle => _selectedNextFocus?.Title ?? string.Empty;

    public string SelectedNextFocusSourceText => _selectedNextFocus?.SourceText ?? string.Empty;

    public Guid? LastSavedNextFocusId => _lastSavedNextFocusId;

    public bool HasSavedNextFocus
    {
        get => _hasSavedNextFocus;
        private set
        {
            if (SetProperty(ref _hasSavedNextFocus, value))
            {
                OnPropertyChanged(nameof(CanSaveReview));
                SaveReviewCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool CanSaveReview => HasNextFocus && !IsSavingReview && !HasSavedNextFocus;

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        return LoadReviewAsync(cancellationToken);
    }

    public async Task LoadReviewAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IsLoadingReview = true;
        StatusText = "Loading review.";
        SmallWinsText = "Loading small wins.";
        StuckItemsText = "Loading stuck items.";
        NeedsDecisionText = "Loading needs-decision items.";
        WeekSummaryText = "Loading local review highlights.";

        try
        {
            IReadOnlyList<NoteItem> loadedSmallWins = await _loadSmallWinsAsync(cancellationToken);
            IReadOnlyList<TaskItem> loadedReviewTasks = await _loadReviewTasksAsync(cancellationToken);
            IReadOnlyList<FocusSession> loadedFocusWins = await _loadFocusWinsAsync(cancellationToken);
            IReadOnlyList<MovementSession> loadedMovementWins = await _loadMovementWinsAsync(cancellationToken);
            IReadOnlyList<ReviewSmallWinViewModel> smallWinCards = loadedSmallWins
                .Where(note => note.IsReviewHighlight)
                .Select(note => ReviewSmallWinViewModel.FromNote(note, ChooseNextFocus))
                .Concat(loadedFocusWins
                    .Where(ShouldShowFocusWin)
                    .Select(session => ReviewSmallWinViewModel.FromFocusSession(session, ChooseNextFocus)))
                .Concat(loadedMovementWins
                    .Where(ShouldShowMovementSuccess)
                    .Select(session => ReviewSmallWinViewModel.FromMovementSession(session, ChooseNextFocus)))
                .OrderByDescending(card => card.CreatedAt)
                .ThenBy(card => card.Text, StringComparer.OrdinalIgnoreCase)
                .ToList();
            IReadOnlyList<ReviewStuckItemViewModel> stuckCards = loadedReviewTasks
                .Where(ShouldShowStuckItem)
                .OrderBy(task => StuckSortValue(task))
                .ThenBy(task => task.DueDate ?? task.StartDate ?? DateOnly.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => ReviewStuckItemViewModel.FromTask(task, ChooseNextFocus))
                .ToList();
            IReadOnlyList<ReviewNeedsDecisionItemViewModel> needsDecisionCards = loadedReviewTasks
                .Where(ShouldShowNeedsDecisionItem)
                .OrderBy(task => PrioritySortValue(task.Priority))
                .ThenBy(task => task.DueDate ?? task.StartDate ?? DateOnly.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(task => ReviewNeedsDecisionItemViewModel.FromTask(task, ChooseNextFocus))
                .ToList();

            SmallWins.Clear();
            foreach (ReviewSmallWinViewModel card in smallWinCards)
            {
                SmallWins.Add(card);
            }

            StuckItems.Clear();
            foreach (ReviewStuckItemViewModel card in stuckCards)
            {
                StuckItems.Add(card);
            }

            NeedsDecisionItems.Clear();
            foreach (ReviewNeedsDecisionItemViewModel card in needsDecisionCards)
            {
                NeedsDecisionItems.Add(card);
            }

            UpdateReviewSummary();
            StatusText = BuildLoadedStatusText();
            EmptyStateText = BuildLoadedEmptyStateText();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            SmallWins.Clear();
            StuckItems.Clear();
            NeedsDecisionItems.Clear();
            UpdateReviewSummary();
            StatusText = "Review could not load.";
            WeekSummaryText = "Review highlights could not load.";
            SmallWinsText = "Small wins could not load.";
            StuckItemsText = "Stuck items could not load.";
            NeedsDecisionText = "Needs-decision items could not load.";
            EmptyStateText = "Try Review again after checking local data.";
        }
        finally
        {
            IsLoadingReview = false;
        }
    }

    public void ChooseNextFocus(ReviewNextFocusSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);

        _selectedNextFocus = selection;
        _lastSavedNextFocusId = null;
        HasSavedNextFocus = false;
        NextFocusText = $"{selection.SourceText}: {selection.Title}";
        SaveReviewStatusText = "Next focus ready to save locally.";
        StatusText = "Next focus selected.";

        OnPropertyChanged(nameof(HasNextFocus));
        OnPropertyChanged(nameof(SelectedNextFocusKind));
        OnPropertyChanged(nameof(SelectedNextFocusId));
        OnPropertyChanged(nameof(SelectedNextFocusTitle));
        OnPropertyChanged(nameof(SelectedNextFocusSourceText));
        OnPropertyChanged(nameof(LastSavedNextFocusId));
        OnPropertyChanged(nameof(CanSaveReview));
        SaveReviewCommand.RaiseCanExecuteChanged();
    }

    public async Task SaveReviewAsync(CancellationToken cancellationToken = default)
    {
        if (_selectedNextFocus is null)
        {
            SaveReviewStatusText = "Choose one review item before saving.";
            StatusText = "Choose a next focus first.";
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        IsSavingReview = true;
        SaveReviewStatusText = "Saving next focus locally.";
        StatusText = "Saving review focus.";

        try
        {
            await _saveNextFocusAsync(_selectedNextFocus, cancellationToken);
            _lastSavedNextFocusId = _selectedNextFocus.ItemId;
            OnPropertyChanged(nameof(LastSavedNextFocusId));
            HasSavedNextFocus = true;
            SaveReviewStatusText = $"Saved locally: {_selectedNextFocus.Title}";
            StatusText = "Next focus saved.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            HasSavedNextFocus = false;
            SaveReviewStatusText = "Next focus could not save locally.";
            StatusText = "Review save failed.";
        }
        finally
        {
            IsSavingReview = false;
        }
    }

    private void UpdateReviewSummary()
    {
        int reviewCount = SmallWins.Count + StuckItems.Count + NeedsDecisionItems.Count;
        ReviewCountText = reviewCount == 1 ? "1 review item" : $"{reviewCount} review items";
        WeekSummaryText = BuildWeekSummaryText();
        SmallWinsText = SmallWins.Count == 1
            ? "1 small win ready."
            : HasSmallWins
                ? $"{SmallWins.Count} small wins ready."
                : "No small wins yet.";
        StuckItemsText = StuckItems.Count == 1
            ? "1 stuck item ready."
            : HasStuckItems
                ? $"{StuckItems.Count} stuck items ready."
                : "No stuck items yet.";
        NeedsDecisionText = NeedsDecisionItems.Count == 1
            ? "1 needs-decision item ready."
            : HasNeedsDecisionItems
                ? $"{NeedsDecisionItems.Count} needs-decision items ready."
                : "No needs-decision items yet.";
        OnPropertyChanged(nameof(HasReviewData));
        OnPropertyChanged(nameof(HasSmallWins));
        OnPropertyChanged(nameof(HasStuckItems));
        OnPropertyChanged(nameof(HasNeedsDecisionItems));
    }

    public static bool ShouldShowStuckItem(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.Status == TaskItemStatus.Blocked
            || HasTag(task, "Stuck")
            || HasTag(task, "Repeated Snooze");
    }

    public static bool ShouldShowNeedsDecisionItem(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.Status != TaskItemStatus.Done
            && (HasTag(task, "Needs Decision")
                || HasTag(task, "Decision Needed")
                || HasTag(task, "Decision")
                || HasTag(task, "NeedsDecision"));
    }

    public static bool ShouldShowFocusWin(FocusSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.Status == FocusSessionStatus.Completed;
    }

    public static bool ShouldShowMovementSuccess(MovementSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        return session.Status is MovementSessionStatus.Planned
            or MovementSessionStatus.Active
            or MovementSessionStatus.Completed;
    }

    private string BuildLoadedStatusText()
    {
        int categoryCount = CountLoadedCategories();
        if (categoryCount > 1)
        {
            return "Review ready.";
        }

        if (HasSmallWins)
        {
            return "Small wins ready.";
        }

        if (HasStuckItems)
        {
            return "Stuck items ready.";
        }

        if (HasNeedsDecisionItems)
        {
            return "Needs-decision items ready.";
        }

        return "No review items yet.";
    }

    private string BuildLoadedEmptyStateText()
    {
        int categoryCount = CountLoadedCategories();
        if (categoryCount > 1)
        {
            return "Review data is loaded from local activity.";
        }

        if (HasSmallWins)
        {
            return "Small wins are loaded from local completions.";
        }

        if (HasStuckItems)
        {
            return "Stuck items are loaded from local task status.";
        }

        if (HasNeedsDecisionItems)
        {
            return "Needs-decision items are loaded from local task tags.";
        }

        return "Complete one small task, then come back to Review.";
    }

    private string BuildWeekSummaryText()
    {
        if (!HasReviewData)
        {
            return "No local review items loaded yet.";
        }

        List<string> parts = [];
        if (HasSmallWins)
        {
            parts.Add(SmallWins.Count == 1 ? "1 small win" : $"{SmallWins.Count} small wins");
        }

        if (HasStuckItems)
        {
            parts.Add(StuckItems.Count == 1 ? "1 stuck item" : $"{StuckItems.Count} stuck items");
        }

        if (HasNeedsDecisionItems)
        {
            parts.Add(NeedsDecisionItems.Count == 1
                ? "1 needs-decision item"
                : $"{NeedsDecisionItems.Count} needs-decision items");
        }

        return $"{JoinReviewParts(parts)} this review.";
    }

    private static int StuckSortValue(TaskItem task)
    {
        if (task.Status == TaskItemStatus.Blocked)
        {
            return 0;
        }

        return HasTag(task, "Repeated Snooze") ? 1 : 2;
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

    private static bool HasTag(TaskItem task, string tag)
    {
        return task.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
    }

    private int CountLoadedCategories()
    {
        int count = 0;
        if (HasSmallWins)
        {
            count++;
        }

        if (HasStuckItems)
        {
            count++;
        }

        if (HasNeedsDecisionItems)
        {
            count++;
        }

        return count;
    }

    private static string JoinReviewParts(IReadOnlyList<string> parts)
    {
        return parts.Count switch
        {
            0 => string.Empty,
            1 => parts[0],
            2 => $"{parts[0]} and {parts[1]}",
            _ => $"{string.Join(", ", parts.Take(parts.Count - 1))}, and {parts[^1]}"
        };
    }

    private static Task MissingNextFocusSaveAsync(
        ReviewNextFocusSelection selection,
        CancellationToken cancellationToken)
    {
        return Task.FromException(new InvalidOperationException("No next-focus save repository is configured."));
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

public enum ReviewNextFocusKind
{
    SmallWin,
    StuckItem,
    NeedsDecision
}

public sealed record ReviewNextFocusSelection(
    ReviewNextFocusKind Kind,
    Guid ItemId,
    string Title,
    string SourceText,
    DateTimeOffset SelectedAt);

public static class ReviewNextFocusMetadataKeys
{
    public const string Kind = "review.next_focus.kind";
    public const string ItemId = "review.next_focus.item_id";
    public const string Title = "review.next_focus.title";
    public const string Source = "review.next_focus.source";
    public const string SelectedAt = "review.next_focus.selected_at";
}

public sealed class ReviewSmallWinViewModel
{
    private ReviewSmallWinViewModel(
        Guid id,
        NoteOwnerType ownerType,
        Guid? ownerId,
        string text,
        DateTimeOffset createdAt,
        string sourceText,
        Action<ReviewNextFocusSelection>? chooseNextFocus)
    {
        Id = id;
        OwnerType = ownerType;
        OwnerId = ownerId;
        Text = text;
        CreatedAt = createdAt;
        CreatedText = $"Recorded {createdAt.LocalDateTime:MMM d, h:mm tt}";
        SourceText = sourceText;
        BadgeText = "Small Win";
        CardIconGlyph = "\uE73E";
        CardAccentColor = "#A8E6B1";
        CardBorderColor = "#CDEAD5";
        CardToolTip = $"{BadgeText}: {Text}";
        HasSource = !string.IsNullOrWhiteSpace(SourceText);
        ChooseNextFocusCommand = new AsyncRelayCommand(
            () =>
            {
                chooseNextFocus?.Invoke(ToNextFocusSelection());
                return Task.CompletedTask;
            },
            () => chooseNextFocus is not null);
    }

    public Guid Id { get; }

    public NoteOwnerType OwnerType { get; }

    public Guid? OwnerId { get; }

    public string Text { get; }

    public DateTimeOffset CreatedAt { get; }

    public string CreatedText { get; }

    public string SourceText { get; }

    public bool HasSource { get; }

    public string BadgeText { get; }

    public string CardIconGlyph { get; }

    public string CardAccentColor { get; }

    public string CardBorderColor { get; }

    public string CardToolTip { get; }

    public AsyncRelayCommand ChooseNextFocusCommand { get; }

    public static ReviewSmallWinViewModel FromNote(NoteItem note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return CreateFromNote(note, null);
    }

    public static ReviewSmallWinViewModel FromNote(
        NoteItem note,
        Action<ReviewNextFocusSelection> chooseNextFocus)
    {
        ArgumentNullException.ThrowIfNull(note);
        ArgumentNullException.ThrowIfNull(chooseNextFocus);
        return CreateFromNote(note, chooseNextFocus);
    }

    public static ReviewSmallWinViewModel FromFocusSession(FocusSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return CreateFromFocusSession(session, null);
    }

    public static ReviewSmallWinViewModel FromFocusSession(
        FocusSession session,
        Action<ReviewNextFocusSelection> chooseNextFocus)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(chooseNextFocus);
        return CreateFromFocusSession(session, chooseNextFocus);
    }

    public static ReviewSmallWinViewModel FromMovementSession(MovementSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return CreateFromMovementSession(session, null);
    }

    public static ReviewSmallWinViewModel FromMovementSession(
        MovementSession session,
        Action<ReviewNextFocusSelection> chooseNextFocus)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(chooseNextFocus);
        return CreateFromMovementSession(session, chooseNextFocus);
    }

    public ReviewNextFocusSelection ToNextFocusSelection()
    {
        return new ReviewNextFocusSelection(
            ReviewNextFocusKind.SmallWin,
            Id,
            Text,
            SourceText,
            DateTimeOffset.UtcNow);
    }

    private static ReviewSmallWinViewModel CreateFromNote(
        NoteItem note,
        Action<ReviewNextFocusSelection>? chooseNextFocus)
    {
        return new ReviewSmallWinViewModel(
            note.Id,
            note.OwnerType,
            note.OwnerId,
            note.Text,
            note.CreatedAt,
            BuildSourceText(note),
            chooseNextFocus);
    }

    private static ReviewSmallWinViewModel CreateFromFocusSession(
        FocusSession session,
        Action<ReviewNextFocusSelection>? chooseNextFocus)
    {
        return new ReviewSmallWinViewModel(
            session.Id,
            NoteOwnerType.FocusSession,
            session.TaskId,
            BuildFocusWinText(session),
            session.CompletedAt ?? session.EndedAt ?? session.StartedAt,
            "Focus win",
            chooseNextFocus);
    }

    private static ReviewSmallWinViewModel CreateFromMovementSession(
        MovementSession session,
        Action<ReviewNextFocusSelection>? chooseNextFocus)
    {
        return new ReviewSmallWinViewModel(
            session.Id,
            NoteOwnerType.MovementSession,
            session.TaskId,
            BuildMovementSuccessText(session),
            session.CompletedAt ?? session.StartedAt ?? session.ScheduledFor ?? session.CreatedAt,
            "Movement win",
            chooseNextFocus);
    }

    private static string BuildSourceText(NoteItem note)
    {
        return note.OwnerType switch
        {
            NoteOwnerType.Task => "Task win",
            NoteOwnerType.FocusSession => "Focus win",
            NoteOwnerType.MovementSession => "Movement win",
            NoteOwnerType.Review => "Review win",
            _ => "Local win"
        };
    }

    private static string BuildFocusWinText(FocusSession session)
    {
        return string.IsNullOrWhiteSpace(session.WinNote)
            ? $"Completed {session.PlannedMinutes} minute focus: {session.Intention}"
            : session.WinNote;
    }

    private static string BuildMovementSuccessText(MovementSession session)
    {
        if (!string.IsNullOrWhiteSpace(session.WinNote))
        {
            return session.WinNote;
        }

        return session.Status switch
        {
            MovementSessionStatus.Completed => $"Movement completed: {session.ActivityName}",
            MovementSessionStatus.Active => $"Movement started: {session.ActivityName}",
            MovementSessionStatus.Planned => $"Movement planned: {session.ActivityName}",
            _ => $"Movement saved: {session.ActivityName}"
        };
    }
}

public sealed class ReviewStuckItemViewModel
{
    private ReviewStuckItemViewModel(
        TaskItem task,
        Action<ReviewNextFocusSelection>? chooseNextFocus)
    {
        Id = task.Id;
        Title = task.Title;
        Notes = task.Notes;
        Status = task.Status;
        Priority = task.Priority;
        DueDate = task.DueDate;
        StartDate = task.StartDate;
        CreatedAt = task.CreatedAt;
        HasNotes = !string.IsNullOrWhiteSpace(task.Notes);
        NotesPreview = HasNotes ? BuildNotesPreview(task.Notes) : string.Empty;
        StatusText = BuildStatusText(task);
        PriorityText = BuildPriorityText(task.Priority);
        DateText = BuildDateText(task);
        BadgeText = StatusText;
        CardIconGlyph = "\uE7BA";
        CardAccentColor = "#F8A8A8";
        CardBorderColor = "#F3CACA";
        CardToolTip = HasNotes
            ? $"{Title} - {NotesPreview}"
            : $"{Title} - {StatusText.ToLowerInvariant()}";
        ChooseNextFocusCommand = new AsyncRelayCommand(
            () =>
            {
                chooseNextFocus?.Invoke(ToNextFocusSelection());
                return Task.CompletedTask;
            },
            () => chooseNextFocus is not null);
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Notes { get; }

    public string NotesPreview { get; }

    public TaskItemStatus Status { get; }

    public TaskPriority Priority { get; }

    public DateOnly? DueDate { get; }

    public DateOnly? StartDate { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool HasNotes { get; }

    public string StatusText { get; }

    public string PriorityText { get; }

    public string DateText { get; }

    public string BadgeText { get; }

    public string CardIconGlyph { get; }

    public string CardAccentColor { get; }

    public string CardBorderColor { get; }

    public string CardToolTip { get; }

    public AsyncRelayCommand ChooseNextFocusCommand { get; }

    public static ReviewStuckItemViewModel FromTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new ReviewStuckItemViewModel(task, null);
    }

    public static ReviewStuckItemViewModel FromTask(
        TaskItem task,
        Action<ReviewNextFocusSelection> chooseNextFocus)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(chooseNextFocus);
        return new ReviewStuckItemViewModel(task, chooseNextFocus);
    }

    public ReviewNextFocusSelection ToNextFocusSelection()
    {
        return new ReviewNextFocusSelection(
            ReviewNextFocusKind.StuckItem,
            Id,
            Title,
            StatusText,
            DateTimeOffset.UtcNow);
    }

    private static string BuildStatusText(TaskItem task)
    {
        if (task.Status == TaskItemStatus.Blocked)
        {
            return "Blocked";
        }

        return task.Tags.Contains("Repeated Snooze", StringComparer.OrdinalIgnoreCase)
            ? "Repeated Snooze"
            : "Stuck";
    }

    private static string BuildPriorityText(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "Critical priority",
            TaskPriority.High => "High priority",
            TaskPriority.Normal => "Normal priority",
            TaskPriority.Low => "Low priority",
            _ => $"{priority} priority"
        };
    }

    private static string BuildDateText(TaskItem task)
    {
        if (task.DueDate.HasValue)
        {
            return $"Due {task.DueDate.Value:MMM d}";
        }

        return task.StartDate.HasValue
            ? $"Selected {task.StartDate.Value:MMM d}"
            : $"Created {task.CreatedAt.LocalDateTime:MMM d}";
    }

    private static string BuildNotesPreview(string notes)
    {
        string trimmed = notes.Trim();
        return trimmed.Length <= 120 ? trimmed : $"{trimmed[..117]}...";
    }
}

public sealed class ReviewNeedsDecisionItemViewModel
{
    private ReviewNeedsDecisionItemViewModel(
        TaskItem task,
        Action<ReviewNextFocusSelection>? chooseNextFocus)
    {
        Id = task.Id;
        Title = task.Title;
        Notes = task.Notes;
        Priority = task.Priority;
        DueDate = task.DueDate;
        StartDate = task.StartDate;
        CreatedAt = task.CreatedAt;
        HasNotes = !string.IsNullOrWhiteSpace(task.Notes);
        NotesPreview = HasNotes ? BuildNotesPreview(task.Notes) : string.Empty;
        PriorityText = BuildPriorityText(task.Priority);
        DateText = BuildDateText(task);
        BadgeText = "Needs Decision";
        CardIconGlyph = "\uE9CE";
        CardAccentColor = "#9DCCFF";
        CardBorderColor = "#B7D8FF";
        CardToolTip = HasNotes
            ? $"{Title} - {NotesPreview}"
            : $"{Title} - needs a decision";
        ChooseNextFocusCommand = new AsyncRelayCommand(
            () =>
            {
                chooseNextFocus?.Invoke(ToNextFocusSelection());
                return Task.CompletedTask;
            },
            () => chooseNextFocus is not null);
    }

    public Guid Id { get; }

    public string Title { get; }

    public string Notes { get; }

    public string NotesPreview { get; }

    public TaskPriority Priority { get; }

    public DateOnly? DueDate { get; }

    public DateOnly? StartDate { get; }

    public DateTimeOffset CreatedAt { get; }

    public bool HasNotes { get; }

    public string PriorityText { get; }

    public string DateText { get; }

    public string BadgeText { get; }

    public string CardIconGlyph { get; }

    public string CardAccentColor { get; }

    public string CardBorderColor { get; }

    public string CardToolTip { get; }

    public AsyncRelayCommand ChooseNextFocusCommand { get; }

    public static ReviewNeedsDecisionItemViewModel FromTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new ReviewNeedsDecisionItemViewModel(task, null);
    }

    public static ReviewNeedsDecisionItemViewModel FromTask(
        TaskItem task,
        Action<ReviewNextFocusSelection> chooseNextFocus)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(chooseNextFocus);
        return new ReviewNeedsDecisionItemViewModel(task, chooseNextFocus);
    }

    public ReviewNextFocusSelection ToNextFocusSelection()
    {
        return new ReviewNextFocusSelection(
            ReviewNextFocusKind.NeedsDecision,
            Id,
            Title,
            BadgeText,
            DateTimeOffset.UtcNow);
    }

    private static string BuildPriorityText(TaskPriority priority)
    {
        return priority switch
        {
            TaskPriority.Critical => "Critical priority",
            TaskPriority.High => "High priority",
            TaskPriority.Normal => "Normal priority",
            TaskPriority.Low => "Low priority",
            _ => $"{priority} priority"
        };
    }

    private static string BuildDateText(TaskItem task)
    {
        if (task.DueDate.HasValue)
        {
            return $"Due {task.DueDate.Value:MMM d}";
        }

        return task.StartDate.HasValue
            ? $"Selected {task.StartDate.Value:MMM d}"
            : $"Created {task.CreatedAt.LocalDateTime:MMM d}";
    }

    private static string BuildNotesPreview(string notes)
    {
        string trimmed = notes.Trim();
        return trimmed.Length <= 120 ? trimmed : $"{trimmed[..117]}...";
    }
}
