using System.Globalization;
using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Infrastructure;

public sealed class SourceLinkRepository
{
    private readonly AppPaths _paths;

    public SourceLinkRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task SaveAsync(SourceLink sourceLink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceLink);

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO source_links (
                id,
                local_item_type,
                local_item_id,
                source_system,
                external_id,
                external_container_id,
                external_display_name,
                external_web_url,
                source_version,
                sync_state,
                created_at,
                last_attempted_at,
                last_synced_at,
                last_failed_at,
                retry_count,
                failure_message,
                is_read_only)
            VALUES (
                $id,
                $localItemType,
                $localItemId,
                $sourceSystem,
                $externalId,
                $externalContainerId,
                $externalDisplayName,
                $externalWebUrl,
                $sourceVersion,
                $syncState,
                $createdAt,
                $lastAttemptedAt,
                $lastSyncedAt,
                $lastFailedAt,
                $retryCount,
                $failureMessage,
                $isReadOnly)
            ON CONFLICT(id) DO UPDATE SET
                local_item_type = excluded.local_item_type,
                local_item_id = excluded.local_item_id,
                source_system = excluded.source_system,
                external_id = excluded.external_id,
                external_container_id = excluded.external_container_id,
                external_display_name = excluded.external_display_name,
                external_web_url = excluded.external_web_url,
                source_version = excluded.source_version,
                sync_state = excluded.sync_state,
                last_attempted_at = excluded.last_attempted_at,
                last_synced_at = excluded.last_synced_at,
                last_failed_at = excluded.last_failed_at,
                retry_count = excluded.retry_count,
                failure_message = excluded.failure_message,
                is_read_only = excluded.is_read_only;
            """;

        AddSourceLinkParameters(command, sourceLink);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SourceLink?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            {SelectSourceLinkSql}
            WHERE id = $id;
            """;
        command.Parameters.AddWithValue("$id", FormatGuid(id));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadSourceLink(reader);
    }

    public async Task<IReadOnlyList<SourceLink>> GetForLocalItemAsync(
        SourceLinkItemType localItemType,
        Guid localItemId,
        CancellationToken cancellationToken = default)
    {
        List<SourceLink> sourceLinks = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            {SelectSourceLinkSql}
            WHERE local_item_type = $localItemType
              AND local_item_id = $localItemId
            ORDER BY source_system, external_display_name, id;
            """;
        command.Parameters.AddWithValue("$localItemType", localItemType.ToString());
        command.Parameters.AddWithValue("$localItemId", FormatGuid(localItemId));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sourceLinks.Add(ReadSourceLink(reader));
        }

        return sourceLinks;
    }

    private const string SelectSourceLinkSql =
        """
        SELECT
            id,
            local_item_type,
            local_item_id,
            source_system,
            external_id,
            external_container_id,
            external_display_name,
            external_web_url,
            source_version,
            sync_state,
            created_at,
            last_attempted_at,
            last_synced_at,
            last_failed_at,
            retry_count,
            failure_message,
            is_read_only
        FROM source_links
        """;

    private static void AddSourceLinkParameters(SqliteCommand command, SourceLink sourceLink)
    {
        command.Parameters.AddWithValue("$id", FormatGuid(sourceLink.Id));
        command.Parameters.AddWithValue("$localItemType", sourceLink.LocalItemType.ToString());
        command.Parameters.AddWithValue("$localItemId", FormatGuid(sourceLink.LocalItemId));
        command.Parameters.AddWithValue("$sourceSystem", sourceLink.SourceSystem.ToString());
        command.Parameters.AddWithValue("$externalId", sourceLink.ExternalId);
        command.Parameters.AddWithValue("$externalContainerId", sourceLink.ExternalContainerId);
        command.Parameters.AddWithValue("$externalDisplayName", sourceLink.ExternalDisplayName);
        command.Parameters.AddWithValue("$externalWebUrl", sourceLink.ExternalWebUrl);
        command.Parameters.AddWithValue("$sourceVersion", sourceLink.SourceVersion);
        command.Parameters.AddWithValue("$syncState", sourceLink.SyncState.ToString());
        command.Parameters.AddWithValue("$createdAt", FormatDateTimeOffset(sourceLink.CreatedAt));
        command.Parameters.AddWithValue("$lastAttemptedAt", ToDbValue(sourceLink.LastAttemptedAt));
        command.Parameters.AddWithValue("$lastSyncedAt", ToDbValue(sourceLink.LastSyncedAt));
        command.Parameters.AddWithValue("$lastFailedAt", ToDbValue(sourceLink.LastFailedAt));
        command.Parameters.AddWithValue("$retryCount", sourceLink.RetryCount);
        command.Parameters.AddWithValue("$failureMessage", sourceLink.FailureMessage);
        command.Parameters.AddWithValue("$isReadOnly", sourceLink.IsReadOnly ? 1 : 0);
    }

    private static SourceLink ReadSourceLink(SqliteDataReader reader)
    {
        return SourceLink.Rehydrate(
            ParseGuid(reader.GetString(0)),
            Enum.Parse<SourceLinkItemType>(reader.GetString(1)),
            ParseGuid(reader.GetString(2)),
            Enum.Parse<SourceSystem>(reader.GetString(3)),
            reader.GetString(4),
            ParseDateTimeOffset(reader.GetString(10)),
            Enum.Parse<SyncState>(reader.GetString(9)),
            externalContainerId: reader.GetString(5),
            externalDisplayName: reader.GetString(6),
            externalWebUrl: reader.GetString(7),
            sourceVersion: reader.GetString(8),
            lastAttemptedAt: ReadDateTimeOffset(reader, 11),
            lastSyncedAt: ReadDateTimeOffset(reader, 12),
            lastFailedAt: ReadDateTimeOffset(reader, 13),
            retryCount: reader.GetInt32(14),
            failureMessage: reader.GetString(15),
            isReadOnly: reader.GetInt32(16) == 1);
    }

    private static object ToDbValue(DateTimeOffset? value)
    {
        return value.HasValue ? FormatDateTimeOffset(value.Value) : DBNull.Value;
    }

    private static DateTimeOffset? ReadDateTimeOffset(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : ParseDateTimeOffset(reader.GetString(ordinal));
    }

    private static string FormatGuid(Guid id)
    {
        return id.ToString("D");
    }

    private static Guid ParseGuid(string value)
    {
        return Guid.Parse(value);
    }

    private static string FormatDateTimeOffset(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
    }

    private static DateTimeOffset ParseDateTimeOffset(string value)
    {
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }
}
