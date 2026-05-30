using Microsoft.Data.Sqlite;

namespace SuccessPlanner.App.Infrastructure;

public sealed class DatabaseMigrator
{
    private const string MigrationTableName = "schema_migrations";
    private readonly IReadOnlyList<IDatabaseMigration> _migrations;

    public DatabaseMigrator(IReadOnlyList<IDatabaseMigration> migrations)
    {
        _migrations = migrations
            .OrderBy(migration => migration.Version)
            .ToArray();

        ValidateMigrations(_migrations);
    }

    public async Task<DatabaseMigrationResult> MigrateAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await EnsureMigrationTableAsync(connection, cancellationToken);

        HashSet<int> appliedVersions = await GetAppliedVersionsAsync(connection, cancellationToken);
        List<int> appliedVersionsThisRun = [];

        foreach (IDatabaseMigration migration in _migrations)
        {
            if (appliedVersions.Contains(migration.Version))
            {
                continue;
            }

            await ApplyMigrationAsync(connection, migration, cancellationToken);
            appliedVersions.Add(migration.Version);
            appliedVersionsThisRun.Add(migration.Version);
        }

        int latestAppliedVersion = appliedVersions.Count == 0
            ? 0
            : appliedVersions.Max();
        int latestRequiredVersion = _migrations.Count == 0
            ? 0
            : _migrations.Max(migration => migration.Version);

        return new DatabaseMigrationResult(
            appliedVersionsThisRun,
            appliedVersionsThisRun.Count,
            appliedVersions.Count,
            latestAppliedVersion,
            _migrations.Count,
            latestRequiredVersion);
    }

    private static async Task EnsureMigrationTableAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE IF NOT EXISTS {MigrationTableName} (
                version INTEGER NOT NULL PRIMARY KEY,
                name TEXT NOT NULL,
                applied_at TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<HashSet<int>> GetAppliedVersionsAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        HashSet<int> versions = [];

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = $"SELECT version FROM {MigrationTableName} ORDER BY version;";

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            versions.Add(reader.GetInt32(0));
        }

        return versions;
    }

    private static async Task ApplyMigrationAsync(
        SqliteConnection connection,
        IDatabaseMigration migration,
        CancellationToken cancellationToken)
    {
        using SqliteTransaction transaction = connection.BeginTransaction();

        try
        {
            await migration.ApplyAsync(connection, transaction, cancellationToken);
            await RecordMigrationAsync(connection, transaction, migration, cancellationToken);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static async Task RecordMigrationAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        IDatabaseMigration migration,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText =
            $"""
            INSERT INTO {MigrationTableName} (version, name, applied_at)
            VALUES ($version, $name, $appliedAt);
            """;

        command.Parameters.AddWithValue("$version", migration.Version);
        command.Parameters.AddWithValue("$name", migration.Name);
        command.Parameters.AddWithValue("$appliedAt", DateTimeOffset.UtcNow.ToString("O"));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void ValidateMigrations(IReadOnlyList<IDatabaseMigration> migrations)
    {
        HashSet<int> versions = [];

        foreach (IDatabaseMigration migration in migrations)
        {
            if (!versions.Add(migration.Version))
            {
                throw new InvalidOperationException($"Duplicate database migration version {migration.Version}.");
            }
        }
    }
}
