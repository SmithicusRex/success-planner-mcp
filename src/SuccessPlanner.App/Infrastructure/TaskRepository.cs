using System.Text.Json;
using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Infrastructure;

public sealed class TaskRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);
    private readonly AppPaths _paths;

    public TaskRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO tasks (
                id,
                title,
                notes,
                status,
                priority,
                due_date,
                start_date,
                created_at,
                completed_at,
                project_id,
                estimated_minutes,
                energy_level,
                is_tiny_step,
                is_physical_activity,
                tags_json)
            VALUES (
                $id,
                $title,
                $notes,
                $status,
                $priority,
                $dueDate,
                $startDate,
                $createdAt,
                $completedAt,
                $projectId,
                $estimatedMinutes,
                $energyLevel,
                $isTinyStep,
                $isPhysicalActivity,
                $tagsJson);
            """;

        AddTaskParameters(command, task);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO tasks (
                id,
                title,
                notes,
                status,
                priority,
                due_date,
                start_date,
                created_at,
                completed_at,
                project_id,
                estimated_minutes,
                energy_level,
                is_tiny_step,
                is_physical_activity,
                tags_json)
            VALUES (
                $id,
                $title,
                $notes,
                $status,
                $priority,
                $dueDate,
                $startDate,
                $createdAt,
                $completedAt,
                $projectId,
                $estimatedMinutes,
                $energyLevel,
                $isTinyStep,
                $isPhysicalActivity,
                $tagsJson)
            ON CONFLICT(id) DO UPDATE SET
                title = excluded.title,
                notes = excluded.notes,
                status = excluded.status,
                priority = excluded.priority,
                due_date = excluded.due_date,
                start_date = excluded.start_date,
                completed_at = excluded.completed_at,
                project_id = excluded.project_id,
                estimated_minutes = excluded.estimated_minutes,
                energy_level = excluded.energy_level,
                is_tiny_step = excluded.is_tiny_step,
                is_physical_activity = excluded.is_physical_activity,
                tags_json = excluded.tags_json;
            """;

        AddTaskParameters(command, task);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                title,
                notes,
                status,
                priority,
                due_date,
                start_date,
                created_at,
                completed_at,
                project_id,
                estimated_minutes,
                energy_level,
                is_tiny_step,
                is_physical_activity,
                tags_json
            FROM tasks
            WHERE id = $id;
            """;

        command.Parameters.AddWithValue("$id", FormatGuid(id));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadTask(reader);
    }

    public async Task<IReadOnlyList<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<TaskItem> tasks = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                title,
                notes,
                status,
                priority,
                due_date,
                start_date,
                created_at,
                completed_at,
                project_id,
                estimated_minutes,
                energy_level,
                is_tiny_step,
                is_physical_activity,
                tags_json
            FROM tasks
            ORDER BY created_at, title;
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            tasks.Add(ReadTask(reader));
        }

        return tasks;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = "DELETE FROM tasks WHERE id = $id;";
        command.Parameters.AddWithValue("$id", FormatGuid(id));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddTaskParameters(SqliteCommand command, TaskItem task)
    {
        command.Parameters.AddWithValue("$id", FormatGuid(task.Id));
        command.Parameters.AddWithValue("$title", task.Title);
        command.Parameters.AddWithValue("$notes", task.Notes);
        command.Parameters.AddWithValue("$status", task.Status.ToString());
        command.Parameters.AddWithValue("$priority", task.Priority.ToString());
        command.Parameters.AddWithValue("$dueDate", ToDbValue(task.DueDate));
        command.Parameters.AddWithValue("$startDate", ToDbValue(task.StartDate));
        command.Parameters.AddWithValue("$createdAt", FormatDateTimeOffset(task.CreatedAt));
        command.Parameters.AddWithValue("$completedAt", ToDbValue(task.CompletedAt));
        command.Parameters.AddWithValue("$projectId", ToDbValue(task.ProjectId));
        command.Parameters.AddWithValue("$estimatedMinutes", ToDbValue(task.EstimatedMinutes));
        command.Parameters.AddWithValue("$energyLevel", task.EnergyLevel);
        command.Parameters.AddWithValue("$isTinyStep", task.IsTinyStep ? 1 : 0);
        command.Parameters.AddWithValue("$isPhysicalActivity", task.IsPhysicalActivity ? 1 : 0);
        command.Parameters.AddWithValue("$tagsJson", JsonSerializer.Serialize(task.Tags, JsonOptions));
    }

    private static TaskItem ReadTask(SqliteDataReader reader)
    {
        return TaskItem.Rehydrate(
            ParseGuid(reader.GetString(0)),
            reader.GetString(1),
            ParseDateTimeOffset(reader.GetString(7)),
            Enum.Parse<TaskItemStatus>(reader.GetString(3)),
            Enum.Parse<TaskPriority>(reader.GetString(4)),
            notes: reader.GetString(2),
            dueDate: ReadDateOnly(reader, 5),
            startDate: ReadDateOnly(reader, 6),
            completedAt: ReadDateTimeOffset(reader, 8),
            projectId: ReadGuid(reader, 9),
            estimatedMinutes: ReadInt32(reader, 10),
            energyLevel: reader.GetString(11),
            isTinyStep: reader.GetInt32(12) == 1,
            isPhysicalActivity: reader.GetInt32(13) == 1,
            tags: ReadTags(reader.GetString(14)));
    }

    private static IReadOnlyList<string> ReadTags(string tagsJson)
    {
        return JsonSerializer.Deserialize<IReadOnlyList<string>>(tagsJson, JsonOptions) ?? [];
    }

    private static object ToDbValue(DateOnly? value)
    {
        return value.HasValue ? value.Value.ToString("yyyy-MM-dd") : DBNull.Value;
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

    private static DateOnly? ReadDateOnly(SqliteDataReader reader, int ordinal)
    {
        return reader.IsDBNull(ordinal) ? null : DateOnly.Parse(reader.GetString(ordinal));
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
