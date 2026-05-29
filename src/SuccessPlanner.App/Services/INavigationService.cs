using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.Services;

public interface INavigationService
{
    IScreenViewModel? CurrentScreen { get; }

    bool CanGoBack { get; }

    event EventHandler<NavigationChangedEventArgs>? Navigated;

    void Register(AppScreen screen, Func<IScreenViewModel> screenFactory);

    Task NavigateToAsync(AppScreen screen, CancellationToken cancellationToken = default);

    Task GoHomeAsync(CancellationToken cancellationToken = default);

    Task GoBackAsync(CancellationToken cancellationToken = default);
}
