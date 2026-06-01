namespace SuccessPlanner.App.Services;

public sealed class NoMicrosoftToDoAccessTokenProvider : IMicrosoftToDoAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(null);
    }
}
