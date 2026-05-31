using System.Text.Json;
using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Infrastructure;

public sealed class FocusSessionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);
    private readonly AppPaths _paths;

    public FocusSessionRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task SaveAsync(FocusSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO focus_sessions (
                id,
                task_id,
                intention,
                planned_minutes,
                status,
                started_at,
                paused_at,
                completed_at,
                ended_at,
                total_paused_minutes,
                actual_focus_minutes,
                win_note,
                blocked_reason,
                tags_json)
            VALUES (
                $id,
                $taskId,
                $intention,
                $plannedMinutes,
                $status,
                $startedAt,
                $pausedAt,
                $completedAt,
                $endedAt,
                $totalPausedMinutes,
                $actualFocusMinutes,
                $winNote,
                $blockedReason,
                $tagsJson)
            ON CONFLICT(id) DO UPDATE SET
                task_id = excluded.task_id,
                intention = excluded.intention,
                planned_minutes = excluded.planned_minutes,
                status = excluded.status,
                paused_at = excluded.paused_at,
                completed_at = excluded.completed_at,
                ended_at = excluded.ended_at,
                total_paused_minutes = excluded.total_paused_minutes,
                actual_focus_minutes = excluded.actual_focus_minutes,
                win_note = excluded.win_note,
                blocked_reason = excluded.blocked_reason,
                tags_json = excluded.tags_json;
            """;

        AddFocusSessionParameters(command, session);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<FocusSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                task_id,
                intention,
                planned_minutes,
                status,
                started_at,
                paused_at,
                completed_at,
                ended_at,
                total_paused_minutes,
                actual_focus_minutes,
                win_note,
                blocked_reason,
                tags_json
            FROM focus_sessions
            WHERE id = $id;
            """;

        command.Parameters.AddWithValue("$id", FormatGuid(id));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadFocusSession(reader);
    }

    public async Task<IReadOnlyList<FocusSession>> GetForTaskAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        List<FocusSession> sessions = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                task_id,
                intention,
                planned_minutes,
                status,
                started_at,
                paused_at,
                completed_at,
                ended_at,
                total_paused_minutes,
                actual_focus_minutes,
                win_note,
                blocked_reason,
                tags_json
            FROM focus_sessions
            WHERE task_id = $taskId
            ORDER BY started_at DESC;
            """;

        command.Parameters.AddWithValue("$taskId", FormatGuid(taskId));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sessions.Add(ReadFocusSession(reader));
        }

        return sessions;
    }

    public async Task<IReadOnlyList<FocusSession>> GetRecentAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be at least 1.");
        }

        List<FocusSession> sessions = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                task_id,
                intention,
                planned_minutes,
                status,
                started_at,
                paused_at,
                completed_at,
                ended_at,
                total_paused_minutes,
                actual_focus_minutes,
                win_note,
                blocked_reason,
                tags_json
            FROM focus_sessions
            ORDER BY started_at DESC
            LIMIT $limit;
            """;

        command.Parameters.AddWithValue("$limit", limit);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sessions.Add(ReadFocusSession(reader));
        }

        return sessions;
    }

    private static void AddFocusSessionParameters(SqliteCommand command, FocusSession session)
    {
        command.Parameters.AddWithValue("$id", FormatGuid(session.Id));
        command.Parameters.AddWithValue("$taskId", ToDbValue(session.TaskId));
        command.Parameters.AddWithValue("$intention", session.Intention);
        command.Parameters.AddWithValue("$plannedMinutes", session.PlannedMinutes);
        command.Parameters.AddWithValue("$status", session.Status.ToString());
        command.Parameters.AddWithValue("$startedAt", FormatDateTimeOffset(session.StartedAt));
        command.Parameters.AddWithValue("$pausedAt", ToDbValue(session.PausedAt));
        command.Parameters.AddWithValue("$completedAt", ToDbValue(session.CompletedAt));
        command.Parameters.AddWithValue("$endedAt", ToDbValue(session.EndedAt));
        command.Parameters.AddWithValue("$totalPausedMinutes", session.TotalPausedMinutes);
        command.Parameters.AddWithValue("$actualFocusMinutes", ToDbValue(session.ActualFocusMinutes));
        command.Parameters.AddWithValue("$winNote", session.WinNote);
        command.Parameters.AddWithValue("$blockedReason", session.BlockedReason);
        command.Parameters.AddWithValue("$tagsJson", JsonSerializer.Serialize(session.Tags, JsonOptions));
    }

    private static FocusSession ReadFocusSession(SqliteDataReader reader)
    {
        return FocusSession.Rehydrate(
            ParseGuid(reader.GetString(0)),
            ReadGuid(reader, 1),
            reader.GetString(2),
            reader.GetInt32(3),
            Enum.Parse<FocusSessionStatus>(reader.GetString(4)),
            ParseDateTimeOffset(reader.GetString(5)),
            pausedAt: ReadDateTimeOffset(reader, 6),
            completedAt: ReadDateTimeOffset(reader, 7),
            endedAt: ReadDateTimeOffset(reader, 8),
            totalPausedMinutes: reader.GetInt32(9),
            actualFocusMinutes: ReadInt32(reader, 10),
            winNote: reader.GetString(11),
            blockedReason: reader.GetString(12),
            tags: ReadTags(reader.GetString(13)));
    }

    private static IReadOnlyList<string> ReadTags(string tagsJson)
    {
        return JsonSerializer.Deserialize<IReadOnlyList<string>>(tagsJson, JsonOptions) ?? [];
    }

    private static object ToDbValue(DateTimeOffset? value)
    {
        return value.HasValue ? FormatDateTimeOffset(value.Value) : DBNull.Value;
    }

    private static object ToDbValue(Guid? value)
    {
        return value.HasValue ? FormatGuid(value.Value) : DBNull.Value;
    }

    private static object ToDbValue(int? value)
    {
        return value.HasValue ? value.Value : DBNull.Value;
    }

    private static DateTimeOffset? ReadDateTimeOffset(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : ParseDateTimeOffset(reader.GetString(ordinal));
    }

    private static Guid? ReadGuid(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : ParseGuid(reader.GetString(ordinal));
    }

    private static int? ReadInt32(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
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
