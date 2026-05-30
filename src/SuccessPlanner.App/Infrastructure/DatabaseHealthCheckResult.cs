namespace SuccessPlanner.App.Infrastructure;

public sealed record DatabaseHealthCheckResult(
    bool IsHealthy,
    string Summary,
    string DatabasePath,
    string QuickCheckResult,
    int AppliedMigrationCount,
    int LatestAppliedMigration,
    int RequiredMigrationCount,
    int LatestRequiredMigration,
    IReadOnlyList<string> Findings)
{
    public string ToFailureMessage()
    {
        if (IsHealthy)
        {
            return Summary;
        }

        string detail = Findings.Count == 0
            ? "No specific database failure detail was recorded."
            : string.Join(" ", Findings);

        return $"{Summary} {detail}";
    }
}
