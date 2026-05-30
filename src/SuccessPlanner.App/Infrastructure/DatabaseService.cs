using Microsoft.Data.Sqlite;

namespace SuccessPlanner.App.Infrastructure;

public sealed class DatabaseService
{
    private const string PlaceholderText = "Success Planner MCP local data store placeholder.";
    private const string SqliteHeader = "SQLite format 3";

    private readonly AppPaths _paths;
    private readonly DatabaseMigrator _migrator;
    private SqliteConnection? _connection;
    private bool _isOpen;

    public DatabaseService(AppPaths paths)
    {
        _paths = paths;
        _migrator = new DatabaseMigrator(DatabaseMigrations.All);
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Directory.CreateDirectory(_paths.AppDataDirectory);

        ReplacePlaceholderIfNeeded();

        SqliteConnectionStringBuilder connectionString = new()
        {
            DataSource = _paths.DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate
        };
        connectionString["Pooling"] = false;

        _connection = new SqliteConnection(connectionString.ToString());
        await _connection.OpenAsync(cancellationToken);
        await ExecuteNonQueryAsync("PRAGMA foreign_keys = ON;", cancellationToken);
        _isOpen = true;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();
        await _migrator.MigrateAsync(_connection!, cancellationToken);
    }

    public async Task HealthCheckAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();

        await using SqliteCommand command = _connection!.CreateCommand();
        command.CommandText = "PRAGMA quick_check;";
        object? result = await command.ExecuteScalarAsync(cancellationToken);

        if (!string.Equals(result?.ToString(), "ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The local SQLite database health check failed.");
        }
    }

    public async Task CloseAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
            SqliteConnection.ClearAllPools();
        }

        _isOpen = false;
    }

    private void EnsureOpen()
    {
        if (!_isOpen)
        {
            throw new InvalidOperationException("The local database has not been opened.");
        }
    }

    private async Task ExecuteNonQueryAsync(string commandText, CancellationToken cancellationToken)
    {
        EnsureConnection();

        await using SqliteCommand command = _connection!.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private void EnsureConnection()
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("The local SQLite connection has not been created.");
        }
    }

    private void ReplacePlaceholderIfNeeded()
    {
        if (!File.Exists(_paths.DatabasePath))
        {
            return;
        }

        FileInfo existingFile = new(_paths.DatabasePath);
        if (existingFile.Length == 0)
        {
            File.Delete(_paths.DatabasePath);
            return;
        }

        if (IsSqliteDatabase(_paths.DatabasePath))
        {
            return;
        }

        string existingContent = File.ReadAllText(_paths.DatabasePath).Trim();
        if (!string.Equals(existingContent, PlaceholderText, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The local data file exists but is not a SQLite database.");
        }

        string backupPath = $"{_paths.DatabasePath}.placeholder-{DateTimeOffset.Now:yyyyMMddHHmmss}";
        File.Move(_paths.DatabasePath, backupPath);
    }

    private static bool IsSqliteDatabase(string path)
    {
        FileInfo file = new(path);
        if (!file.Exists || file.Length < 16)
        {
            return false;
        }

        Span<byte> header = stackalloc byte[16];
        using FileStream stream = File.OpenRead(path);
        int bytesRead = stream.Read(header);

        if (bytesRead < 16)
        {
            return false;
        }

        string headerText = System.Text.Encoding.ASCII.GetString(header);
        return headerText.StartsWith(SqliteHeader, StringComparison.Ordinal);
    }
}
