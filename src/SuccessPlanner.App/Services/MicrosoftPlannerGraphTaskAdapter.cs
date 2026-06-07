using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerGraphTaskAdapter : IMicrosoftPlannerTaskAdapter
{
    private static readonly Uri DefaultGraphBaseUri = new("https://graph.microsoft.com/v1.0/");
    private const string PlannerTasksPath = "me/planner/tasks?$top=50";

    private readonly HttpClient _httpClient;
    private readonly IMicrosoftPlannerAccessTokenProvider _accessTokenProvider;
    private readonly Func<DateTimeOffset> _nowProvider;

    public MicrosoftPlannerGraphTaskAdapter()
        : this(new HttpClient(), new NoMicrosoftPlannerAccessTokenProvider())
    {
    }

    public MicrosoftPlannerGraphTaskAdapter(
        HttpClient httpClient,
        IMicrosoftPlannerAccessTokenProvider accessTokenProvider,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _accessTokenProvider = accessTokenProvider
            ?? throw new ArgumentNullException(nameof(accessTokenProvider));
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public async Task<MicrosoftPlannerPullResult> PullAssignedTasksAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DateTimeOffset checkedAt = _nowProvider();
        string? accessToken = await _accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new MicrosoftPlannerPullResult(MicrosoftPlannerConnectionStatus.NeedsSignIn(
                "Sign in before importing Microsoft Planner tasks.",
                checkedAt));
        }

        try
        {
            using HttpRequestMessage request = new(HttpMethod.Get, BuildPlannerTasksUri());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new MicrosoftPlannerPullResult(MapFailure(response.StatusCode, checkedAt));
            }

            await using Stream responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using JsonDocument document = await JsonDocument.ParseAsync(
                responseStream,
                cancellationToken: cancellationToken);
            IReadOnlyList<MicrosoftPlannerTaskItem> tasks = ReadTasks(document.RootElement);

            return new MicrosoftPlannerPullResult(
                MicrosoftPlannerConnectionStatus.Available(
                    checkedAt: checkedAt,
                    message: BuildSuccessMessage(tasks.Count)),
                tasks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return new MicrosoftPlannerPullResult(
                MicrosoftPlannerConnectionStatus.Unavailable(BuildFailureMessage(ex), checkedAt));
        }
        catch (JsonException ex)
        {
            return new MicrosoftPlannerPullResult(
                MicrosoftPlannerConnectionStatus.Failed($"Microsoft Planner returned unreadable data: {ex.Message}", checkedAt));
        }
        catch (Exception ex)
        {
            return new MicrosoftPlannerPullResult(
                MicrosoftPlannerConnectionStatus.Failed(BuildFailureMessage(ex), checkedAt));
        }
    }

    private Uri BuildPlannerTasksUri()
    {
        Uri baseUri = _httpClient.BaseAddress ?? DefaultGraphBaseUri;
        return new Uri(baseUri, PlannerTasksPath);
    }

    private static IReadOnlyList<MicrosoftPlannerTaskItem> ReadTasks(JsonElement root)
    {
        List<MicrosoftPlannerTaskItem> tasks = [];

        foreach (JsonElement taskElement in EnumerateValueArray(root))
        {
            MicrosoftPlannerTaskItem? task = ReadTask(taskElement);
            if (task is not null)
            {
                tasks.Add(task);
            }
        }

        return tasks;
    }

    private static MicrosoftPlannerTaskItem? ReadTask(JsonElement taskElement)
    {
        string id = ReadString(taskElement, "id");
        string title = ReadString(taskElement, "title");
        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(title))
        {
            return null;
        }

        return new MicrosoftPlannerTaskItem(
            id,
            title,
            ReadString(taskElement, "planId"),
            ReadString(taskElement, "bucketId"),
            ReadInt32(taskElement, "percentComplete"),
            ReadInt32(taskElement, "priority"),
            ReadDateTimeOffset(taskElement, "createdDateTime"),
            ReadDateTimeOffset(taskElement, "startDateTime"),
            ReadDateTimeOffset(taskElement, "dueDateTime"),
            ReadDateTimeOffset(taskElement, "completedDateTime"),
            ReadString(taskElement, "webUrl"));
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

    private static int? ReadInt32(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement property)
            || property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return property.TryGetInt32(out int value) ? value : null;
    }

    private static DateTimeOffset? ReadDateTimeOffset(JsonElement element, string propertyName)
    {
        string value = ReadString(element, propertyName);
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

    private static MicrosoftPlannerConnectionStatus MapFailure(
        HttpStatusCode statusCode,
        DateTimeOffset checkedAt)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized =>
                MicrosoftPlannerConnectionStatus.NeedsSignIn(
                    "Sign in again before importing Microsoft Planner tasks.",
                    checkedAt),
            HttpStatusCode.Forbidden or HttpStatusCode.NotFound or HttpStatusCode.BadRequest =>
                MicrosoftPlannerConnectionStatus.Unavailable(
                    "Planner data was not available for this account. It may require a work or school account, Planner license, or accessible plans.",
                    checkedAt),
            HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError
                or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout =>
                MicrosoftPlannerConnectionStatus.Unavailable(
                    $"Microsoft Planner is temporarily unavailable ({(int)statusCode}).",
                    checkedAt),
            _ => MicrosoftPlannerConnectionStatus.Failed(
                $"Microsoft Planner import returned HTTP {(int)statusCode}.",
                checkedAt)
        };
    }

    private static string BuildSuccessMessage(int taskCount)
    {
        return taskCount == 1
            ? "Pulled 1 assigned Planner task."
            : $"Pulled {taskCount} assigned Planner tasks.";
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
    }
}
