namespace SuccessPlanner.App.Screens;

public interface IScreenViewModel
{
    AppScreenDescriptor Descriptor { get; }

    bool CanGoBack { get; }

    Task OnNavigatedToAsync(CancellationToken cancellationToken);

    Task OnNavigatedFromAsync(CancellationToken cancellationToken);
}
