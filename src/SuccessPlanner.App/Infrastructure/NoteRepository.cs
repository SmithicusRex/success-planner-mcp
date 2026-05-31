using System.Text.Json;
using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Infrastructure;

public sealed class NoteRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.General);
    private readonly AppPaths _paths;

    public NoteRepository(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task AddAsync(NoteItem note, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO notes (
                id,
                owner_type,
                owner_id,
                text,
                created_at,
                updated_at,
                is_pinned,
                is_review_highlight,
                tags_json)
            VALUES (
                $id,
                $ownerType,
                $ownerId,
                $text,
                $createdAt,
                $updatedAt,
                $isPinned,
                $isReviewHighlight,
                $tagsJson);
            """;

        AddNoteParameters(command, note);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<NoteItem> AddTaskSmallWinAsync(
        TaskItem task,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(task);
        string text = BuildTaskSmallWinText(task);

        NoteItem? existing = await GetExistingTaskSmallWinAsync(task.Id, text, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        NoteItem note = NoteItem.Create(NoteOwnerType.Task, task.Id, text);
        note.MarkReviewHighlight();
        note.AddTag("Win");
        note.AddTag("Small Win");

        await AddAsync(note, cancellationToken);
        return note;
    }

    public async Task<NoteItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                owner_type,
                owner_id,
                text,
                created_at,
                updated_at,
                is_pinned,
                is_review_highlight,
                tags_json
            FROM notes
            WHERE id = $id;
            """;

        command.Parameters.AddWithValue("$id", FormatGuid(id));

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadNote(reader);
    }

    public async Task<IReadOnlyList<NoteItem>> GetForOwnerAsync(
        NoteOwnerType ownerType,
        Guid? ownerId,
        CancellationToken cancellationToken = default)
    {
        List<NoteItem> notes = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            ownerId.HasValue
                ? """
                  SELECT
                      id,
                      owner_type,
                      owner_id,
                      text,
                      created_at,
                      updated_at,
                      is_pinned,
                      is_review_highlight,
                      tags_json
                  FROM notes
                  WHERE owner_type = $ownerType
                    AND owner_id = $ownerId
                  ORDER BY created_at DESC;
                  """
                : """
                  SELECT
                      id,
                      owner_type,
                      owner_id,
                      text,
                      created_at,
                      updated_at,
                      is_pinned,
                      is_review_highlight,
                      tags_json
                  FROM notes
                  WHERE owner_type = $ownerType
                    AND owner_id IS NULL
                  ORDER BY created_at DESC;
                  """;

        command.Parameters.AddWithValue("$ownerType", ownerType.ToString());
        if (ownerId.HasValue)
        {
            command.Parameters.AddWithValue("$ownerId", FormatGuid(ownerId.Value));
        }

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            notes.Add(ReadNote(reader));
        }

        return notes;
    }

    public async Task<IReadOnlyList<NoteItem>> GetReviewHighlightsAsync(CancellationToken cancellationToken = default)
    {
        List<NoteItem> notes = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                owner_type,
                owner_id,
                text,
                created_at,
                updated_at,
                is_pinned,
                is_review_highlight,
                tags_json
            FROM notes
            WHERE is_review_highlight = 1
            ORDER BY created_at DESC;
            """;

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            notes.Add(ReadNote(reader));
        }

        return notes;
    }

    private async Task<NoteItem?> GetExistingTaskSmallWinAsync(
        Guid taskId,
        string text,
        CancellationToken cancellationToken)
    {
        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT
                id,
                owner_type,
                owner_id,
                text,
                created_at,
                updated_at,
                is_pinned,
                is_review_highlight,
                tags_json
            FROM notes
            WHERE owner_type = $ownerType
              AND owner_id = $ownerId
              AND text = $text
              AND is_review_highlight = 1
            ORDER BY created_at
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("$ownerType", NoteOwnerType.Task.ToString());
        command.Parameters.AddWithValue("$ownerId", FormatGuid(taskId));
        command.Parameters.AddWithValue("$text", text);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadNote(reader);
    }

    private static string BuildTaskSmallWinText(TaskItem task)
    {
        return $"Small win: {task.Title}";
    }

    private static void AddNoteParameters(SqliteCommand command, NoteItem note)
    {
        command.Parameters.AddWithValue("$id", FormatGuid(note.Id));
        command.Parameters.AddWithValue("$ownerType", note.OwnerType.ToString());
        command.Parameters.AddWithValue("$ownerId", ToDbValue(note.OwnerId));
        command.Parameters.AddWithValue("$text", note.Text);
        command.Parameters.AddWithValue("$createdAt", FormatDateTimeOffset(note.CreatedAt));
        command.Parameters.AddWithValue("$updatedAt", FormatDateTimeOffset(note.UpdatedAt));
        command.Parameters.AddWithValue("$isPinned", note.IsPinned ? 1 : 0);
        command.Parameters.AddWithValue("$isReviewHighlight", note.IsReviewHighlight ? 1 : 0);
        command.Parameters.AddWithValue("$tagsJson", JsonSerializer.Serialize(note.Tags, JsonOptions));
    }

    private static NoteItem ReadNote(SqliteDataReader reader)
    {
        return NoteItem.Rehydrate(
            ParseGuid(reader.GetString(0)),
            Enum.Parse<NoteOwnerType>(reader.GetString(1)),
            ReadGuid(reader, 2),
            reader.GetString(3),
            ParseDateTimeOffset(reader.GetString(4)),
            ParseDateTimeOffset(reader.GetString(5)),
            isPinned: reader.GetInt32(6) == 1,
            isReviewHighlight: reader.GetInt32(7) == 1,
            tags: ReadTags(reader.GetString(8)));
    }

    private static IReadOnlyList<string> ReadTags(string tagsJson)
    {
        return JsonSerializer.Deserialize<IReadOnlyList<string>>(tagsJson, JsonOptions) ?? [];
    }

    private static object ToDbValue(Guid? value)
    {
        return value.HasValue ? FormatGuid(value.Value) : DBNull.Value;
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
