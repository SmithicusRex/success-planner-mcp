using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectTaskImportService
{
    private readonly SettingsService _settingsService;
    private readonly TaskRepository _taskRepository;
    private readonly IMicrosoftProjectAutomationAdapter _automationAdapter;

    public MicrosoftProjectTaskImportService(
        SettingsService settingsService,
        TaskRepository taskRepository,
        IMicrosoftProjectAutomationAdapter? automationAdapter = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _automationAdapter = automationAdapter ?? new MicrosoftProjectComAutomationAdapter();
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

            foreach (MicrosoftProjectImportedTask importedTask in importedTasks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                TaskItem localTask = CreateLocalTask(projectFilePath, importedTask);
                await _taskRepository.AddAsync(localTask, cancellationToken);
                localTaskIds.Add(localTask.Id);
            }

            return MicrosoftProjectImportResult.Success(projectFilePath, importedTasks, localTaskIds);
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

    private static TaskItem CreateLocalTask(
        string projectFilePath,
        MicrosoftProjectImportedTask importedTask)
    {
        TaskItem localTask = TaskItem.Capture(importedTask.Name);
        DateOnly? startDate = ToDateOnly(importedTask.StartAt);
        DateOnly? dueDate = ToDateOnly(importedTask.FinishAt);

        if (startDate.HasValue || dueDate.HasValue)
        {
            localTask.Schedule(dueDate, startDate);
        }

        localTask.AddTag("Microsoft Project");
        localTask.AddTag("Project Import");
        localTask.UpdateNotes(BuildTaskNotes(projectFilePath, importedTask));
        return localTask;
    }

    private static string BuildTaskNotes(
        string projectFilePath,
        MicrosoftProjectImportedTask importedTask)
    {
        List<string> lines =
        [
            $"Imported from Microsoft Project: {Path.GetFileName(projectFilePath)}"
        ];

        if (!string.IsNullOrWhiteSpace(importedTask.ExternalId))
        {
            lines.Add($"Project Task Id: {importedTask.ExternalId}");
        }

        if (importedTask.StartAt.HasValue)
        {
            lines.Add($"Project start: {FormatDate(importedTask.StartAt.Value)}");
        }

        if (importedTask.FinishAt.HasValue)
        {
            lines.Add($"Project finish: {FormatDate(importedTask.FinishAt.Value)}");
        }

        if (importedTask.PercentComplete.HasValue)
        {
            lines.Add($"Project percent complete: {importedTask.PercentComplete.Value}%");
        }

        if (!string.IsNullOrWhiteSpace(importedTask.Notes))
        {
            lines.Add(string.Empty);
            lines.Add(importedTask.Notes);
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static DateOnly? ToDateOnly(DateTimeOffset? value)
    {
        return value.HasValue ? DateOnly.FromDateTime(value.Value.DateTime) : null;
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return DateOnly.FromDateTime(value.DateTime).ToString("yyyy-MM-dd");
    }

    private static string BuildFailureMessage(Exception exception)
    {
        return string.IsNullOrWhiteSpace(exception.Message)
            ? exception.GetType().Name
            : exception.Message.Trim();
    }
}
