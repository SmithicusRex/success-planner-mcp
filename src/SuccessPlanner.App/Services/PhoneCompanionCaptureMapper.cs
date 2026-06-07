using SuccessPlanner.App.Domain;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionCaptureMapper
{
    public PhoneCompanionMappedCapture Map(
        PhoneCompanionSyncBatch batch,
        PhoneCompanionQuickCaptureItem capture,
        DateTimeOffset importedAt)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(capture);

        TaskItem localTask = CreateLocalInboxTask(capture);
        string externalId = BuildExternalId(batch.DeviceId, capture.ClientCaptureId);
        string sourceVersion = BuildSourceVersion(batch, capture);
        SourceLink sourceLink = SourceLink.Create(
            SourceLinkItemType.Task,
            localTask.Id,
            SourceSystem.PhoneCompanion,
            externalId);
        sourceLink.UpdateExternalReference(
            externalId,
            batch.DeviceId,
            capture.Title,
            sourceVersion: sourceVersion);
        sourceLink.MarkSynced(sourceVersion, importedAt);

        return new PhoneCompanionMappedCapture(localTask, sourceLink);
    }

    public static string BuildExternalId(string deviceId, string clientCaptureId)
    {
        string normalizedDeviceId = NormalizeRequired(deviceId, nameof(deviceId));
        string normalizedClientCaptureId = NormalizeRequired(clientCaptureId, nameof(clientCaptureId));
        return $"{normalizedDeviceId}:{normalizedClientCaptureId}";
    }

    private static TaskItem CreateLocalInboxTask(PhoneCompanionQuickCaptureItem capture)
    {
        TaskItem localTask = TaskItem.Rehydrate(
            Guid.NewGuid(),
            capture.Title,
            capture.CapturedAt,
            capture.DueDate.HasValue ? TaskItemStatus.Planned : TaskItemStatus.Captured,
            TaskPriority.Normal,
            notes: capture.Notes,
            dueDate: capture.DueDate,
            tags: BuildTags(capture));

        if (capture.Destination == PhoneCompanionCaptureDestination.LetSuccessPlannerChoose)
        {
            localTask.AddTag("MCP Chosen");
        }

        return localTask;
    }

    private static IEnumerable<string> BuildTags(PhoneCompanionQuickCaptureItem capture)
    {
        yield return "Phone Companion";
        yield return "Phone Capture";

        foreach (string tag in capture.Tags)
        {
            yield return tag;
        }
    }

    private static string BuildSourceVersion(
        PhoneCompanionSyncBatch batch,
        PhoneCompanionQuickCaptureItem capture)
    {
        return string.Join(
            "|",
            $"contract={capture.ContractVersion}",
            $"batch={batch.BatchId}",
            $"device={batch.DeviceId}",
            $"captured={capture.CapturedAt.ToUniversalTime():O}",
            $"due={capture.DueDate?.ToString("yyyy-MM-dd") ?? string.Empty}",
            $"destination={capture.Destination}");
    }

    private static string NormalizeRequired(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be blank.", parameterName);
        }

        return value.Trim();
    }
}
