using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftToDoTaskPushService
{
    private readonly MicrosoftToDoGraphTaskAdapter _taskAdapter;
    private readonly SourceLinkRepository _sourceLinkRepository;

    public MicrosoftToDoTaskPushService(
        MicrosoftToDoGraphTaskAdapter taskAdapter,
        SourceLinkRepository sourceLinkRepository)
    {
        _taskAdapter = taskAdapter ?? throw new ArgumentNullException(nameof(taskAdapter));
        _sourceLinkRepository = sourceLinkRepository
            ?? throw new ArgumentNullException(nameof(sourceLinkRepository));
    }

    public async Task<MicrosoftToDoPushResult> PushCapturedTaskAsync(
        TaskItem localTask,
        string listId,
        CancellationToken cancellationToken = default)
    {
        MicrosoftToDoPushResult result = await _taskAdapter.PushCapturedTaskAsync(
            new MicrosoftToDoPushRequest(localTask, listId),
            cancellationToken);

        if (result.SourceLink is not null)
        {
            await _sourceLinkRepository.SaveAsync(result.SourceLink, cancellationToken);
        }

        return result;
    }
}
