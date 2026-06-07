using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;

namespace SuccessPlanner.App.Services;

public sealed class PhoneCompanionCaptureImportService
{
    private readonly SettingsService _settingsService;
    private readonly TaskRepository _taskRepository;
    private readonly SourceLinkRepository _sourceLinkRepository;
    private readonly PhoneCompanionCaptureMapper _captureMapper;
    private readonly Func<DateTimeOffset> _nowProvider;

    public PhoneCompanionCaptureImportService(
        SettingsService settingsService,
        TaskRepository taskRepository,
        SourceLinkRepository sourceLinkRepository,
        PhoneCompanionCaptureMapper? captureMapper = null,
        Func<DateTimeOffset>? nowProvider = null)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _sourceLinkRepository = sourceLinkRepository ?? throw new ArgumentNullException(nameof(sourceLinkRepository));
        _captureMapper = captureMapper ?? new PhoneCompanionCaptureMapper();
        _nowProvider = nowProvider ?? (() => DateTimeOffset.Now);
    }

    public async Task<PhoneCompanionSyncResult> ImportBatchAsync(
        PhoneCompanionSyncBatch batch,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(batch);
        cancellationToken.ThrowIfCancellationRequested();

        AppSettings settings = await _settingsService.LoadOrCreateAsync(cancellationToken);
        if (!settings.Connections.EnablePhoneCompanion)
        {
            return PhoneCompanionSyncResult.Rejected(
                "Turn on Phone Companion in Settings before importing phone captures.");
        }

        if (!batch.HasCaptures)
        {
            return PhoneCompanionSyncResult.Empty();
        }

        List<PhoneCompanionCaptureImportOutcome> outcomes = [];
        DateTimeOffset importedAt = _nowProvider();

        foreach (PhoneCompanionQuickCaptureItem capture in batch.Captures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!CanImportToLocalInbox(capture))
            {
                outcomes.Add(PhoneCompanionCaptureImportOutcome.Rejected(
                    capture.ClientCaptureId,
                    "Only local inbox phone captures can import right now."));
                continue;
            }

            string externalId = PhoneCompanionCaptureMapper.BuildExternalId(
                batch.DeviceId,
                capture.ClientCaptureId);
            SourceLink? existingLink = await _sourceLinkRepository.GetByExternalReferenceAsync(
                SourceSystem.PhoneCompanion,
                externalId,
                cancellationToken);
            if (existingLink is not null)
            {
                outcomes.Add(PhoneCompanionCaptureImportOutcome.Skipped(
                    capture.ClientCaptureId,
                    "Already imported into local inbox."));
                continue;
            }

            PhoneCompanionMappedCapture mappedCapture = _captureMapper.Map(batch, capture, importedAt);
            await _taskRepository.AddAsync(mappedCapture.LocalTask, cancellationToken);
            await _sourceLinkRepository.SaveAsync(mappedCapture.SourceLink, cancellationToken);
            outcomes.Add(PhoneCompanionCaptureImportOutcome.Imported(
                capture.ClientCaptureId,
                mappedCapture.LocalTask.Id));
        }

        return outcomes.Any(outcome => outcome.State == PhoneCompanionCaptureImportState.Rejected)
            ? PhoneCompanionSyncResult.Partial(outcomes)
            : PhoneCompanionSyncResult.Accepted(outcomes);
    }

    private static bool CanImportToLocalInbox(PhoneCompanionQuickCaptureItem capture)
    {
        return capture.Destination is PhoneCompanionCaptureDestination.LetSuccessPlannerChoose
            or PhoneCompanionCaptureDestination.LocalInbox;
    }
}
