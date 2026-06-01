using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoPushRequest
{
    public MicrosoftToDoPushRequest(TaskItem localTask, string listId)
    {
        LocalTask = localTask ?? throw new ArgumentNullException(nameof(localTask));
        ListId = NormalizeRequired(listId, nameof(listId));
    }

    public TaskItem LocalTask { get; }

    public string ListId { get; }

    public string Title => LocalTask.Title;

    public string BodyContent => LocalTask.Notes;

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
