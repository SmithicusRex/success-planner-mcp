using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerTaskImportService
{
    private readonly SettingsService _settingsService;
    private readonly TaskRepository _taskRepository;
    private readonly SourceLinkRepository _sourceLinkRepository;
    private readonly IMicrosoftPlannerTaskAdapter _taskAdapter;
    private readonly MicrosoftPlannerTaskMapper _taskMapper;
    private readonly Func<DateTimeOffset> _nowProvider;

    public MicrosoftPlannerTaskImportService(
        SettingsService settingsService,
        TaskRepository taskRepository,
        SourceLinkRepository sourceLinkRepository,
        IMicrosoftPlannerTaskAdapter? taskAdapter = null,
        MicrosoftPlannerTaskMapper? taskMapper = null,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _sourceLinkRepository = sourceLinkRepository ?? throw new ArgumentNullException(nameof(sourceLinkRepository));
        _taskAdapter = taskAdapter ?? new MicrosoftPlannerGraphTaskAdapter();
        _taskMapper = taskMapper ?? new MicrosoftPlannerTaskMapper();
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public async Task<MicrosoftPlannerImportResult> ImportAssignedTasksAsync(
        CancellationToken cancellationToken = default)
    {
        AppSettings settings = await _settingsService.LoadOrCreateAsync(cancellationToken);
        if (!settings.Connections.EnablePlanner)
        {
            MicrosoftPlannerConnectionStatus disabled = MicrosoftPlannerConnectionStatus.Disabled();
            return MicrosoftPlannerImportResult.Failed(
                disabled,
                "Planner import off",
                "Turn on Planner in Settings before importing Planner tasks.");
        }

        try
        {
            MicrosoftPlannerPullResult pullResult =
                await _taskAdapter.PullAssignedTasksAsync(cancellationToken);
            if (!pullResult.CanUseData)
            {
                return MicrosoftPlannerImportResult.Failed(
                    pullResult.ConnectionStatus,
                    pullResult.ConnectionStatus.StatusText,
                    pullResult.ConnectionStatus.DetailText);
            }

            List<Guid> localTaskIds = [];
            List<Guid> sourceLinkIds = [];
            int skippedExistingCount = 0;
            DateTimeOffset importedAt = _nowProvider();

            foreach (MicrosoftPlannerTaskItem plannerTask in pullResult.Tasks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SourceLink? existingLink = await _sourceLinkRepository.GetByExternalReferenceAsync(
                    SourceSystem.MicrosoftPlanner,
                    plannerTask.Id,
                    cancellationToken);
                if (existingLink is not null)
                {
                    skippedExistingCount++;
                    continue;
                }

                MicrosoftPlannerMappedTask mappedTask = _taskMapper.Map(plannerTask, importedAt);
                await _taskRepository.AddAsync(mappedTask.LocalTask, cancellationToken);
                await _sourceLinkRepository.SaveAsync(mappedTask.SourceLink, cancellationToken);
                localTaskIds.Add(mappedTask.LocalTask.Id);
                sourceLinkIds.Add(mappedTask.SourceLink.Id);
            }

            return MicrosoftPlannerImportResult.Success(
                pullResult.ConnectionStatus,
                pullResult.Tasks,
                localTaskIds,
                sourceLinkIds,
                skippedExistingCount);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            MicrosoftPlannerConnectionStatus failed =
                MicrosoftPlannerConnectionStatus.Failed(BuildFailureMessage(ex), _nowProvider());
            return MicrosoftPlannerImportResult.Failed(
                failed,
                "Planner import failed",
                failed.DetailText);
        }
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
    }
}
