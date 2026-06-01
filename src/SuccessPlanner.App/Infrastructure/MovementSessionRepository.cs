using System.Text.Json;
using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Infrastructure;

public sealed class MovementSessionRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);
    private readonly AppPaths _paths;

    public MovementSessionRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task SaveAsync(MovementSession session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO movement_sessions (
                id,
                activity_type,
                activity_name,
                status,
                task_id,
                planned_minutes,
                actual_minutes,
                created_at,
                scheduled_for,
                started_at,
                completed_at,
                ended_at,
                mind_occupier,
                is_with_spouse,
                notes,
                win_note,
                tags_json)
            VALUES (
                $id,
                $activityType,
                $activityName,
                $status,
                $taskId,
                $plannedMinutes,
                $actualMinutes,
                $createdAt,
                $scheduledFor,
                $startedAt,
                $completedAt,
                $endedAt,
                $mindOccupier,
                $isWithSpouse,
                $notes,
                $winNote,
                $tagsJson)
            ON CONFLICT(id) DO UPDATE SET
                activity_type = excluded.activity_type,
                activity_name = excluded.activity_name,
                status = excluded.status,
                task_id = excluded.task_id,
                planned_minutes = excluded.planned_minutes,
                actual_minutes = excluded.actual_minutes,
                scheduled_for = excluded.scheduled_for,
                started_at = excluded.started_at,
                completed_at = excluded.completed_at,
                ended_at = excluded.ended_at,
                mind_occupier = excluded.mind_occupier,
                is_with_spouse = excluded.is_with_spouse,
                notes = excluded.notes,
                win_note = excluded.win_note,
                tags_json = excluded.tags_json;
            """;

        AddMovementSessionParameters(command, session);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<MovementSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                activity_type,
                activity_name,
                status,
                task_id,
                planned_minutes,
                actual_minutes,
                created_at,
                scheduled_for,
                started_at,
                completed_at,
                ended_at,
                mind_occupier,
                is_with_spouse,
                notes,
                win_note,
                tags_json
            FROM movement_sessions
            WHERE id = $id;
            """;

        command.Parameters.AddWithValue("$id", FormatGuid(id));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadMovementSession(reader);
    }

    public async Task<IReadOnlyList<MovementSession>> GetRecentAsync(
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be at least 1.");
        }

        List<MovementSession> sessions = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                activity_type,
                activity_name,
                status,
                task_id,
                planned_minutes,
                actual_minutes,
                created_at,
                scheduled_for,
                started_at,
                completed_at,
                ended_at,
                mind_occupier,
                is_with_spouse,
                notes,
                win_note,
                tags_json
            FROM movement_sessions
            ORDER BY COALESCE(started_at, scheduled_for, created_at) DESC
            LIMIT $limit;
            """;

        command.Parameters.AddWithValue("$limit", limit);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            sessions.Add(ReadMovementSession(reader));
        }

        return sessions;
    }

    private static void AddMovementSessionParameters(SqliteCommand command, MovementSession session)
    {
        command.Parameters.AddWithValue("$id", FormatGuid(session.Id));
        command.Parameters.AddWithValue("$activityType", session.ActivityType.ToString());
        command.Parameters.AddWithValue("$activityName", session.ActivityName);
        command.Parameters.AddWithValue("$status", session.Status.ToString());
        command.Parameters.AddWithValue("$taskId", ToDbValue(session.TaskId));
        command.Parameters.AddWithValue("$plannedMinutes", session.PlannedMinutes);
        command.Parameters.AddWithValue("$actualMinutes", ToDbValue(session.ActualMinutes));
        command.Parameters.AddWithValue("$createdAt", FormatDateTimeOffset(session.CreatedAt));
        command.Parameters.AddWithValue("$scheduledFor", ToDbValue(session.ScheduledFor));
        command.Parameters.AddWithValue("$startedAt", ToDbValue(session.StartedAt));
        command.Parameters.AddWithValue("$completedAt", ToDbValue(session.CompletedAt));
        command.Parameters.AddWithValue("$endedAt", ToDbValue(session.EndedAt));
        command.Parameters.AddWithValue("$mindOccupier", session.MindOccupier);
        command.Parameters.AddWithValue("$isWithSpouse", session.IsWithSpouse ? 1 : 0);
        command.Parameters.AddWithValue("$notes", session.Notes);
        command.Parameters.AddWithValue("$winNote", session.WinNote);
        command.Parameters.AddWithValue("$tagsJson", JsonSerializer.Serialize(session.Tags, JsonOptions));
    }

    private static MovementSession ReadMovementSession(SqliteDataReader reader)
    {
        return MovementSession.Rehydrate(
            ParseGuid(reader.GetString(0)),
            Enum.Parse<MovementActivityType>(reader.GetString(1)),
            reader.GetString(2),
            reader.GetInt32(5),
            Enum.Parse<MovementSessionStatus>(reader.GetString(3)),
            ParseDateTimeOffset(reader.GetString(7)),
            taskId: ReadGuid(reader, 4),
            actualMinutes: ReadInt32(reader, 6),
            scheduledFor: ReadDateTimeOffset(reader, 8),
            startedAt: ReadDateTimeOffset(reader, 9),
            completedAt: ReadDateTimeOffset(reader, 10),
            endedAt: ReadDateTimeOffset(reader, 11),
            mindOccupier: reader.GetString(12),
            isWithSpouse: reader.GetInt32(13) == 1,
            notes: reader.GetString(14),
            winNote: reader.GetString(15),
            tags: ReadTags(reader.GetString(16)));
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
