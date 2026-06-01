namespace SuccessPlanner.App.Services;

public interface IMicrosoftToDoAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
