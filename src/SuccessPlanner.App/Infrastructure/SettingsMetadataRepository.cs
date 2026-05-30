using Microsoft.Data.Sqlite;

namespace SuccessPlanner.App.Infrastructure;

public sealed class SettingsMetadataRepository
{
    private readonly AppPaths _paths;

    public SettingsMetadataRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task UpsertAsync(
        string key,
        string value,
        DateTimeOffset? updatedAt = null,
        CancellationToken cancellationToken = default)
    {
        string normalizedKey = NormalizeRequired(key, nameof(key));

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO settings_metadata (key, value, updated_at)
            VALUES ($key, $value, $updatedAt)
            ON CONFLICT(key) DO UPDATE SET
                value = excluded.value,
                updated_at = excluded.updated_at;
            """;

        command.Parameters.AddWithValue("$key", normalizedKey);
        command.Parameters.AddWithValue("$value", value);
        command.Parameters.AddWithValue("$updatedAt", FormatDateTimeOffset(updatedAt ?? DateTimeOffset.UtcNow));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SettingsMetadataEntry?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        string normalizedKey = NormalizeRequired(key, nameof(key));

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT key, value, updated_at
            FROM settings_metadata
            WHERE key = $key;
            """;

        command.Parameters.AddWithValue("$key", normalizedKey);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadEntry(reader);
    }

    public async Task<IReadOnlyList<SettingsMetadataEntry>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<SettingsMetadataEntry> entries = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT key, value, updated_at
            FROM settings_metadata
            ORDER BY key;
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(ReadEntry(reader));
        }

        return entries;
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        string normalizedKey = NormalizeRequired(key, nameof(key));

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM settings_metadata WHERE key = $key;";
        command.Parameters.AddWithValue("$key", normalizedKey);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static SettingsMetadataEntry ReadEntry(SqliteDataReader reader)
    {
        return new SettingsMetadataEntry(
            reader.GetString(0),
            reader.GetString(1),
            DateTimeOffset.Parse(reader.GetString(2), null, System.Globalization.DateTimeStyles.RoundtripKind));
    }

    private static string FormatDateTimeOffset(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("O");
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

public sealed record SettingsMetadataEntry(string Key, string Value, DateTimeOffset UpdatedAt);
