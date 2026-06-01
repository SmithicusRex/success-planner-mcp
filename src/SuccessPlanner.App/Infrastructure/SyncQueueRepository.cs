using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Infrastructure;

public sealed class SyncQueueRepository
{
    private readonly AppPaths _paths;

    public SyncQueueRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task EnqueueAsync(SyncQueueItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO sync_queue (
                id,
                local_item_type,
                local_item_id,
                source_system,
                source_link_id,
                action_type,
                payload_json,
                sync_state,
                retry_count,
                next_attempt_at,
                last_attempted_at,
                failure_message,
                created_at,
                updated_at)
            VALUES (
                $id,
                $localItemType,
                $localItemId,
                $sourceSystem,
                $sourceLinkId,
                $actionType,
                $payloadJson,
                $syncState,
                $retryCount,
                $nextAttemptAt,
                $lastAttemptedAt,
                $failureMessage,
                $createdAt,
                $updatedAt);
            """;

        AddQueueItemParameters(command, item);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveAsync(SyncQueueItem item, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(item);

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO sync_queue (
                id,
                local_item_type,
                local_item_id,
                source_system,
                source_link_id,
                action_type,
                payload_json,
                sync_state,
                retry_count,
                next_attempt_at,
                last_attempted_at,
                failure_message,
                created_at,
                updated_at)
            VALUES (
                $id,
                $localItemType,
                $localItemId,
                $sourceSystem,
                $sourceLinkId,
                $actionType,
                $payloadJson,
                $syncState,
                $retryCount,
                $nextAttemptAt,
                $lastAttemptedAt,
                $failureMessage,
                $createdAt,
                $updatedAt)
            ON CONFLICT(id) DO UPDATE SET
                local_item_type = excluded.local_item_type,
                local_item_id = excluded.local_item_id,
                source_system = excluded.source_system,
                source_link_id = excluded.source_link_id,
                action_type = excluded.action_type,
                payload_json = excluded.payload_json,
                sync_state = excluded.sync_state,
                retry_count = excluded.retry_count,
                next_attempt_at = excluded.next_attempt_at,
                last_attempted_at = excluded.last_attempted_at,
                failure_message = excluded.failure_message,
                updated_at = excluded.updated_at;
            """;

        AddQueueItemParameters(command, item);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<SyncQueueItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            {SelectQueueItemSql}
            WHERE id = $id;
            """;

        command.Parameters.AddWithValue("$id", FormatGuid(id));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadQueueItem(reader);
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<SyncQueueItem> items = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            {SelectQueueItemSql}
            ORDER BY created_at, id;
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadQueueItem(reader));
        }

        return items;
    }

    public async Task<IReadOnlyList<SyncQueueItem>> GetReadyAsync(
        DateTimeOffset now,
        int limit = 25,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be at least 1.");
        }

        List<SyncQueueItem> items = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            $"""
            {SelectQueueItemSql}
            WHERE sync_state IN ($pendingState, $failedState)
              AND (next_attempt_at IS NULL OR next_attempt_at <= $now)
            ORDER BY
                CASE sync_state
                    WHEN 'Pending' THEN 0
                    WHEN 'Failed' THEN 1
                    ELSE 2
                END,
                COALESCE(next_attempt_at, created_at),
                created_at,
                id
            LIMIT $limit;
            """;

        command.Parameters.AddWithValue("$pendingState", SyncState.Pending.ToString());
        command.Parameters.AddWithValue("$failedState", SyncState.Failed.ToString());
        command.Parameters.AddWithValue("$now", FormatDateTimeOffset(now));
        command.Parameters.AddWithValue("$limit", limit);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(ReadQueueItem(reader));
        }

        return items;
    }

    public async Task<IReadOnlyDictionary<SyncState, int>> CountByStateAsync(
        CancellationToken cancellationToken = default)
    {
        Dictionary<SyncState, int> counts = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT sync_state, COUNT(*)
            FROM sync_queue
            GROUP BY sync_state;
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            counts[Enum.Parse<SyncState>(reader.GetString(0))] = Convert.ToInt32(reader.GetInt64(1));
        }

        return counts;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM sync_queue WHERE id = $id;";
        command.Parameters.AddWithValue("$id", FormatGuid(id));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private const string SelectQueueItemSql =
        """
        SELECT
            id,
            local_item_type,
            local_item_id,
            source_system,
            source_link_id,
            action_type,
            payload_json,
            sync_state,
            retry_count,
            next_attempt_at,
            last_attempted_at,
            failure_message,
            created_at,
            updated_at
        FROM sync_queue
        """;

    private static void AddQueueItemParameters(SqliteCommand command, SyncQueueItem item)
    {
        command.Parameters.AddWithValue("$id", FormatGuid(item.Id));
        command.Parameters.AddWithValue("$localItemType", item.LocalItemType.ToString());
        command.Parameters.AddWithValue("$localItemId", FormatGuid(item.LocalItemId));
        command.Parameters.AddWithValue("$sourceSystem", item.SourceSystem.ToString());
        command.Parameters.AddWithValue("$sourceLinkId", ToDbValue(item.SourceLinkId));
        command.Parameters.AddWithValue("$actionType", item.ActionType.ToString());
        command.Parameters.AddWithValue("$payloadJson", item.PayloadJson);
        command.Parameters.AddWithValue("$syncState", item.SyncState.ToString());
        command.Parameters.AddWithValue("$retryCount", item.RetryCount);
        command.Parameters.AddWithValue("$nextAttemptAt", ToDbValue(item.NextAttemptAt));
        command.Parameters.AddWithValue("$lastAttemptedAt", ToDbValue(item.LastAttemptedAt));
        command.Parameters.AddWithValue("$failureMessage", item.FailureMessage);
        command.Parameters.AddWithValue("$createdAt", FormatDateTimeOffset(item.CreatedAt));
        command.Parameters.AddWithValue("$updatedAt", FormatDateTimeOffset(item.UpdatedAt));
    }

    private static SyncQueueItem ReadQueueItem(SqliteDataReader reader)
    {
        return SyncQueueItem.Rehydrate(
            ParseGuid(reader.GetString(0)),
            Enum.Parse<SourceLinkItemType>(reader.GetString(1)),
            ParseGuid(reader.GetString(2)),
            Enum.Parse<SourceSystem>(reader.GetString(3)),
            Enum.Parse<SyncQueueActionType>(reader.GetString(5)),
            reader.GetString(6),
            Enum.Parse<SyncState>(reader.GetString(7)),
            reader.GetInt32(8),
            ParseDateTimeOffset(reader.GetString(12)),
            ParseDateTimeOffset(reader.GetString(13)),
            sourceLinkId: ReadGuid(reader, 4),
            nextAttemptAt: ReadDateTimeOffset(reader, 9),
            lastAttemptedAt: ReadDateTimeOffset(reader, 10),
            failureMessage: reader.GetString(11));
    }

    private static object ToDbValue(DateTimeOffset? value)
    {
        return value.HasValue ? FormatDateTimeOffset(value.Value) : DBNull.Value;
    }

    private static object ToDbValue(Guid? value)
    {
        return value.HasValue ? FormatGuid(value.Value) : DBNull.Value;
    }

    private static DateTimeOffset? ReadDateTimeOffset(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : ParseDateTimeOffset(reader.GetString(ordinal));
    }

    private static Guid? ReadGuid(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : ParseGuid(reader.GetString(ordinal));
    }

    private static string FormatGuid(Guid value)
    {
        return value.ToString("D");
    }

    private static Guid ParseGuid(string value)
    {
        return Guid.Parse(value);
    }

    private static string FormatDateTimeOffset(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("O");
    }

    private static DateTimeOffset ParseDateTimeOffset(string value)
    {
        return DateTimeOffset.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }
}
