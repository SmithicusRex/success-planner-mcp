namespace SuccessPlanner.App.Services;

public sealed class NoMicrosoftPlannerAccessTokenProvider : IMicrosoftPlannerAccessTokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<string?>(null);
    }
}
