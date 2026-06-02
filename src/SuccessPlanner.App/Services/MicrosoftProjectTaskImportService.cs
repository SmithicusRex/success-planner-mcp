using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectTaskImportService
{
    private readonly SettingsService _settingsService;
    private readonly TaskRepository _taskRepository;
    private readonly SourceLinkRepository? _sourceLinkRepository;
    private readonly IMicrosoftProjectAutomationAdapter _automationAdapter;
    private readonly MicrosoftProjectTaskMapper _taskMapper;
    private readonly Func<DateTimeOffset> _nowProvider;

    public MicrosoftProjectTaskImportService(
        SettingsService settingsService,
        TaskRepository taskRepository,
        IMicrosoftProjectAutomationAdapter? automationAdapter = null)
        : this(settingsService, taskRepository, null, automationAdapter)
    {
    }

    public MicrosoftProjectTaskImportService(
        SettingsService settingsService,
        TaskRepository taskRepository,
        SourceLinkRepository? sourceLinkRepository,
        IMicrosoftProjectAutomationAdapter? automationAdapter = null,
        MicrosoftProjectTaskMapper? taskMapper = null,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _sourceLinkRepository = sourceLinkRepository;
        _automationAdapter = automationAdapter ?? new MicrosoftProjectComAutomationAdapter();
        _taskMapper = taskMapper ?? new MicrosoftProjectTaskMapper();
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public async Task<MicrosoftProjectImportResult> ImportSelectedProjectFileAsync(
        CancellationToken cancellationToken = default)
    {
        AppSettings settings = await _settingsService.LoadOrCreateAsync(cancellationToken);
        if (!settings.Connections.EnableProjectDesktop)
        {
            return MicrosoftProjectImportResult.Failed(
                settings.ProjectDesktop.LocalProjectFilePath,
                "Project import off",
                "Turn on Project Desktop in Settings before importing Project tasks.");
        }

        string projectFilePath = settings.ProjectDesktop.LocalProjectFilePath;
        if (string.IsNullOrWhiteSpace(projectFilePath))
        {
            return MicrosoftProjectImportResult.Failed(
                string.Empty,
                "No Project file selected",
                "Choose a local Microsoft Project file in Settings, then import again.");
        }

        if (!File.Exists(projectFilePath))
        {
            return MicrosoftProjectImportResult.Failed(
                projectFilePath,
                "Project file not found",
                "Select an existing Microsoft Project file in Settings, then import again.");
        }

        try
        {
            IReadOnlyList<MicrosoftProjectImportedTask> importedTasks =
                await _automationAdapter.ImportTasksAsync(projectFilePath, cancellationToken);
            List<Guid> localTaskIds = [];
            List<Guid> sourceLinkIds = [];
            DateTimeOffset mappedAt = _nowProvider();

            foreach (MicrosoftProjectImportedTask importedTask in importedTasks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MicrosoftProjectMappedTask mappedTask = _taskMapper.Map(
                    projectFilePath,
                    importedTask,
                    mappedAt);
                await _taskRepository.AddAsync(mappedTask.LocalTask, cancellationToken);
                localTaskIds.Add(mappedTask.LocalTask.Id);

                if (mappedTask.SourceLink is not null && _sourceLinkRepository is not null)
                {
                    await _sourceLinkRepository.SaveAsync(mappedTask.SourceLink, cancellationToken);
                    sourceLinkIds.Add(mappedTask.SourceLink.Id);
                }
            }

            return MicrosoftProjectImportResult.Success(
                projectFilePath,
                importedTasks,
                localTaskIds,
                sourceLinkIds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return MicrosoftProjectImportResult.Failed(
                projectFilePath,
                "Project import failed",
                BuildFailureMessage(ex));
        }
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
    }
}
