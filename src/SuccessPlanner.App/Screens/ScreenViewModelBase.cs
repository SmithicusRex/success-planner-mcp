namespace SuccessPlanner.App.Screens;

public abstract class ScreenViewModelBase : IScreenViewModel
{
    protected ScreenViewModelBase(AppScreenDescriptor descriptor, bool canGoBack = true)
    {
        Descriptor = descriptor;
        CanGoBack = canGoBack;
    }

    public AppScreenDescriptor Descriptor { get; }

    public bool CanGoBack { get; }

    public virtual Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public virtual Task OnNavigatedFromAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
