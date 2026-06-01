using System.Net.Http;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoConnectionTestService
{
    private readonly IMicrosoftToDoConnectionProbe _connectionProbe;
    private readonly Func<DateTimeOffset> _nowProvider;

    public MicrosoftToDoConnectionTestService()
        : this(new MicrosoftToDoGraphConnectionProbe(), () => DateTimeOffset.Now)
    {
    }

    public MicrosoftToDoConnectionTestService(
        IMicrosoftToDoConnectionProbe connectionProbe,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _connectionProbe = connectionProbe ?? throw new ArgumentNullException(nameof(connectionProbe));
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public MicrosoftToDoConnectionStatus GetInitialStatus(ConnectionSettings connectionSettings)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings);

        return connectionSettings.EnableMicrosoftToDo
            ? MicrosoftToDoConnectionStatus.NotConnected()
            : MicrosoftToDoConnectionStatus.Disabled();
    }

    public async Task<MicrosoftToDoConnectionStatus> TestConnectionAsync(
        ConnectionSettings connectionSettings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionSettings);
        cancellationToken.ThrowIfCancellationRequested();

        if (!connectionSettings.EnableMicrosoftToDo)
        {
            return MicrosoftToDoConnectionStatus.Disabled();
        }

        DateTimeOffset checkedAt = _nowProvider();
        try
        {
            return await _connectionProbe.TestConnectionAsync(checkedAt, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            return MicrosoftToDoConnectionStatus.Unavailable(BuildFailureMessage(ex), checkedAt);
        }
        catch (Exception ex)
        {
            return MicrosoftToDoConnectionStatus.Failed(BuildFailureMessage(ex), checkedAt);
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
