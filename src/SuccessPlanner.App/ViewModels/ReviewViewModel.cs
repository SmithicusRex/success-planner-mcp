using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class ReviewViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
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
        : base(ScreenCatalog.Review)
    {
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

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
        private set => SetProperty(ref _isLoadingReview, value);
    }

    public bool HasReviewData => false;

    public bool HasSmallWins => false;

    public bool HasStuckItems => false;

    public bool HasNeedsDecisionItems => false;

    public bool HasNextFocus => false;

    public bool CanSaveReview => false;

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
