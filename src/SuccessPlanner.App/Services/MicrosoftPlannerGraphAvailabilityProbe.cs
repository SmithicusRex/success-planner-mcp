using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerGraphAvailabilityProbe : IMicrosoftPlannerAvailabilityProbe
{
    private static readonly Uri DefaultGraphBaseUri = new("https://graph.microsoft.com/v1.0/");
    private const string PlannerTasksPath = "me/planner/tasks?$top=1";

    private readonly HttpClient _httpClient;
    private readonly IMicrosoftPlannerAccessTokenProvider _accessTokenProvider;

    public MicrosoftPlannerGraphAvailabilityProbe()
        : this(new HttpClient(), new NoMicrosoftPlannerAccessTokenProvider())
    {
    }

    public MicrosoftPlannerGraphAvailabilityProbe(
        HttpClient httpClient,
        IMicrosoftPlannerAccessTokenProvider accessTokenProvider)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _accessTokenProvider = accessTokenProvider
            ?? throw new ArgumentNullException(nameof(accessTokenProvider));
    }

    public async Task<MicrosoftPlannerConnectionStatus> TestAvailabilityAsync(
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string? accessToken = await _accessTokenProvider.GetAccessTokenAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return MicrosoftPlannerConnectionStatus.NeedsSignIn(
                "Sign in before checking Microsoft Planner availability.",
                checkedAt);
        }

        using HttpRequestMessage request = new(HttpMethod.Get, BuildPlannerTasksUri());
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Trim());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return MicrosoftPlannerConnectionStatus.Available(
                checkedAt: checkedAt,
                message: "Microsoft Planner task data is available for this account.");
        }

        return MapFailure(response.StatusCode, checkedAt);
    }

    private Uri BuildPlannerTasksUri()
    {
        Uri baseUri = _httpClient.BaseAddress ?? DefaultGraphBaseUri;
        return new Uri(baseUri, PlannerTasksPath);
    }

    private static MicrosoftPlannerConnectionStatus MapFailure(
        HttpStatusCode statusCode,
        DateTimeOffset checkedAt)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized =>
                MicrosoftPlannerConnectionStatus.NeedsSignIn(
                    "Sign in again before checking Microsoft Planner.",
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
                $"Microsoft Planner availability check returned HTTP {(int)statusCode}.",
                checkedAt)
        };
    }
}
