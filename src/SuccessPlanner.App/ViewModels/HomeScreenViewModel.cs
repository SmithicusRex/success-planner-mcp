using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class HomeScreenViewModel : ScreenViewModelBase
{
    public HomeScreenViewModel()
        : base(ScreenCatalog.Home, canGoBack: false)
    {
    }
}
