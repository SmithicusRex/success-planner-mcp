using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectMappedTask
{
    public MicrosoftProjectMappedTask(TaskItem localTask, SourceLink? sourceLink)
    {
        LocalTask = localTask ?? throw new ArgumentNullException(nameof(localTask));
        SourceLink = sourceLink;
    }

    public TaskItem LocalTask { get; }

    public SourceLink? SourceLink { get; }
}
