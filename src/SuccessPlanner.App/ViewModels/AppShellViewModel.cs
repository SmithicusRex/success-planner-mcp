namespace SuccessPlanner.App.ViewModels;

public sealed class AppShellViewModel
{
    public AppShellViewModel(string statusText, string footerText)
    {
        StatusText = statusText;
        FooterText = footerText;
    }

    public string StatusText { get; }

    public string FooterText { get; }
}
