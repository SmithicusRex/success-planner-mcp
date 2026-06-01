using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class PlanViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private const string ReadyStatus = "Ready to plan.";
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
    private bool _isLoading;

    public PlanViewModel()
        : base(ScreenCatalog.Plan)
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

    public bool IsLoading
    {
        get => _isLoading;
        private set => SetProperty(ref _isLoading, value);
    }

    public bool HasInboxItems => false;

    public bool HasSelectedInboxItem => false;

    public bool HasPlanningChanges => false;

    public bool CanSavePlan => false;

    public override Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StatusText = ReadyStatus;
        return Task.CompletedTask;
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
