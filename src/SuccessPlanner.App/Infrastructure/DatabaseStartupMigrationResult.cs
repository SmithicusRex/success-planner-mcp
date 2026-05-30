namespace SuccessPlanner.App.Infrastructure;

public sealed record DatabaseStartupMigrationResult(
    DatabaseMigrationResult Migration,
    DatabaseHealthCheckResult Health)
{
    public string StatusText => Migration.AppliedMigrations
        ? "Ready - Data Updated"
        : "Ready - Data OK";
}
