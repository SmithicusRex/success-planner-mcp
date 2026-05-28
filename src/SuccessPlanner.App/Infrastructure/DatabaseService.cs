namespace SuccessPlanner.App.Infrastructure;

public sealed class DatabaseService
{
    private readonly AppPaths _paths;
    private bool _isOpen;

    public DatabaseService(AppPaths paths)
    {
        _paths = paths;
    }

    public Task OpenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(_paths.AppDataDirectory);

        if (!File.Exists(_paths.DatabasePath))
        {
            File.WriteAllText(_paths.DatabasePath, "Success Planner MCP local data store placeholder.");
        }

        _isOpen = true;
        return Task.CompletedTask;
    }

    public Task MigrateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();
        return Task.CompletedTask;
    }

    public Task HealthCheckAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();

        _ = new FileInfo(_paths.DatabasePath);
        return Task.CompletedTask;
    }

    public Task CloseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _isOpen = false;
        return Task.CompletedTask;
    }

    private void EnsureOpen()
    {
        if (!_isOpen)
        {
            throw new InvalidOperationException("The local database has not been opened.");
        }
    }
}
