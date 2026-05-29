using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.Services;

public sealed class NavigationService : INavigationService
{
    private readonly Dictionary<AppScreen, Func<IScreenViewModel>> _screenFactories = [];
    private readonly Stack<AppScreen> _backStack = [];

    public IScreenViewModel? CurrentScreen { get; private set; }

    public bool CanGoBack => CurrentScreen?.CanGoBack == true && _backStack.Count > 0;

    public event EventHandler<NavigationChangedEventArgs>? Navigated;

    public void Register(AppScreen screen, Func<IScreenViewModel> screenFactory)
    {
        ArgumentNullException.ThrowIfNull(screenFactory);
        _screenFactories[screen] = screenFactory;
    }

    public async Task NavigateToAsync(AppScreen screen, CancellationToken cancellationToken = default)
    {
        await NavigateToAsync(screen, addCurrentToBackStack: true, cancellationToken);
    }

    public async Task GoHomeAsync(CancellationToken cancellationToken = default)
    {
        _backStack.Clear();
        await NavigateToAsync(AppScreen.Home, addCurrentToBackStack: false, cancellationToken);
    }

    public async Task GoBackAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!CanGoBack)
        {
            return;
        }

        AppScreen previousScreen = _backStack.Pop();
        await NavigateToAsync(previousScreen, addCurrentToBackStack: false, cancellationToken);
    }

    private async Task NavigateToAsync(
        AppScreen screen,
        bool addCurrentToBackStack,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (CurrentScreen?.Descriptor.Screen == screen)
        {
            return;
        }

        if (!_screenFactories.TryGetValue(screen, out Func<IScreenViewModel>? factory))
        {
            throw new InvalidOperationException($"Screen '{screen}' has not been registered.");
        }

        IScreenViewModel? previousScreen = CurrentScreen;
        if (previousScreen is not null)
        {
            await previousScreen.OnNavigatedFromAsync(cancellationToken);
        }

        if (screen == AppScreen.Home)
        {
            _backStack.Clear();
        }
        else if (addCurrentToBackStack && previousScreen is not null && previousScreen.CanGoBack)
        {
            _backStack.Push(previousScreen.Descriptor.Screen);
        }

        IScreenViewModel nextScreen = factory();
        CurrentScreen = nextScreen;
        await nextScreen.OnNavigatedToAsync(cancellationToken);

        Navigated?.Invoke(this, new NavigationChangedEventArgs(previousScreen, nextScreen, CanGoBack));
    }
}
