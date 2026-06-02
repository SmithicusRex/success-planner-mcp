using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectTaskMapper
{
    public MicrosoftProjectMappedTask Map(
        string projectFilePath,
        MicrosoftProjectImportedTask importedTask,
        DateTimeOffset mappedAt)
    {
        if (string.IsNullOrWhiteSpace(projectFilePath))
        {
            throw new ArgumentException("Project file path cannot be blank.", nameof(projectFilePath));
        }

        ArgumentNullException.ThrowIfNull(importedTask);

        TaskItem localTask = CreateLocalTask(projectFilePath, importedTask);
        SourceLink? sourceLink = CreateSourceLink(projectFilePath, importedTask, localTask.Id, mappedAt);
        return new MicrosoftProjectMappedTask(localTask, sourceLink);
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

        localTask.SetPriority(MapPriority(importedTask));
        ApplyEstimate(localTask, importedTask);

        if (importedTask.PercentComplete is > 0 and < 100)
        {
            localTask.Start();
        }

        if (importedTask.IsComplete)
        {
            localTask.Complete(importedTask.FinishAt);
        }

        localTask.AddTag("Microsoft Project");
        localTask.AddTag("Project Import");

        if (importedTask.IsCritical)
        {
            localTask.AddTag("Critical Path");
        }

        if (importedTask.IsSummary)
        {
            localTask.AddTag("Project Summary");
        }

        if (importedTask.IsMilestone)
        {
            localTask.AddTag("Milestone");
        }

        if (ShouldMarkTinyStep(importedTask))
        {
            localTask.MarkTinyStep();
        }

        localTask.UpdateNotes(BuildTaskNotes(projectFilePath, importedTask));
        return localTask;
    }

    private static SourceLink? CreateSourceLink(
        string projectFilePath,
        MicrosoftProjectImportedTask importedTask,
        Guid localTaskId,
        DateTimeOffset mappedAt)
    {
        if (string.IsNullOrWhiteSpace(importedTask.ExternalId))
        {
            return null;
        }

        string sourceVersion = BuildSourceVersion(importedTask);
        SourceLink sourceLink = SourceLink.Create(
            SourceLinkItemType.Task,
            localTaskId,
            SourceSystem.MicrosoftProjectDesktop,
            importedTask.ExternalId);
        sourceLink.UpdateExternalReference(
            importedTask.ExternalId,
            projectFilePath,
            importedTask.Name,
            sourceVersion: sourceVersion);
        sourceLink.MarkReadOnly();
        sourceLink.MarkSynced(sourceVersion, mappedAt);
        return sourceLink;
    }

    private static void ApplyEstimate(TaskItem localTask, MicrosoftProjectImportedTask importedTask)
    {
        if (importedTask.IsSummary || importedTask.IsMilestone)
        {
            return;
        }

        if (importedTask.DurationMinutes is >= 1 and <= 480)
        {
            localTask.SetEstimate(importedTask.DurationMinutes);
        }
    }

    private static TaskPriority MapPriority(MicrosoftProjectImportedTask importedTask)
    {
        if (importedTask.IsCritical)
        {
            return TaskPriority.Critical;
        }

        return importedTask.ProjectPriority switch
        {
            >= 800 => TaskPriority.Critical,
            >= 600 => TaskPriority.High,
            <= 300 => TaskPriority.Low,
            _ => TaskPriority.Normal
        };
    }

    private static bool ShouldMarkTinyStep(MicrosoftProjectImportedTask importedTask)
    {
        return !importedTask.IsSummary
            && !importedTask.IsMilestone
            && importedTask.DurationMinutes is > 0 and <= 20;
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

        if (importedTask.OutlineLevel.HasValue)
        {
            lines.Add($"Project outline level: {importedTask.OutlineLevel.Value}");
        }

        if (importedTask.IsSummary || importedTask.IsMilestone)
        {
            lines.Add($"Project task type: {BuildTaskType(importedTask)}");
        }

        if (importedTask.StartAt.HasValue)
        {
            lines.Add($"Project start: {FormatDate(importedTask.StartAt.Value)}");
        }

        if (importedTask.FinishAt.HasValue)
        {
            lines.Add($"Project finish: {FormatDate(importedTask.FinishAt.Value)}");
        }

        if (importedTask.DurationMinutes.HasValue)
        {
            lines.Add($"Project duration: {FormatDuration(importedTask.DurationMinutes.Value)}");
        }

        if (importedTask.ProjectPriority.HasValue)
        {
            lines.Add($"Project priority: {importedTask.ProjectPriority.Value}");
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

    private static string BuildTaskType(MicrosoftProjectImportedTask importedTask)
    {
        if (importedTask.IsSummary && importedTask.IsMilestone)
        {
            return "Summary, Milestone";
        }

        if (importedTask.IsSummary)
        {
            return "Summary";
        }

        return "Milestone";
    }

    private static string BuildSourceVersion(MicrosoftProjectImportedTask importedTask)
    {
        return string.Join(
            "|",
            $"percent={importedTask.PercentComplete?.ToString() ?? string.Empty}",
            $"start={FormatVersionDate(importedTask.StartAt)}",
            $"finish={FormatVersionDate(importedTask.FinishAt)}",
            $"duration={importedTask.DurationMinutes?.ToString() ?? string.Empty}",
            $"priority={importedTask.ProjectPriority?.ToString() ?? string.Empty}",
            $"summary={importedTask.IsSummary}",
            $"milestone={importedTask.IsMilestone}",
            $"critical={importedTask.IsCritical}");
    }

    private static DateOnly? ToDateOnly(DateTimeOffset? value)
    {
        return value.HasValue ? DateOnly.FromDateTime(value.Value.DateTime) : null;
    }

    private static string FormatDate(DateTimeOffset value)
    {
        return DateOnly.FromDateTime(value.DateTime).ToString("yyyy-MM-dd");
    }

    private static string FormatVersionDate(DateTimeOffset? value)
    {
        return value.HasValue ? value.Value.ToUniversalTime().ToString("O") : string.Empty;
    }

    private static string FormatDuration(int durationMinutes)
    {
        if (durationMinutes == 1)
        {
            return "1 minute";
        }

        return $"{durationMinutes} minutes";
    }
}
