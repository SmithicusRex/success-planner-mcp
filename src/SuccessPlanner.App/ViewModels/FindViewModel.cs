using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.ViewModels;

public sealed class FindViewModel : ScreenViewModelBase, INotifyPropertyChanged
{
    private readonly Func<string, CancellationToken, Task<IReadOnlyList<LocalSearchResult>>> _searchAsync;
    private FindResultViewModel? _openedResult;
    private string _searchText = string.Empty;
    private string _statusText = "Ready to find.";
    private string _searchPanelTitle = "Find Local Data";
    private string _searchPanelText = "Search tasks, projects, notes, and source links from this computer.";
    private string _emptyStateText = "Type a word or phrase to search local data.";
    private string _resultsCountText = "0 results";
    private string _openedItemPanelTitle = "No Item Open";
    private string _openedItemPanelText = "Search results can be opened here as local Success Planner items.";
    private bool _isSearching;

    public FindViewModel()
        : this((_, _) => Task.FromResult<IReadOnlyList<LocalSearchResult>>([]))
    {
    }

    public FindViewModel(Func<string, CancellationToken, Task<IReadOnlyList<LocalSearchResult>>> searchAsync)
        : base(ScreenCatalog.Find)
    {
        ArgumentNullException.ThrowIfNull(searchAsync);
        _searchAsync = searchAsync;
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

    public ObservableCollection<FindResultViewModel> Results { get; } = [];

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

    public FindResultViewModel? OpenedResult
    {
        get => _openedResult;
        private set
        {
            if (ReferenceEquals(_openedResult, value))
            {
                return;
            }

            if (_openedResult is not null)
            {
                _openedResult.IsOpened = false;
            }

            _openedResult = value;

            if (_openedResult is not null)
            {
                _openedResult.IsOpened = true;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(HasOpenedItem));
        }
    }

    public string OpenedItemPanelTitle
    {
        get => _openedItemPanelTitle;
        private set => SetProperty(ref _openedItemPanelTitle, value);
    }

    public string OpenedItemPanelText
    {
        get => _openedItemPanelText;
        private set => SetProperty(ref _openedItemPanelText, value);
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

    public bool HasResults => Results.Count > 0;

    public bool HasOpenedItem => OpenedResult is not null;

    public bool CanSearch => HasQuery && !IsSearching;

    public async Task SearchAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!HasQuery)
        {
            ClearResults();
            StatusText = "Type something to search.";
            EmptyStateText = "Type a word or phrase to search local data.";
            return;
        }

        string query = SearchText.Trim();
        IsSearching = true;
        StatusText = "Searching local data.";
        EmptyStateText = "Searching this computer.";

        try
        {
            IReadOnlyList<LocalSearchResult> results = await _searchAsync(query, cancellationToken);
            ClearOpenedItem();
            Results.Clear();
            foreach (FindResultViewModel result in results.Select(result => FindResultViewModel.FromSearchResult(
                         result,
                         OpenLocalItemAsync)))
            {
                Results.Add(result);
            }

            RefreshResultsState(query);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            ClearOpenedItem();
            ClearResults();
            StatusText = "Find could not search.";
            EmptyStateText = "Try the local search again.";
        }
        finally
        {
            IsSearching = false;
        }
    }

