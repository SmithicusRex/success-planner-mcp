namespace SuccessPlanner.App.Services;

public interface IMicrosoftToDoConnectionProbe
{
    Task<MicrosoftToDoConnectionStatus> TestConnectionAsync(
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken = default);
}
