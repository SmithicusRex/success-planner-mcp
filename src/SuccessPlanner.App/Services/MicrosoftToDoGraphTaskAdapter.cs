using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoGraphTaskAdapter
{
    private static readonly Uri DefaultGraphBaseUri = new("https://graph.microsoft.com/v1.0/");
    private const string TodoListsPath = "me/todo/lists?$select=id,displayName,wellknownListName&$top=50";

    private readonly HttpClient _httpClient;
    private readonly IMicrosoftToDoAccessTokenProvider _accessTokenProvider;
    private readonly Func<DateTimeOffset> _nowProvider;

    public MicrosoftToDoGraphTaskAdapter()
        : this(new HttpClient(), new NoMicrosoftToDoAccessTokenProvider())
    {
    }

    public MicrosoftToDoGraphTaskAdapter(
        HttpClient httpClient,
        IMicrosoftToDoAccessTokenProvider accessTokenProvider,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _accessTokenProvider = accessTokenProvider
            ?? throw new ArgumentNullException(nameof(accessTokenProvider));
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public async Task<MicrosoftToDoPullResult> PullListsAndTasksAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset checkedAt = _nowProvider();
        string? accessToken = await _accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new MicrosoftToDoPullResult(MicrosoftToDoConnectionStatus.NeedsSignIn(
                "Sign in to Microsoft To Do before pulling tasks.",
                checkedAt));
        }

        try
        {
            GraphReadResult listsRead = await ReadJsonAsync(
                TodoListsPath,
                accessToken,
                checkedAt,
                cancellationToken);
            if (listsRead.ConnectionStatus is not null)
            {
                return new MicrosoftToDoPullResult(listsRead.ConnectionStatus);
            }

            using JsonDocument listsDocument = listsRead.JsonDocument!;
            IReadOnlyList<MicrosoftToDoTaskList> taskLists =
                ReadTaskLists(listsDocument.RootElement);
            List<MicrosoftToDoTaskItem> tasks = [];

            foreach (MicrosoftToDoTaskList list in taskLists)
            {
                GraphReadResult tasksRead = await ReadJsonAsync(
                    BuildTasksPath(list.Id),
                    accessToken,
                    checkedAt,
                    cancellationToken);
                if (tasksRead.ConnectionStatus is not null)
                {
                    return new MicrosoftToDoPullResult(tasksRead.ConnectionStatus, taskLists, tasks);
                }

                using JsonDocument tasksDocument = tasksRead.JsonDocument!;
                tasks.AddRange(ReadTasks(list.Id, tasksDocument.RootElement));
            }

            return new MicrosoftToDoPullResult(
                MicrosoftToDoConnectionStatus.Connected(
                    checkedAt: checkedAt,
                    message: BuildSuccessMessage(taskLists.Count, tasks.Count)),
                taskLists,
                tasks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return new MicrosoftToDoPullResult(
                MicrosoftToDoConnectionStatus.Unavailable(BuildFailureMessage(ex), checkedAt));
        }
        catch (JsonException ex)
        {
            return new MicrosoftToDoPullResult(
                MicrosoftToDoConnectionStatus.Failed($"Microsoft To Do returned unreadable data: {ex.Message}", checkedAt));
        }
        catch (Exception ex)
        {
            return new MicrosoftToDoPullResult(
                MicrosoftToDoConnectionStatus.Failed(BuildFailureMessage(ex), checkedAt));
        }
    }

    private async Task<GraphReadResult> ReadJsonAsync(
        string relativePath,
        string accessToken,
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, BuildUri(relativePath));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new GraphReadResult(MapFailure(response.StatusCode, checkedAt), null);
        }

        await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        JsonDocument document = await JsonDocument.ParseAsync(responseStream, cancellationToken: cancellationToken);
        return new GraphReadResult(null, document);
    }

    private Uri BuildUri(string relativePath)
    {
        Uri baseUri = _httpClient.BaseAddress ?? DefaultGraphBaseUri;
        return new Uri(baseUri, relativePath);
    }

    private static string BuildTasksPath(string listId)
    {
        string escapedListId = Uri.EscapeDataString(listId);
        return $"me/todo/lists/{escapedListId}/tasks?$select=id,title,status,importance,body,dueDateTime,completedDateTime,lastModifiedDateTime,webLink&$top=50";
    }

    private static IReadOnlyList<MicrosoftToDoTaskList> ReadTaskLists(JsonElement root)
    {
        List<MicrosoftToDoTaskList> lists = [];

        foreach (JsonElement listElement in EnumerateValueArray(root))
        {
            string id = ReadString(listElement, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                continue;
            }

            lists.Add(new MicrosoftToDoTaskList(
                id,
                ReadString(listElement, "displayName"),
                ReadString(listElement, "wellknownListName")));
        }

        return lists;
    }

    private static IReadOnlyList<MicrosoftToDoTaskItem> ReadTasks(string listId, JsonElement root)
    {
        List<MicrosoftToDoTaskItem> tasks = [];

        foreach (JsonElement taskElement in EnumerateValueArray(root))
        {
            string id = ReadString(taskElement, "id");
            string title = ReadString(taskElement, "title");
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
            {
                continue;
            }

            tasks.Add(new MicrosoftToDoTaskItem(
                id,
                listId,
                title,
                ReadString(taskElement, "status"),
                ReadString(taskElement, "importance"),
                ReadBodyContent(taskElement),
                ReadDateTimeTimeZone(taskElement, "dueDateTime"),
                ReadDateTimeTimeZone(taskElement, "completedDateTime"),
                ReadDateTimeOffset(taskElement, "lastModifiedDateTime"),
                ReadString(taskElement, "webLink")));
        }

        return tasks;
    }

    private static IEnumerable<JsonElement> EnumerateValueArray(JsonElement root)
    {
        if (!root.TryGetProperty("value", out JsonElement valueElement)
            || valueElement.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (JsonElement item in valueElement.EnumerateArray())
        {
            yield return item;
        }
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)
            || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return string.Empty;
        }

        return property.GetString()?.Trim() ?? string.Empty;
    }

    private static string ReadBodyContent(JsonElement taskElement)
    {
        if (!taskElement.TryGetProperty("body", out JsonElement bodyElement)
            || bodyElement.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return ReadString(bodyElement, "content");
    }

    private static DateTimeOffset? ReadDateTimeTimeZone(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement dateTimeElement)
            || dateTimeElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return TryParseDateTimeOffset(ReadString(dateTimeElement, "dateTime"));
    }

    private static DateTimeOffset? ReadDateTimeOffset(JsonElement element, string propertyName)
    {
        return TryParseDateTimeOffset(ReadString(element, propertyName));
    }

    private static DateTimeOffset? TryParseDateTimeOffset(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out DateTimeOffset result)
            ? result
            : null;
    }

    private static MicrosoftToDoConnectionStatus MapFailure(HttpStatusCode statusCode, DateTimeOffset checkedAt)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                MicrosoftToDoConnectionStatus.NeedsSignIn(
                    "Sign in again to pull Microsoft To Do tasks.",
                    checkedAt),
            HttpStatusCode.NotFound =>
                MicrosoftToDoConnectionStatus.Unavailable(
                    "Microsoft To Do lists or tasks were not available for this account.",
                    checkedAt),
            HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError
                or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout =>
                MicrosoftToDoConnectionStatus.Unavailable(
                    $"Microsoft To Do is temporarily unavailable ({(int)statusCode}).",
                    checkedAt),
            _ => MicrosoftToDoConnectionStatus.Failed(
                $"Microsoft To Do pull returned HTTP {(int)statusCode}.",
                checkedAt)
        };
    }

    private static string BuildSuccessMessage(int listCount, int taskCount)
    {
        string listText = listCount == 1 ? "1 list" : $"{listCount} lists";
        string taskText = taskCount == 1 ? "1 task" : $"{taskCount} tasks";
        return $"Pulled {listText} and {taskText} from Microsoft To Do.";
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
    }

    private sealed record GraphReadResult(
        MicrosoftToDoConnectionStatus? ConnectionStatus,
        JsonDocument? JsonDocument);
}