    public Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        ClearOpenedItem();
        ClearResults();
        StatusText = "Search cleared.";
        SearchPanelTitle = "Find Local Data";
        SearchPanelText = "Search tasks, projects, notes, and source links from this computer.";
        EmptyStateText = "Type a word or phrase to search local data.";
        ResultsCountText = "0 results";
        return Task.CompletedTask;
    }

    public Task OpenLocalItemAsync(FindResultViewModel result)
    {
        ArgumentNullException.ThrowIfNull(result);

        OpenedResult = result;
        OpenedItemPanelTitle = "Opened Item";
        OpenedItemPanelText = $"{result.BadgeText}: {result.Title}";
        StatusText = "Local item opened.";
        return Task.CompletedTask;
    }

    private void RefreshSearchTextState()
    {
        if (Results.Count > 0)
        {
            ClearOpenedItem();
            ClearResults();
        }

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

    private void RefreshResultsState(string query)
    {
        ResultsCountText = Results.Count == 1 ? "1 result" : $"{Results.Count} results";
        if (HasResults)
        {
            StatusText = "Search complete.";
            SearchPanelTitle = "Local Matches";
            SearchPanelText = $"Found local matches for \"{query}\".";
            EmptyStateText = "Local matches found.";
        }
        else
        {
            StatusText = "No local matches.";
            SearchPanelTitle = "No Matches";
            SearchPanelText = $"No local matches for \"{query}\" yet.";
            EmptyStateText = $"No local matches for \"{query}\".";
        }

        OnPropertyChanged(nameof(HasResults));
        ClearSearchCommand.RaiseCanExecuteChanged();
    }

    private void ClearResults()
    {
        if (Results.Count > 0)
        {
            Results.Clear();
        }

        ResultsCountText = "0 results";
        OnPropertyChanged(nameof(HasResults));
        ClearSearchCommand.RaiseCanExecuteChanged();
    }

    private void ClearOpenedItem()
    {
        OpenedResult = null;
        OpenedItemPanelTitle = "No Item Open";
        OpenedItemPanelText = "Search results can be opened here as local Success Planner items.";
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

public sealed class FindResultViewModel : INotifyPropertyChanged
{
    private bool _isOpened;

    private FindResultViewModel(
        LocalSearchResult result,
        Func<FindResultViewModel, Task> openLocalItemAsync)
    {
        ArgumentNullException.ThrowIfNull(openLocalItemAsync);

        Id = result.ItemId;
        Kind = result.Kind;
        Title = result.Title;
        Detail = result.Detail;
        SourceText = result.SourceText;
        CreatedAt = result.CreatedAt;
        LocalItemType = result.LocalItemType;
        LocalItemId = result.LocalItemId;
        ExternalWebUrl = result.ExternalWebUrl;
        HasDetail = !string.IsNullOrWhiteSpace(Detail);
        HasExternalSource = !string.IsNullOrWhiteSpace(ExternalWebUrl);
        BadgeText = BuildBadgeText(result.Kind);
        CardIconGlyph = BuildIconGlyph(result.Kind);
        CardAccentColor = BuildAccentColor(result.Kind);
        CardBorderColor = BuildBorderColor(result.Kind);
        CardToolTip = HasDetail ? $"{Title} - {Detail}" : Title;
        LocalIdText = LocalItemId.HasValue
            ? LocalItemId.Value.ToString("D")
            : Id.ToString("D");
        CreatedText = CreatedAt.ToLocalTime().ToString("g");
        OpenCommand = new AsyncRelayCommand(() => openLocalItemAsync(this));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Guid Id { get; }

    public LocalSearchResultKind Kind { get; }

    public string Title { get; }

    public string Detail { get; }

    public string SourceText { get; }

    public DateTimeOffset CreatedAt { get; }

    public string LocalItemType { get; }

    public Guid? LocalItemId { get; }

    public string ExternalWebUrl { get; }

    public bool HasDetail { get; }

    public bool HasExternalSource { get; }

    public string BadgeText { get; }

    public string CardIconGlyph { get; }

    public string CardAccentColor { get; }

    public string CardBorderColor { get; }

    public string CardToolTip { get; }

    public string LocalIdText { get; }

    public string CreatedText { get; }

    public AsyncRelayCommand OpenCommand { get; }

    public bool IsOpened
    {
        get => _isOpened;
        set
        {
            if (_isOpened == value)
            {
                return;
            }

            _isOpened = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CardBackgroundColor));
            OnPropertyChanged(nameof(OpenButtonText));
        }
    }

    public string CardBackgroundColor => IsOpened ? "#FFFFFF" : "#F7FAFF";

    public string OpenButtonText => IsOpened ? "Opened" : "Open";

    public static FindResultViewModel FromSearchResult(
        LocalSearchResult result,
        Func<FindResultViewModel, Task> openLocalItemAsync)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new FindResultViewModel(result, openLocalItemAsync);
    }

    private static string BuildBadgeText(LocalSearchResultKind kind)
    {
        return kind switch
        {
            LocalSearchResultKind.Task => "Task",
            LocalSearchResultKind.Project => "Project",
            LocalSearchResultKind.Note => "Note",
            LocalSearchResultKind.SourceLink => "Source Link",
            _ => "Result"
        };
    }

    private static string BuildIconGlyph(LocalSearchResultKind kind)
    {
        return kind switch
        {
            LocalSearchResultKind.Task => "\uE8FD",
            LocalSearchResultKind.Project => "\uE8F1",
            LocalSearchResultKind.Note => "\uE70B",
            LocalSearchResultKind.SourceLink => "\uE71B",
            _ => "\uE721"
        };
    }

    private static string BuildAccentColor(LocalSearchResultKind kind)
    {
        return kind switch
        {
            LocalSearchResultKind.Task => "#D6E8FF",
            LocalSearchResultKind.Project => "#FFF6D6",
            LocalSearchResultKind.Note => "#ECF8EE",
            LocalSearchResultKind.SourceLink => "#F0ECFF",
            _ => "#EAF4FF"
        };
    }

    private static string BuildBorderColor(LocalSearchResultKind kind)
    {
        return kind switch
        {
            LocalSearchResultKind.Task => "#B7D8FF",
            LocalSearchResultKind.Project => "#E4CD75",
            LocalSearchResultKind.Note => "#CDEAD5",
            LocalSearchResultKind.SourceLink => "#B8A2F0",
            _ => "#D8E9FF"
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
