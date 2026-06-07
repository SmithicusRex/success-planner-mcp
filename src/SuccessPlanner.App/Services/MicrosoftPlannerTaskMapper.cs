using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class MicrosoftPlannerTaskMapper
{
    public MicrosoftPlannerMappedTask Map(
        MicrosoftPlannerTaskItem plannerTask,
        DateTimeOffset mappedAt)
    {
        ArgumentNullException.ThrowIfNull(plannerTask);

        TaskItem localTask = CreateLocalTask(plannerTask, mappedAt);
        SourceLink sourceLink = CreateSourceLink(plannerTask, localTask.Id, mappedAt);
        return new MicrosoftPlannerMappedTask(localTask, sourceLink);
    }

    private static TaskItem CreateLocalTask(
        MicrosoftPlannerTaskItem plannerTask,
        DateTimeOffset mappedAt)
    {
        TaskItem localTask = TaskItem.Capture(plannerTask.Title);
        DateOnly? startDate = ToDateOnly(plannerTask.StartAt);
        DateOnly? dueDate = ToDateOnly(plannerTask.DueAt);

        if (startDate.HasValue || dueDate.HasValue)
        {
            localTask.Schedule(dueDate, startDate);
        }

        localTask.SetPriority(MapPriority(plannerTask.Priority));

        if (plannerTask.PercentComplete is > 0 and < 100)
        {
            localTask.Start();
        }

        if (plannerTask.IsComplete)
        {
            localTask.Complete(plannerTask.CompletedAt ?? mappedAt);
        }

        localTask.AddTag("Microsoft Planner");
        localTask.AddTag("Planner Import");
        localTask.AddTag("Read Only");
        localTask.UpdateNotes(BuildTaskNotes(plannerTask));
        return localTask;
    }

    private static SourceLink CreateSourceLink(
        MicrosoftPlannerTaskItem plannerTask,
        Guid localTaskId,
        DateTimeOffset mappedAt)
    {
        string sourceVersion = BuildSourceVersion(plannerTask);
        SourceLink sourceLink = SourceLink.Create(
            SourceLinkItemType.Task,
            localTaskId,
            SourceSystem.MicrosoftPlanner,
            plannerTask.Id);
        sourceLink.UpdateExternalReference(
            plannerTask.Id,
            plannerTask.PlanId,
            plannerTask.Title,
            plannerTask.WebLink,
            sourceVersion);
        sourceLink.MarkReadOnly();
        sourceLink.MarkSynced(sourceVersion, mappedAt);
        return sourceLink;
    }

    private static TaskPriority MapPriority(int? plannerPriority)
    {
        return plannerPriority switch
        {
            <= 3 => TaskPriority.High,
            >= 8 => TaskPriority.Low,
            _ => TaskPriority.Normal
        };
    }

    private static string BuildTaskNotes(MicrosoftPlannerTaskItem plannerTask)
    {
        List<string> lines =
        [
            "Imported read-only from Microsoft Planner.",
            $"Planner Task Id: {plannerTask.Id}"
        ];

        if (!string.IsNullOrWhiteSpace(plannerTask.PlanId))
        {
            lines.Add($"Planner plan id: {plannerTask.PlanId}");
        }

        if (!string.IsNullOrWhiteSpace(plannerTask.BucketId))
        {
            lines.Add($"Planner bucket id: {plannerTask.BucketId}");
        }

        if (plannerTask.PercentComplete.HasValue)
        {
            lines.Add($"Planner percent complete: {plannerTask.PercentComplete.Value}%");
        }

        if (plannerTask.Priority.HasValue)
        {
            lines.Add($"Planner priority: {plannerTask.Priority.Value}");
        }

        if (plannerTask.StartAt.HasValue)
        {
            lines.Add($"Planner start: {FormatDate(plannerTask.StartAt.Value)}");
        }

        if (plannerTask.DueAt.HasValue)
        {
            lines.Add($"Planner due: {FormatDate(plannerTask.DueAt.Value)}");
        }

        if (plannerTask.CompletedAt.HasValue)
        {
            lines.Add($"Planner completed: {FormatDate(plannerTask.CompletedAt.Value)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string BuildSourceVersion(MicrosoftPlannerTaskItem plannerTask)
    {
        return string.Join(
            "|",
            $"percent={plannerTask.PercentComplete?.ToString() ?? string.Empty}",
            $"priority={plannerTask.Priority?.ToString() ?? string.Empty}",
            $"start={FormatVersionDate(plannerTask.StartAt)}",
            $"due={FormatVersionDate(plannerTask.DueAt)}",
            $"completed={FormatVersionDate(plannerTask.CompletedAt)}");
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
}
