using Microsoft.Data.Sqlite;

namespace SuccessPlanner.App.Infrastructure;

internal static class SqliteConnectionFactory
{
    public static async Task<SqliteConnection> OpenAsync(AppPaths paths, CancellationToken cancellationToken)
    {
        SqliteConnectionStringBuilder connectionString = new()
        {
            DataSource = paths.DatabasePath,
            Mode = SqliteOpenMode.ReadWrite
        };
        connectionString["Pooling"] = false;

        SqliteConnection connection = new(connectionString.ToString());
        await connection.OpenAsync(cancellationToken);

        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys = ON;";
        await command.ExecuteNonQueryAsync(cancellationToken);

        return connection;
    }
}
