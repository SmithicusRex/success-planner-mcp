namespace SuccessPlanner.App.Services;

public interface IMicrosoftPlannerAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
