namespace SuccessPlanner.App.Infrastructure;

public sealed class DatabaseStartupMigrationService
{
    private readonly DatabaseService _databaseService;

    public DatabaseStartupMigrationService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task<DatabaseStartupMigrationResult> RunAsync(CancellationToken cancellationToken)
    {
        await _databaseService.OpenAsync(cancellationToken);
        DatabaseMigrationResult migration = await _databaseService.MigrateAsync(cancellationToken);
        DatabaseHealthCheckResult health = await _databaseService.CheckHealthAsync(cancellationToken);

        if (!health.IsHealthy)
        {
            throw new InvalidOperationException(health.ToFailureMessage());
        }

        return new DatabaseStartupMigrationResult(migration, health);
    }
}
