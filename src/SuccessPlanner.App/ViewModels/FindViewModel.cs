using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class FindViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private string _searchText = string.Empty;
    private string _statusText = "Ready to find.";
    private string _searchPanelTitle = "Find Local Data";
    private string _searchPanelText = "Search tasks, projects, notes, and source links from this computer.";
    private string _emptyStateText = "Type a word or phrase to search local data.";
    private string _resultsCountText = "0 results";
    private bool _isSearching;

    public FindViewModel()
        : base(ScreenCatalog.Find)
    {
        SearchCommand = new AsyncRelayCommand(
            () => SearchAsync(CancellationToken.None),
            () => CanSearch);
        ClearSearchCommand = new AsyncRelayCommand(
            ClearSearchAsync,
            () => HasQuery || HasResults);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title => Descriptor.Title;

    public string Subtitle => Descriptor.Subtitle;

    public string IconGlyph => Descriptor.IconGlyph;

    public string AccentColor => Descriptor.AccentColor;

    public AsyncRelayCommand SearchCommand { get; }

    public AsyncRelayCommand ClearSearchCommand { get; }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                RefreshSearchTextState();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string SearchPanelTitle
    {
        get => _searchPanelTitle;
        private set => SetProperty(ref _searchPanelTitle, value);
    }

    public string SearchPanelText
    {
        get => _searchPanelText;
        private set => SetProperty(ref _searchPanelText, value);
    }

    public string EmptyStateText
    {
        get => _emptyStateText;
        private set => SetProperty(ref _emptyStateText, value);
    }

    public string ResultsCountText
    {
        get => _resultsCountText;
        private set => SetProperty(ref _resultsCountText, value);
    }

    public bool IsSearching
    {
        get => _isSearching;
        private set
        {
            if (SetProperty(ref _isSearching, value))
            {
                OnPropertyChanged(nameof(CanSearch));
                SearchCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasQuery => !string.IsNullOrWhiteSpace(SearchText);

    public bool HasResults => false;

    public bool CanSearch => HasQuery && !IsSearching;

    public Task SearchAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HasQuery)
        {
            StatusText = "Type something to search.";
            EmptyStateText = "Type a word or phrase to search local data.";
            return Task.CompletedTask;
        }

        StatusText = "Local search is not connected yet.";
        EmptyStateText = "Search service connects in the next Find step.";
        ResultsCountText = "0 results";
        return Task.CompletedTask;
    }

    public Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        StatusText = "Search cleared.";
        SearchPanelTitle = "Find Local Data";
        SearchPanelText = "Search tasks, projects, notes, and source links from this computer.";
        EmptyStateText = "Type a word or phrase to search local data.";
        ResultsCountText = "0 results";
        return Task.CompletedTask;
    }

    private void RefreshSearchTextState()
    {
        if (HasQuery)
        {
            StatusText = "Ready to search locally.";
            SearchPanelTitle = "Ready To Search";
            SearchPanelText = $"Find local matches for \"{SearchText.Trim()}\".";
            EmptyStateText = "Search will stay local-first and work without Microsoft sync.";
        }
        else
        {
            StatusText = "Ready to find.";
            SearchPanelTitle = "Find Local Data";
            SearchPanelText = "Search tasks, projects, notes, and source links from this computer.";
            EmptyStateText = "Type a word or phrase to search local data.";
        }

        OnPropertyChanged(nameof(HasQuery));
        OnPropertyChanged(nameof(CanSearch));
        SearchCommand.RaiseCanExecuteChanged();
        ClearSearchCommand.RaiseCanExecuteChanged();
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
