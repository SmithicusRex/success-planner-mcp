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
    private readonly Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> _loadStuckItemsAsync;
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
    private bool _isLoadingReview;

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
        Func<CancellationToken, Task<IReadOnlyList<TaskItem>>> loadStuckItemsAsync)
        : base(ScreenCatalog.Review)
    {
        ArgumentNullException.ThrowIfNull(loadSmallWinsAsync);
        ArgumentNullException.ThrowIfNull(loadStuckItemsAsync);
        _loadSmallWinsAsync = loadSmallWinsAsync;
        _loadStuckItemsAsync = loadStuckItemsAsync;
        RefreshCommand = new AsyncRelayCommand(
            () => LoadReviewAsync(CancellationToken.None),
            () => !IsLoadingReview);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public ObservableCollection<ReviewSmallWinViewModel> SmallWins { get; } = [];

    public ObservableCollection<ReviewStuckItemViewModel> StuckItems { get; } = [];

    public AsyncRelayCommand RefreshCommand { get; }

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

    public bool HasReviewData => HasSmallWins || HasStuckItems;

    public bool HasSmallWins => SmallWins.Count > 0;

    public bool HasStuckItems => StuckItems.Count > 0;

    public bool HasNeedsDecisionItems => false;

    public bool HasNextFocus => false;

    public bool CanSaveReview => false;

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
        WeekSummaryText = "Loading local review highlights.";

        try
        {
            IReadOnlyList<NoteItem> loadedSmallWins = await _loadSmallWinsAsync(cancellationToken);
            IReadOnlyList<TaskItem> loadedStuckItems = await _loadStuckItemsAsync(cancellationToken);
            IReadOnlyList<ReviewSmallWinViewModel> smallWinCards = loadedSmallWins
                .Where(note => note.IsReviewHighlight)
                .OrderByDescending(note => note.CreatedAt)
                .ThenBy(note => note.Text, StringComparer.OrdinalIgnoreCase)
                .Select(ReviewSmallWinViewModel.FromNote)
                .ToList();
            IReadOnlyList<ReviewStuckItemViewModel> stuckCards = loadedStuckItems
                .Where(ShouldShowStuckItem)
                .OrderBy(task => StuckSortValue(task))
                .ThenBy(task => task.DueDate ?? task.StartDate ?? DateOnly.MaxValue)
                .ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
                .Select(ReviewStuckItemViewModel.FromTask)
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
            UpdateReviewSummary();
            StatusText = "Review could not load.";
            WeekSummaryText = "Review highlights could not load.";
            SmallWinsText = "Small wins could not load.";
            StuckItemsText = "Stuck items could not load.";
            EmptyStateText = "Try Review again after checking local data.";
        }
        finally
        {
            IsLoadingReview = false;
        }
    }

    private void UpdateReviewSummary()
    {
        int reviewCount = SmallWins.Count + StuckItems.Count;
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
        OnPropertyChanged(nameof(HasReviewData));
        OnPropertyChanged(nameof(HasSmallWins));
        OnPropertyChanged(nameof(HasStuckItems));
    }

    public static bool ShouldShowStuckItem(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);

        return task.Status == TaskItemStatus.Blocked
            || HasTag(task, "Stuck")
            || HasTag(task, "Repeated Snooze");
    }

    private string BuildLoadedStatusText()
    {
        if (HasSmallWins && HasStuckItems)
        {
            return "Review ready.";
        }

        if (HasSmallWins)
        {
            return "Small wins ready.";
        }

        return HasStuckItems ? "Stuck items ready." : "No review items yet.";
    }

    private string BuildLoadedEmptyStateText()
    {
        if (HasSmallWins && HasStuckItems)
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

        return "Complete one small task, then come back to Review.";
    }

    private string BuildWeekSummaryText()
    {
        if (!HasSmallWins && !HasStuckItems)
        {
            return "No local wins or stuck items loaded yet.";
        }

        if (HasSmallWins && !HasStuckItems)
        {
            return SmallWins.Count == 1
                ? "1 small win this review."
                : $"{SmallWins.Count} small wins this review.";
        }

        if (!HasSmallWins && HasStuckItems)
        {
            return StuckItems.Count == 1
                ? "1 stuck item this review."
                : $"{StuckItems.Count} stuck items this review.";
        }

        string winsText = SmallWins.Count == 1 ? "1 small win" : $"{SmallWins.Count} small wins";
        string stuckText = StuckItems.Count == 1 ? "1 stuck item" : $"{StuckItems.Count} stuck items";
        return $"{winsText} and {stuckText} this review.";
    }

    private static int StuckSortValue(TaskItem task)
    {
        if (task.Status == TaskItemStatus.Blocked)
        {
            return 0;
        }

        return HasTag(task, "Repeated Snooze") ? 1 : 2;
    }

    private static bool HasTag(TaskItem task, string tag)
    {
        return task.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
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

public sealed class ReviewSmallWinViewModel
{
    private ReviewSmallWinViewModel(NoteItem note)
    {
        Id = note.Id;
        OwnerType = note.OwnerType;
        OwnerId = note.OwnerId;
        Text = note.Text;
        CreatedAt = note.CreatedAt;
        CreatedText = $"Recorded {note.CreatedAt.LocalDateTime:MMM d, h:mm tt}";
        SourceText = BuildSourceText(note);
        BadgeText = "Small Win";
        CardIconGlyph = "\uE73E";
        CardAccentColor = "#A8E6B1";
        CardBorderColor = "#CDEAD5";
        CardToolTip = $"{BadgeText}: {Text}";
        HasSource = !string.IsNullOrWhiteSpace(SourceText);
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

    public static ReviewSmallWinViewModel FromNote(NoteItem note)
    {
        ArgumentNullException.ThrowIfNull(note);
        return new ReviewSmallWinViewModel(note);
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
}

public sealed class ReviewStuckItemViewModel
{
    private ReviewStuckItemViewModel(TaskItem task)
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

    public static ReviewStuckItemViewModel FromTask(TaskItem task)
    {
        ArgumentNullException.ThrowIfNull(task);
        return new ReviewStuckItemViewModel(task);
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
