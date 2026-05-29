using SuccessPlanner.App.Screens;

namespace SuccessPlanner.App.ViewModels;

public sealed class AppShellViewModel
{
    public AppShellViewModel(string statusText, string footerText, IScreenViewModel currentScreen)
    {
        StatusText = statusText;
        FooterText = footerText;
        CurrentScreen = currentScreen;
    }

    public string StatusText { get; }

    public string FooterText { get; }

    public IScreenViewModel CurrentScreen { get; }
}
