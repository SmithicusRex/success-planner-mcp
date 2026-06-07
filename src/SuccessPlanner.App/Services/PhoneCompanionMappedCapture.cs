using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionMappedCapture
{
    public PhoneCompanionMappedCapture(TaskItem localTask, SourceLink sourceLink)
    {
        LocalTask = localTask ?? throw new ArgumentNullException(nameof(localTask));
        SourceLink = sourceLink ?? throw new ArgumentNullException(nameof(sourceLink));
    }

    public TaskItem LocalTask { get; }

    public SourceLink SourceLink { get; }
}
