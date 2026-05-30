using Microsoft.Data.Sqlite;

namespace SuccessPlanner.App.Infrastructure;

public interface IDatabaseMigration
{
    int Version { get; }

    string Name { get; }

    Task ApplyAsync(SqliteConnection connection, SqliteTransaction transaction, CancellationToken cancellationToken);
}

public sealed class SqlDatabaseMigration : IDatabaseMigration
{
    private readonly string[] _statements;

    public SqlDatabaseMigration(int version, string name, params string[] statements)
    {
        if (version < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(version), "Migration version must be greater than zero.");
        }

        Version = version;
        Name = NormalizeRequired(name, nameof(name));
        _statements = statements.Length > 0
            ? statements
            : throw new ArgumentException("Migration must contain at least one SQL statement.", nameof(statements));
    }

    public int Version { get; }

    public string Name { get; }

    public async Task ApplyAsync(SqliteConnection connection, SqliteTransaction transaction, CancellationToken cancellationToken)
    {
        foreach (string statement in _statements)
        {
            await using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = statement;
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
