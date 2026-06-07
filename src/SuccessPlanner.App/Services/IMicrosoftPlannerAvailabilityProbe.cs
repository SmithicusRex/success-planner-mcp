namespace SuccessPlanner.App.Services;

public interface IMicrosoftPlannerAvailabilityProbe
{
    Task<MicrosoftPlannerConnectionStatus> TestAvailabilityAsync(
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken = default);
}
