namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoTaskList
{
    public MicrosoftToDoTaskList(string id, string displayName, string wellKnownListName = "")
    {
        Id = NormalizeRequired(id, nameof(id));
        DisplayName = NormalizeOptional(displayName);
        WellKnownListName = NormalizeOptional(wellKnownListName);
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string WellKnownListName { get; }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }

    private static string NormalizeOptional(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
