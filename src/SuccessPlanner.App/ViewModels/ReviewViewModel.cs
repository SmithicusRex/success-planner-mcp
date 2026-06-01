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
        : this(_ => Task.FromResult<IReadOnlyList<NoteItem>>([]))
    {
    }

    public ReviewViewModel(Func<CancellationToken, Task<IReadOnlyList<NoteItem>>> loadSmallWinsAsync)
        : base(ScreenCatalog.Review)
    {
        ArgumentNullException.ThrowIfNull(loadSmallWinsAsync);
        _loadSmallWinsAsync = loadSmallWinsAsync;
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

    public bool HasReviewData => HasSmallWins;

    public bool HasSmallWins => SmallWins.Count > 0;

    public bool HasStuckItems => false;

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
        WeekSummaryText = "Loading local review highlights.";

        try
        {
            IReadOnlyList<NoteItem> loadedSmallWins = await _loadSmallWinsAsync(cancellationToken);
            IReadOnlyList<ReviewSmallWinViewModel> smallWinCards = loadedSmallWins
                .Where(note => note.IsReviewHighlight)
                .OrderByDescending(note => note.CreatedAt)
                .ThenBy(note => note.Text, StringComparer.OrdinalIgnoreCase)
                .Select(ReviewSmallWinViewModel.FromNote)
                .ToList();

            SmallWins.Clear();
            foreach (ReviewSmallWinViewModel card in smallWinCards)
            {
                SmallWins.Add(card);
            }

            UpdateReviewSummary();
            StatusText = HasSmallWins ? "Small wins ready." : "No small wins yet.";
            EmptyStateText = HasSmallWins
                ? "Small wins are loaded from local completions."
                : "Complete one small task, then come back to Review.";
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            SmallWins.Clear();
            UpdateReviewSummary();
            StatusText = "Review could not load.";
            WeekSummaryText = "Review highlights could not load.";
            SmallWinsText = "Small wins could not load.";
            EmptyStateText = "Try Review again after checking local data.";
        }
        finally
        {
            IsLoadingReview = false;
        }
    }

    private void UpdateReviewSummary()
    {
        ReviewCountText = SmallWins.Count == 1 ? "1 review item" : $"{SmallWins.Count} review items";
        WeekSummaryText = SmallWins.Count == 1
            ? "1 small win this review."
            : HasSmallWins
                ? $"{SmallWins.Count} small wins this review."
                : "No local wins loaded yet.";
        SmallWinsText = SmallWins.Count == 1
            ? "1 small win ready."
            : HasSmallWins
                ? $"{SmallWins.Count} small wins ready."
                : "No small wins yet.";
        OnPropertyChanged(nameof(HasReviewData));
        OnPropertyChanged(nameof(HasSmallWins));
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
