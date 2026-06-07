namespace SuccessPlanner.App.Services;

public interface IMicrosoftPlannerTaskAdapter
{
    Task<MicrosoftPlannerPullResult> PullAssignedTasksAsync(
        CancellationToken cancellationToken = default);
}
