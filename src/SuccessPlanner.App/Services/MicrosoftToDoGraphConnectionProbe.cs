using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoGraphConnectionProbe : IMicrosoftToDoConnectionProbe
{
    private static readonly Uri DefaultGraphBaseUri = new("https://graph.microsoft.com/v1.0/");
    private const string TodoListsPath = "me/todo/lists?$top=1";

    private readonly HttpClient _httpClient;
    private readonly IMicrosoftToDoAccessTokenProvider _accessTokenProvider;

    public MicrosoftToDoGraphConnectionProbe()
        : this(new HttpClient(), new NoMicrosoftToDoAccessTokenProvider())
    {
    }

    public MicrosoftToDoGraphConnectionProbe(
        HttpClient httpClient,
        IMicrosoftToDoAccessTokenProvider accessTokenProvider)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _accessTokenProvider = accessTokenProvider
            ?? throw new ArgumentNullException(nameof(accessTokenProvider));
    }

    public async Task<MicrosoftToDoConnectionStatus> TestConnectionAsync(
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? accessToken = await _accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return MicrosoftToDoConnectionStatus.NeedsSignIn(
                "Sign in to Microsoft To Do before testing the connection.",
                checkedAt);
        }

        using HttpRequestMessage request = new(HttpMethod.Get, BuildTodoListsUri());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return MicrosoftToDoConnectionStatus.Connected(checkedAt: checkedAt);
        }

        return MapFailure(response.StatusCode, checkedAt);
    }

    private Uri BuildTodoListsUri()
    {
        Uri baseUri = _httpClient.BaseAddress ?? DefaultGraphBaseUri;
        return new Uri(baseUri, TodoListsPath);
    }

    private static MicrosoftToDoConnectionStatus MapFailure(HttpStatusCode statusCode, DateTimeOffset checkedAt)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                MicrosoftToDoConnectionStatus.NeedsSignIn(
                    "Sign in again to use Microsoft To Do.",
                    checkedAt),
            HttpStatusCode.NotFound =>
                MicrosoftToDoConnectionStatus.Unavailable(
                    "Microsoft To Do task lists were not available for this account.",
                    checkedAt),
            HttpStatusCode.TooManyRequests or HttpStatusCode.InternalServerError
                or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout =>
                MicrosoftToDoConnectionStatus.Unavailable(
                    $"Microsoft To Do is temporarily unavailable ({(int)statusCode}).",
                    checkedAt),
            _ => MicrosoftToDoConnectionStatus.Failed(
                $"Microsoft To Do connection test returned HTTP {(int)statusCode}.",
                checkedAt)
        };
    }
}
