using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Services;

namespace SuccessPlanner.App.Infrastructure;

public sealed class SearchService
{
    private const int DefaultLimit = 50;
    private readonly AppPaths _paths;

    public SearchService(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task<IReadOnlyList<LocalSearchResult>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        string normalizedQuery = query.Trim();
        if (string.IsNullOrWhiteSpace(normalizedQuery))
        {
            return [];
        }

        string pattern = BuildLikePattern(normalizedQuery);
        List<LocalSearchResult> results = [];

        await using SqliteConnection connection = await SqliteConnectionFactory.OpenAsync(_paths, cancellationToken);
        await AddTaskResultsAsync(connection, pattern, results, cancellationToken);
        await AddNoteResultsAsync(connection, pattern, results, cancellationToken);
        await AddProjectResultsAsync(connection, pattern, results, cancellationToken);
        await AddSourceLinkResultsAsync(connection, pattern, results, cancellationToken);

        return results
            .OrderBy(result => KindSortValue(result.Kind))
            .ThenBy(result => result.Title, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(result => result.CreatedAt)
            .Take(DefaultLimit)
            .ToList();
    }

    private static async Task AddTaskResultsAsync(
        SqliteConnection connection,
        string pattern,
        List<LocalSearchResult> results,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, title, notes, status, priority, created_at
            FROM tasks
            WHERE title LIKE $pattern ESCAPE '\'
               OR notes LIKE $pattern ESCAPE '\'
            ORDER BY title COLLATE NOCASE
            LIMIT $limit;
            """;

        AddSearchParameters(command, pattern);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            Guid id = ParseGuid(reader.GetString(0));
            string title = reader.GetString(1);
            string notes = reader.GetString(2);
            string status = reader.GetString(3);
            string priority = reader.GetString(4);
            DateTimeOffset createdAt = ParseDateTimeOffset(reader.GetString(5));
            string detail = string.IsNullOrWhiteSpace(notes)
                ? $"{status} task - {priority} priority"
                : BuildPreview(notes);

            results.Add(new LocalSearchResult(
                LocalSearchResultKind.Task,
                id,
                title,
                detail,
                "Task",
                createdAt,
                LocalItemType: "Task",
                LocalItemId: id));
        }
    }

    private static async Task AddNoteResultsAsync(
        SqliteConnection connection,
        string pattern,
        List<LocalSearchResult> results,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, owner_type, owner_id, text, created_at
            FROM notes
            WHERE text LIKE $pattern ESCAPE '\'
            ORDER BY created_at DESC
            LIMIT $limit;
            """;

        AddSearchParameters(command, pattern);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            Guid id = ParseGuid(reader.GetString(0));
            string ownerType = reader.GetString(1);
            Guid? ownerId = reader.IsDBNull(2) ? null : ParseGuid(reader.GetString(2));
            string text = reader.GetString(3);
            DateTimeOffset createdAt = ParseDateTimeOffset(reader.GetString(4));

            results.Add(new LocalSearchResult(
                LocalSearchResultKind.Note,
                id,
                BuildPreview(text, 72),
                $"Attached to {ownerType}",
                "Note",
                createdAt,
                LocalItemType: ownerType,
                LocalItemId: ownerId));
        }
    }

    private static async Task AddProjectResultsAsync(
        SqliteConnection connection,
        string pattern,
        List<LocalSearchResult> results,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT id, name, notes, status, priority, immediate_need, minimum_win, created_at
            FROM projects
            WHERE name LIKE $pattern ESCAPE '\'
               OR notes LIKE $pattern ESCAPE '\'
               OR immediate_need LIKE $pattern ESCAPE '\'
               OR minimum_win LIKE $pattern ESCAPE '\'
            ORDER BY name COLLATE NOCASE
            LIMIT $limit;
            """;

        AddSearchParameters(command, pattern);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            Guid id = ParseGuid(reader.GetString(0));
            string name = reader.GetString(1);
            string notes = reader.GetString(2);
            string status = reader.GetString(3);
            string priority = reader.GetString(4);
            string immediateNeed = reader.GetString(5);
            string minimumWin = reader.GetString(6);
            DateTimeOffset createdAt = ParseDateTimeOffset(reader.GetString(7));
            string detail = FirstNonBlank(notes, immediateNeed, minimumWin, $"{status} project - {priority} priority");

            results.Add(new LocalSearchResult(
                LocalSearchResultKind.Project,
                id,
                name,
                BuildPreview(detail),
                "Project",
                createdAt,
                LocalItemType: "Project",
                LocalItemId: id));
        }
    }

    private static async Task AddSourceLinkResultsAsync(
        SqliteConnection connection,
        string pattern,
        List<LocalSearchResult> results,
        CancellationToken cancellationToken)
    {
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText =
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
                sync_state,
                created_at
            FROM source_links
            WHERE source_system LIKE $pattern ESCAPE '\'
               OR external_id LIKE $pattern ESCAPE '\'
               OR external_container_id LIKE $pattern ESCAPE '\'
               OR external_display_name LIKE $pattern ESCAPE '\'
               OR external_web_url LIKE $pattern ESCAPE '\'
            ORDER BY source_system COLLATE NOCASE, external_display_name COLLATE NOCASE
            LIMIT $limit;
            """;

        AddSearchParameters(command, pattern);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            Guid id = ParseGuid(reader.GetString(0));
            string localItemType = reader.GetString(1);
            Guid localItemId = ParseGuid(reader.GetString(2));
            string sourceSystem = reader.GetString(3);
            string externalId = reader.GetString(4);
            string externalContainerId = reader.GetString(5);
            string externalDisplayName = reader.GetString(6);
            string externalWebUrl = reader.GetString(7);
            string syncState = reader.GetString(8);
            DateTimeOffset createdAt = ParseDateTimeOffset(reader.GetString(9));
            string title = FirstNonBlank(externalDisplayName, externalId, $"{sourceSystem} source link");
            string detail = $"{sourceSystem} - {localItemType} - {syncState}";
            if (!string.IsNullOrWhiteSpace(externalContainerId))
            {
                detail = $"{detail} - {externalContainerId}";
            }

            results.Add(new LocalSearchResult(
                LocalSearchResultKind.SourceLink,
                id,
                title,
                detail,
                "Source Link",
                createdAt,
                LocalItemType: localItemType,
                LocalItemId: localItemId,
                ExternalWebUrl: externalWebUrl));
        }
    }

    private static void AddSearchParameters(SqliteCommand command, string pattern)
    {
        command.Parameters.AddWithValue("$pattern", pattern);
        command.Parameters.AddWithValue("$limit", DefaultLimit);
    }

    private static string BuildLikePattern(string query)
    {
        string escaped = query
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

        return $"%{escaped}%";
    }

    private static string FirstNonBlank(params string[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
    }

    private static string BuildPreview(string value, int maxLength = 120)
    {
        string trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : $"{trimmed[..(maxLength - 3)]}...";
    }

    private static int KindSortValue(LocalSearchResultKind kind)
    {
        return kind switch
        {
            LocalSearchResultKind.Task => 0,
            LocalSearchResultKind.Project => 1,
            LocalSearchResultKind.Note => 2,
            LocalSearchResultKind.SourceLink => 3,
            _ => 4
        };
    }

    private static Guid ParseGuid(string value)
    {
        return Guid.Parse(value);
    }

    private static DateTimeOffset ParseDateTimeOffset(string value)
    {
        return DateTimeOffset.Parse(value, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }
}
