namespace SuccessPlanner.App.Services;

public sealed class SyncRetryPolicy
{
    private static readonly TimeSpan[] BuiltInDelays =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromHours(1)
    ];

    private readonly IReadOnlyList<TimeSpan> _retryDelays;

    public SyncRetryPolicy()
        : this(BuiltInDelays)
    {
    }

    public SyncRetryPolicy(IEnumerable<TimeSpan> retryDelays)
    {
        ArgumentNullException.ThrowIfNull(retryDelays);

        TimeSpan[] effectiveDelays = retryDelays.ToArray();
        if (effectiveDelays.Length == 0)
        {
            throw new ArgumentException("At least one retry delay is required.", nameof(retryDelays));
        }

        if (effectiveDelays.Any(delay => delay <= TimeSpan.Zero))
        {
            throw new ArgumentException("Retry delays must be greater than zero.", nameof(retryDelays));
        }

        _retryDelays = effectiveDelays;
    }

    public DateTimeOffset GetNextAttemptAt(DateTimeOffset failedAt, int retryCountAfterFailure)
    {
        return failedAt.Add(GetDelay(retryCountAfterFailure));
    }

    public TimeSpan GetDelay(int retryCountAfterFailure)
    {
        if (retryCountAfterFailure < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retryCountAfterFailure),
                "Retry count after failure must be at least one.");
        }

        int delayIndex = Math.Min(retryCountAfterFailure - 1, _retryDelays.Count - 1);
        return _retryDelays[delayIndex];
    }
}
