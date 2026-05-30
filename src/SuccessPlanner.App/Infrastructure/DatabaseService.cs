using Microsoft.Data.Sqlite;

namespace SuccessPlanner.App.Infrastructure;

public sealed class DatabaseService
{
    private const string PlaceholderText = "Success Planner MCP local data store placeholder.";
    private const string SqliteHeader = "SQLite format 3";
    private static readonly string[] RequiredCoreTables =
    [
        "local_store_metadata",
        "schema_migrations",
        "projects",
        "tasks",
        "milestones",
        "notes",
        "focus_sessions",
        "success_goals",
        "movement_sessions",
        "source_links",
        "settings_metadata",
        "sync_queue"
    ];

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
        if (_isOpen)
        {
            return;
        }

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

    public async Task<DatabaseMigrationResult> MigrateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();
        return await _migrator.MigrateAsync(_connection!, cancellationToken);
    }

    public async Task HealthCheckAsync(CancellationToken cancellationToken)
    {
        DatabaseHealthCheckResult result = await CheckHealthAsync(cancellationToken);

        if (!result.IsHealthy)
        {
            throw new InvalidOperationException(result.ToFailureMessage());
        }
    }

    public async Task<DatabaseHealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureOpen();

        List<string> findings = [];
        string quickCheckResult = await ReadScalarTextAsync("PRAGMA quick_check;", cancellationToken);

        if (!string.Equals(quickCheckResult, "ok", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add("SQLite quick_check did not return ok.");
        }

        IReadOnlyList<int> appliedMigrations = await ReadAppliedMigrationVersionsAsync(cancellationToken);
        int requiredMigrationCount = DatabaseMigrations.All.Count;
        int latestRequiredMigration = DatabaseMigrations.All.Count == 0
            ? 0
            : DatabaseMigrations.All.Max(migration => migration.Version);
        int latestAppliedMigration = appliedMigrations.Count == 0
            ? 0
            : appliedMigrations.Max();

        foreach (IDatabaseMigration migration in DatabaseMigrations.All)
        {
            if (!appliedMigrations.Contains(migration.Version))
            {
                findings.Add($"Database migration {migration.Version} is missing.");
            }
        }

        foreach (string tableName in RequiredCoreTables)
        {
            if (!await TableExistsAsync(tableName, cancellationToken))
            {
                findings.Add($"Required table '{tableName}' is missing.");
            }
        }

        if (await TableExistsAsync("local_store_metadata", cancellationToken))
        {
            string storeKind = await ReadScalarTextAsync(
                "SELECT value FROM local_store_metadata WHERE key = 'store_kind';",
                cancellationToken);

            if (!string.Equals(storeKind, "Success Planner MCP SQLite local store", StringComparison.Ordinal))
            {
                findings.Add("Local store metadata is missing or invalid.");
            }
        }

        bool isHealthy = findings.Count == 0;
        string summary = isHealthy
            ? "Local database is healthy."
            : "Local database needs attention.";

        return new DatabaseHealthCheckResult(
            isHealthy,
            summary,
            _paths.DatabasePath,
            quickCheckResult,
            appliedMigrations.Count,
            latestAppliedMigration,
            requiredMigrationCount,
            latestRequiredMigration,
            findings);
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

    private async Task<IReadOnlyList<int>> ReadAppliedMigrationVersionsAsync(CancellationToken cancellationToken)
    {
        if (!await TableExistsAsync("schema_migrations", cancellationToken))
        {
            return [];
        }

        List<int> versions = [];

        await using SqliteCommand command = _connection!.CreateCommand();
        command.CommandText = "SELECT version FROM schema_migrations ORDER BY version;";

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            versions.Add(reader.GetInt32(0));
        }

        return versions;
    }

    private async Task<bool> TableExistsAsync(string tableName, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = _connection!.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = $tableName;
            """;
        command.Parameters.AddWithValue("$tableName", tableName);

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result) == 1;
    }

    private async Task<string> ReadScalarTextAsync(string commandText, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = _connection!.CreateCommand();
        command.CommandText = commandText;
        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result?.ToString() ?? string.Empty;
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
