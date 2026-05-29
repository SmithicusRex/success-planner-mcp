using System.Windows.Input;
using SuccessPlanner.App.Commands;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.ViewModels;

public sealed class HomeScreenViewModel : ScreenViewModelBase
{
    private readonly INavigationService _navigationService;

    public HomeScreenViewModel(INavigationService navigationService)
        : base(ScreenCatalog.Home, canGoBack: false)
    {
        _navigationService = navigationService;

        OpenCaptureCommand = CreateNavigationCommand(AppScreen.Capture);
        OpenTodayCommand = CreateNavigationCommand(AppScreen.Today);
        OpenPlanCommand = CreateNavigationCommand(AppScreen.Plan);
        OpenStartCommand = CreateNavigationCommand(AppScreen.StartWork);
        OpenDoneCommand = CreateNavigationCommand(AppScreen.Done);
        OpenMoveCommand = CreateNavigationCommand(AppScreen.Move);
        OpenReviewCommand = CreateNavigationCommand(AppScreen.Review);
    }

    public ICommand OpenCaptureCommand { get; }

    public ICommand OpenTodayCommand { get; }

    public ICommand OpenPlanCommand { get; }

    public ICommand OpenStartCommand { get; }

    public ICommand OpenDoneCommand { get; }

    public ICommand OpenMoveCommand { get; }

    public ICommand OpenReviewCommand { get; }

    private ICommand CreateNavigationCommand(AppScreen screen)
    {
        return new AsyncRelayCommand(() => _navigationService.NavigateToAsync(screen));
    }
}
