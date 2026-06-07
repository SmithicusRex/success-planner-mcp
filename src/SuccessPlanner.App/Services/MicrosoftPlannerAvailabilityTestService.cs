using System.Net.Http;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerAvailabilityTestService
{
    private readonly IMicrosoftPlannerAvailabilityProbe _availabilityProbe;
    private readonly Func<DateTimeOffset> _nowProvider;

    public MicrosoftPlannerAvailabilityTestService()
        : this(new MicrosoftPlannerGraphAvailabilityProbe(), () => DateTimeOffset.Now)
    {
    }

    public MicrosoftPlannerAvailabilityTestService(
        IMicrosoftPlannerAvailabilityProbe availabilityProbe,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _availabilityProbe = availabilityProbe ?? throw new ArgumentNullException(nameof(availabilityProbe));
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public MicrosoftPlannerConnectionStatus GetInitialStatus(ConnectionSettings connectionSettings)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings);

        return connectionSettings.EnablePlanner
            ? MicrosoftPlannerConnectionStatus.NotConnected()
            : MicrosoftPlannerConnectionStatus.Disabled();
    }

    public async Task<MicrosoftPlannerConnectionStatus> TestAvailabilityAsync(
        ConnectionSettings connectionSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings);
        cancellationToken.ThrowIfCancellationRequested();

        if (!connectionSettings.EnablePlanner)
        {
            return MicrosoftPlannerConnectionStatus.Disabled();
        }

        DateTimeOffset checkedAt = _nowProvider();
        try
        {
            return await _availabilityProbe.TestAvailabilityAsync(checkedAt, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return MicrosoftPlannerConnectionStatus.Unavailable(BuildFailureMessage(ex), checkedAt);
        }
        catch (Exception ex)
        {
            return MicrosoftPlannerConnectionStatus.Failed(BuildFailureMessage(ex), checkedAt);
        }
    }

    private static string BuildFailureMessage(Exception exception)
    {
        if (!string.IsNullOrWhiteSpace(exception.Message))
        {
            return exception.Message.Trim();
        }

        return exception.GetType().Name;
    }
}
