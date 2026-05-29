using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.Services;

public sealed class NavigationChangedEventArgs : EventArgs
{
    public NavigationChangedEventArgs(
        IScreenViewModel? previousScreen,
        IScreenViewModel currentScreen,
        bool canGoBack)
    {
        PreviousScreen = previousScreen;
        CurrentScreen = currentScreen;
        CanGoBack = canGoBack;
    }

    public IScreenViewModel? PreviousScreen { get; }

    public IScreenViewModel CurrentScreen { get; }

    public bool CanGoBack { get; }
}
