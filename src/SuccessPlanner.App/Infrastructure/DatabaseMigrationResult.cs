namespace SuccessPlanner.App.Infrastructure;

public sealed record DatabaseMigrationResult(
    IReadOnlyList<int> AppliedVersionsThisRun,
    int AppliedCountThisRun,
    int TotalAppliedCount,
    int LatestAppliedVersion,
    int RequiredMigrationCount,
    int LatestRequiredVersion)
{
    public bool AppliedMigrations => AppliedCountThisRun > 0;
}
