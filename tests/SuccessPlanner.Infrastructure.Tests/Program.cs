using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Bootstrap;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.Services;
using SuccessPlanner.App.ViewModels;

TestRunner.RunAll(
    ("DatabaseService creates a real SQLite database", DatabaseServiceCreatesSqliteDatabase),
    ("DatabaseService replaces the legacy bootstrap marker", DatabaseServiceReplacesLegacyMarker),
    ("DatabaseService records repeatable migrations", DatabaseServiceRecordsRepeatableMigrations),
    ("DatabaseService opens SQLite file while another handle is active", DatabaseServiceOpensSqliteFileWhileAnotherHandleIsActive),
    ("DatabaseService creates core application tables", DatabaseServiceCreatesCoreApplicationTables),
    ("DatabaseService creates sync queue table", DatabaseServiceCreatesSyncQueueTable),
    ("DatabaseService adds sync queue table to existing stores", DatabaseServiceAddsSyncQueueTableToExistingStores),
    ("SyncQueueRepository saves and loads queue items", SyncQueueRepositorySavesAndLoadsQueueItems),
    ("SyncQueueRepository loads ready queue items", SyncQueueRepositoryLoadsReadyQueueItems),
    ("SyncQueueRepository updates state and deletes queue items", SyncQueueRepositoryUpdatesStateAndDeletesQueueItems),
    ("SyncService queues local changes", SyncServiceQueuesLocalChanges),
    ("SyncService reports ready work and status", SyncServiceReportsReadyWorkAndStatus),
    ("SyncService updates queue item states", SyncServiceUpdatesQueueItemStates),
    ("BackgroundSyncWorker processes ready queue items", BackgroundSyncWorkerProcessesReadyQueueItems),
    ("BackgroundSyncWorker records processor failures", BackgroundSyncWorkerRecordsProcessorFailures),
    ("BackgroundSyncWorker retries failed queue items", BackgroundSyncWorkerRetriesFailedQueueItems),
    ("BackgroundWorkerHost starts and stops workers", BackgroundWorkerHostStartsAndStopsWorkers),
    ("MicrosoftToDoConnectionTestService respects disabled setting", MicrosoftToDoConnectionTestServiceRespectsDisabledSetting),
    ("MicrosoftToDoConnectionTestService uses configured probe", MicrosoftToDoConnectionTestServiceUsesConfiguredProbe),
    ("MicrosoftToDoConnectionTestService maps probe failures", MicrosoftToDoConnectionTestServiceMapsProbeFailures),
    ("MicrosoftToDoGraphConnectionProbe maps token and Graph responses", MicrosoftToDoGraphConnectionProbeMapsTokenAndGraphResponses),
    ("MicrosoftPlannerAvailabilityTestService respects disabled setting", MicrosoftPlannerAvailabilityTestServiceRespectsDisabledSetting),
    ("MicrosoftPlannerAvailabilityTestService uses configured probe", MicrosoftPlannerAvailabilityTestServiceUsesConfiguredProbe),
    ("MicrosoftPlannerAvailabilityTestService maps probe failures", MicrosoftPlannerAvailabilityTestServiceMapsProbeFailures),
    ("MicrosoftPlannerGraphAvailabilityProbe maps token and Graph responses", MicrosoftPlannerGraphAvailabilityProbeMapsTokenAndGraphResponses),
    ("MicrosoftPlannerGraphTaskAdapter needs sign-in without token", MicrosoftPlannerGraphTaskAdapterNeedsSignInWithoutToken),
    ("MicrosoftPlannerGraphTaskAdapter pulls assigned tasks", MicrosoftPlannerGraphTaskAdapterPullsAssignedTasks),
    ("MicrosoftPlannerGraphTaskAdapter maps pull failures", MicrosoftPlannerGraphTaskAdapterMapsPullFailures),
    ("MicrosoftPlannerTaskImportService imports assigned tasks", MicrosoftPlannerTaskImportServiceImportsAssignedTasks),
    ("MicrosoftPlannerTaskImportService skips existing tasks", MicrosoftPlannerTaskImportServiceSkipsExistingTasks),
    ("MicrosoftPlannerTaskImportService reports disabled Planner import", MicrosoftPlannerTaskImportServiceReportsDisabledPlannerImport),
    ("PhoneCompanionCaptureImportService imports captures into local inbox", PhoneCompanionCaptureImportServiceImportsCapturesIntoLocalInbox),
    ("PhoneCompanionCaptureImportService skips existing captures", PhoneCompanionCaptureImportServiceSkipsExistingCaptures),
    ("PhoneCompanionCaptureImportService reports disabled import", PhoneCompanionCaptureImportServiceReportsDisabledImport),
    ("PhoneCompanionCaptureImportService rejects unsupported destinations", PhoneCompanionCaptureImportServiceRejectsUnsupportedDestinations),
    ("MicrosoftToDoGraphTaskAdapter needs sign-in without token", MicrosoftToDoGraphTaskAdapterNeedsSignInWithoutToken),
    ("MicrosoftToDoGraphTaskAdapter pulls lists and tasks", MicrosoftToDoGraphTaskAdapterPullsListsAndTasks),
    ("MicrosoftToDoGraphTaskAdapter maps pull failures", MicrosoftToDoGraphTaskAdapterMapsPullFailures),
    ("MicrosoftToDoGraphTaskAdapter pushes captured tasks", MicrosoftToDoGraphTaskAdapterPushesCapturedTasks),
    ("MicrosoftToDoGraphTaskAdapter does not push without token", MicrosoftToDoGraphTaskAdapterDoesNotPushWithoutToken),
    ("MicrosoftToDoGraphTaskAdapter maps push failures", MicrosoftToDoGraphTaskAdapterMapsPushFailures),
    ("MicrosoftToDoTaskPushService saves source links", MicrosoftToDoTaskPushServiceSavesSourceLinks),
    ("MicrosoftProjectDesktopDetector finds Click-to-Run Project", MicrosoftProjectDesktopDetectorFindsClickToRunProject),
    ("MicrosoftProjectDesktopDetector finds Project on PATH", MicrosoftProjectDesktopDetectorFindsProjectOnPath),
    ("MicrosoftProjectDesktopDetector reports Project not found", MicrosoftProjectDesktopDetectorReportsProjectNotFound),
    ("MicrosoftProjectTaskImportService imports selected Project tasks", MicrosoftProjectTaskImportServiceImportsSelectedProjectTasks),
    ("MicrosoftProjectTaskImportService reports disabled Project import", MicrosoftProjectTaskImportServiceReportsDisabledProjectImport),
    ("MicrosoftProjectTaskImportService reports missing Project file", MicrosoftProjectTaskImportServiceReportsMissingProjectFile),
    ("DatabaseService reports a healthy database", DatabaseServiceReportsHealthyDatabase),
    ("DatabaseService reports missing migration health failures", DatabaseServiceReportsMissingMigrationHealthFailures),
    ("DatabaseStartupMigrationService migrates a new database at startup", DatabaseStartupMigrationServiceMigratesNewDatabaseAtStartup),
    ("DatabaseStartupMigrationService preserves data on restart", DatabaseStartupMigrationServicePreservesDataOnRestart),
    ("AppBootstrapper shows a simple database failure message", AppBootstrapperShowsSimpleDatabaseFailureMessage),
    ("TaskRepository saves and loads task state", TaskRepositorySavesAndLoadsTaskState),
    ("TaskRepository loads unplanned tasks", TaskRepositoryLoadsUnplannedTasks),
    ("TaskRepository loads today tasks", TaskRepositoryLoadsTodayTasks),
    ("TaskRepository loads recent active tasks", TaskRepositoryLoadsRecentActiveTasks),
    ("TodayViewModel saves task actions through TaskRepository", TodayViewModelSavesTaskActionsThroughTaskRepository),
    ("DoneViewModel completes selected task through TaskRepository", DoneViewModelCompletesSelectedTaskThroughTaskRepository),
    ("TaskRepository deletes tasks", TaskRepositoryDeletesTasks),
    ("CaptureViewModel saves captured tasks through TaskRepository", CaptureViewModelSavesCapturedTasksThroughTaskRepository),
    ("PlanViewModel saves planning changes through TaskRepository", PlanViewModelSavesPlanningChangesThroughTaskRepository),
    ("ReviewViewModel loads small wins through NoteRepository", ReviewViewModelLoadsSmallWinsThroughNoteRepository),
    ("ReviewViewModel loads stuck items through TaskRepository", ReviewViewModelLoadsStuckItemsThroughTaskRepository),
    ("ReviewViewModel loads needs-decision items through TaskRepository", ReviewViewModelLoadsNeedsDecisionItemsThroughTaskRepository),
    ("ReviewViewModel saves next focus through SettingsMetadataRepository", ReviewViewModelSavesNextFocusThroughSettingsMetadataRepository),
    ("ReviewViewModel loads focus and movement successes through repositories", ReviewViewModelLoadsFocusAndMovementSuccessesThroughRepositories),
    ("FocusSessionRepository saves and loads focus session state", FocusSessionRepositorySavesAndLoadsFocusSessionState),
    ("StartWorkViewModel records focus sessions through repositories", StartWorkViewModelRecordsFocusSessionsThroughRepositories),
    ("MovementSessionRepository saves and loads movement state", MovementSessionRepositorySavesAndLoadsMovementSessionState),
    ("MoveViewModel saves movement sessions through repository", MoveViewModelSavesMovementSessionsThroughRepository),
    ("MoveViewModel movement save appears in Review", MoveViewModelMovementSaveAppearsInReview),
    ("SearchService finds tasks notes projects and source links", SearchServiceFindsTasksNotesProjectsAndSourceLinks),
    ("FindViewModel searches local data through SearchService", FindViewModelSearchesLocalDataThroughSearchService),
    ("SettingsService saves Project file selection", SettingsServiceSavesProjectFileSelection),
    ("SettingsMetadataRepository upserts and deletes metadata", SettingsMetadataRepositoryUpsertsAndDeletesMetadata));

static async Task DatabaseServiceCreatesSqliteDatabase()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.HealthCheckAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.True(File.Exists(paths.DatabasePath), "Database file should be created.");
    Assert.True(IsSqliteDatabase(paths.DatabasePath), "Database file should have the SQLite header.");
    Assert.Equal("ok", await ReadScalarAsync(paths.DatabasePath, "PRAGMA quick_check;"));
    Assert.Equal(
        "Success Planner MCP SQLite local store",
        await ReadScalarAsync(paths.DatabasePath, "SELECT value FROM local_store_metadata WHERE key = 'store_kind';"));
}

static async Task DatabaseServiceReplacesLegacyMarker()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    Directory.CreateDirectory(paths.AppDataDirectory);
    await File.WriteAllTextAsync(paths.DatabasePath, "Success Planner MCP local data store placeholder.");

    DatabaseService database = new(paths);
    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.HealthCheckAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.True(IsSqliteDatabase(paths.DatabasePath), "Legacy marker should be replaced by SQLite.");
    Assert.Equal(1, Directory.GetFiles(paths.AppDataDirectory, "*.placeholder-*").Length);
}

static async Task DatabaseServiceRecordsRepeatableMigrations()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    DatabaseMigrationResult firstRun = await database.MigrateAsync(CancellationToken.None);
    DatabaseMigrationResult secondRun = await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.Equal(3, firstRun.AppliedCountThisRun);
    Assert.Equal(0, secondRun.AppliedCountThisRun);
    Assert.Equal(3, secondRun.TotalAppliedCount);
    Assert.Equal(3L, await ReadScalarAsync(paths.DatabasePath, "SELECT COUNT(*) FROM schema_migrations;"));
    Assert.Equal(1L, await ReadScalarAsync(paths.DatabasePath, "SELECT version FROM schema_migrations;"));
    Assert.Equal(
        "Create local store metadata",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 1;"));
    Assert.Equal(
        "Create core application tables",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 2;"));
    Assert.Equal(
        "Create sync queue table",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 3;"));
    Assert.Equal(
        "Success Planner MCP SQLite local store",
        await ReadScalarAsync(paths.DatabasePath, "SELECT value FROM local_store_metadata WHERE key = 'store_kind';"));
}

static async Task DatabaseServiceOpensSqliteFileWhileAnotherHandleIsActive()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    using FileStream activeHandle = new(
        paths.DatabasePath,
        FileMode.Open,
        FileAccess.ReadWrite,
        FileShare.ReadWrite);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    await database.HealthCheckAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.True(activeHandle.CanRead, "The active database handle should remain open during startup.");
}

static async Task DatabaseServiceCreatesCoreApplicationTables()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    string[] expectedTables =
    [
        "tasks",
        "projects",
        "milestones",
        "notes",
        "focus_sessions",
        "movement_sessions",
        "source_links",
        "settings_metadata",
        "sync_queue"
    ];

    foreach (string tableName in expectedTables)
    {
        Assert.True(await TableExistsAsync(paths.DatabasePath, tableName), $"Expected table '{tableName}' to exist.");
    }

    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "tasks", "title"), "Tasks should store task titles.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "projects", "minimum_win"), "Projects should store minimum wins.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "notes", "owner_type"), "Notes should store owner type.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "focus_sessions", "planned_minutes"), "Focus sessions should store planned minutes.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "movement_sessions", "mind_occupier"), "Movement sessions should store mind occupiers.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "source_links", "sync_state"), "Source links should store sync state.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "payload_json"), "Sync queue should store payload JSON.");
}

static async Task DatabaseServiceCreatesSyncQueueTable()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.True(await TableExistsAsync(paths.DatabasePath, "sync_queue"), "Sync queue table should exist.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "id"), "Sync queue should store queue ids.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "local_item_type"), "Sync queue should store local item type.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "local_item_id"), "Sync queue should store local item id.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "source_system"), "Sync queue should store source system.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "source_link_id"), "Sync queue should optionally point to source links.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "action_type"), "Sync queue should store the requested sync action.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "payload_json"), "Sync queue should store payload JSON.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "sync_state"), "Sync queue should store sync state.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "retry_count"), "Sync queue should store retry count.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "next_attempt_at"), "Sync queue should store next attempt time.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "last_attempted_at"), "Sync queue should store last attempt time.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "failure_message"), "Sync queue should store failure messages.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "created_at"), "Sync queue should store creation time.");
    Assert.True(await ColumnExistsAsync(paths.DatabasePath, "sync_queue", "updated_at"), "Sync queue should store update time.");
    Assert.True(
        await IndexExistsAsync(paths.DatabasePath, "idx_sync_queue_state_next_attempt"),
        "Sync queue should be indexed by state and next attempt.");
    Assert.True(
        await IndexExistsAsync(paths.DatabasePath, "idx_sync_queue_local_item"),
        "Sync queue should be indexed by local item.");
    Assert.True(
        await IndexExistsAsync(paths.DatabasePath, "idx_sync_queue_source_link"),
        "Sync queue should be indexed by source link.");
}

static async Task DatabaseServiceAddsSyncQueueTableToExistingStores()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);

    await using (SqliteConnection connection = new($"Data Source={paths.DatabasePath};Pooling=False"))
    {
        await connection.OpenAsync();
        DatabaseMigrator olderMigrator = new(DatabaseMigrations.All.Where(migration => migration.Version <= 2).ToArray());
        await olderMigrator.MigrateAsync(connection, CancellationToken.None);
    }

    Assert.False(await TableExistsAsync(paths.DatabasePath, "sync_queue"), "Older stores should start without sync queue.");

    DatabaseService database = new(paths);
    await database.OpenAsync(CancellationToken.None);
    DatabaseMigrationResult result = await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.Equal(1, result.AppliedCountThisRun);
    Assert.Equal(3, result.LatestAppliedVersion);
    Assert.True(await TableExistsAsync(paths.DatabasePath, "sync_queue"), "Migration 3 should add sync queue.");
    Assert.Equal(
        "Create sync queue table",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 3;"));
}

static async Task SyncQueueRepositorySavesAndLoadsQueueItems()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    Guid taskId = Guid.NewGuid();
    Guid sourceLinkId = Guid.NewGuid();
    await InsertSourceLinkRowAsync(
        paths,
        sourceLinkId,
        SourceLinkItemType.Task,
        taskId,
        SourceSystem.MicrosoftToDo,
        "todo-task-321",
        "Call the pharmacy");

    SyncQueueRepository repository = new(paths);
    SyncQueueItem item = SyncQueueItem.Create(
        SourceLinkItemType.Task,
        taskId,
        SourceSystem.MicrosoftToDo,
        SyncQueueActionType.Update,
        """{"title":"Call the pharmacy"}""",
        sourceLinkId);

    await repository.EnqueueAsync(item, CancellationToken.None);

    SyncQueueItem? loaded = await repository.GetByIdAsync(item.Id, CancellationToken.None);
    Assert.NotNull(loaded, "Queued item should load by id.");
    Assert.Equal(item.Id, loaded!.Id);
    Assert.Equal(SourceLinkItemType.Task, loaded.LocalItemType);
    Assert.Equal(taskId, loaded.LocalItemId);
    Assert.Equal(SourceSystem.MicrosoftToDo, loaded.SourceSystem);
    Assert.Equal(sourceLinkId, loaded.SourceLinkId);
    Assert.Equal(SyncQueueActionType.Update, loaded.ActionType);
    Assert.Equal("""{"title":"Call the pharmacy"}""", loaded.PayloadJson);
    Assert.Equal(SyncState.Pending, loaded.SyncState);
    Assert.Equal(0, loaded.RetryCount);

    SyncQueueRepository restartedRepository = new(paths);
    IReadOnlyList<SyncQueueItem> all = await restartedRepository.GetAllAsync(CancellationToken.None);
    Assert.Equal(1, all.Count);
    Assert.Equal(item.Id, all[0].Id);
}

static async Task SyncQueueRepositoryLoadsReadyQueueItems()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 10, 0, 0, TimeSpan.Zero);
    SyncQueueItem readyPending = SyncQueueItem.Rehydrate(
        Guid.NewGuid(),
        SourceLinkItemType.Task,
        Guid.NewGuid(),
        SourceSystem.MicrosoftToDo,
        SyncQueueActionType.Update,
        "{}",
        SyncState.Pending,
        retryCount: 0,
        createdAt: now.AddMinutes(-20),
        updatedAt: now.AddMinutes(-20));
    SyncQueueItem readyFailed = SyncQueueItem.Rehydrate(
        Guid.NewGuid(),
        SourceLinkItemType.Project,
        Guid.NewGuid(),
        SourceSystem.MicrosoftPlanner,
        SyncQueueActionType.Update,
        "{}",
        SyncState.Failed,
        retryCount: 1,
        createdAt: now.AddMinutes(-15),
        updatedAt: now.AddMinutes(-10),
        nextAttemptAt: now.AddMinutes(-1),
        lastAttemptedAt: now.AddMinutes(-10),
        failureMessage: "Temporary adapter failure.");
    SyncQueueItem futureFailed = SyncQueueItem.Rehydrate(
        Guid.NewGuid(),
        SourceLinkItemType.Note,
        Guid.NewGuid(),
        SourceSystem.LocalImport,
        SyncQueueActionType.Create,
        "{}",
        SyncState.Failed,
        retryCount: 1,
        createdAt: now.AddMinutes(-14),
        updatedAt: now.AddMinutes(-9),
        nextAttemptAt: now.AddMinutes(30),
        lastAttemptedAt: now.AddMinutes(-9),
        failureMessage: "Wait before retry.");
    SyncQueueItem syncing = SyncQueueItem.Rehydrate(
        Guid.NewGuid(),
        SourceLinkItemType.Task,
        Guid.NewGuid(),
        SourceSystem.MicrosoftToDo,
        SyncQueueActionType.Delete,
        "{}",
        SyncState.Syncing,
        retryCount: 0,
        createdAt: now.AddMinutes(-13),
        updatedAt: now.AddMinutes(-8),
        lastAttemptedAt: now.AddMinutes(-8));

    SyncQueueRepository repository = new(paths);
    await repository.SaveAsync(futureFailed, CancellationToken.None);
    await repository.SaveAsync(readyFailed, CancellationToken.None);
    await repository.SaveAsync(syncing, CancellationToken.None);
    await repository.SaveAsync(readyPending, CancellationToken.None);

    IReadOnlyList<SyncQueueItem> readyItems = await repository.GetReadyAsync(now, limit: 10, CancellationToken.None);

    Assert.Equal(2, readyItems.Count);
    Assert.Equal(readyPending.Id, readyItems[0].Id);
    Assert.Equal(readyFailed.Id, readyItems[1].Id);
}

static async Task SyncQueueRepositoryUpdatesStateAndDeletesQueueItems()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 11, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    SyncQueueItem item = SyncQueueItem.Create(
        SourceLinkItemType.Milestone,
        Guid.NewGuid(),
        SourceSystem.MicrosoftProjectDesktop,
        SyncQueueActionType.Update,
        """{"status":"Completed"}""",
        createdAt: now);

    await repository.EnqueueAsync(item, CancellationToken.None);
    item.MarkSyncing(now.AddMinutes(1));
    await repository.SaveAsync(item, CancellationToken.None);

    SyncQueueItem? syncing = await repository.GetByIdAsync(item.Id, CancellationToken.None);
    Assert.NotNull(syncing, "Saved queue item should still exist after state update.");
    Assert.Equal(SyncState.Syncing, syncing!.SyncState);
    Assert.Equal(now.AddMinutes(1).ToUniversalTime(), syncing.LastAttemptedAt);

    item.MarkFailed("Project desktop is closed.", now.AddMinutes(15), now.AddMinutes(2));
    await repository.SaveAsync(item, CancellationToken.None);

    SyncQueueItem? failed = await repository.GetByIdAsync(item.Id, CancellationToken.None);
    Assert.NotNull(failed, "Failed queue item should load.");
    Assert.Equal(SyncState.Failed, failed!.SyncState);
    Assert.Equal(1, failed.RetryCount);
    Assert.Equal("Project desktop is closed.", failed.FailureMessage);

    IReadOnlyDictionary<SyncState, int> counts = await repository.CountByStateAsync(CancellationToken.None);
    Assert.True(counts.TryGetValue(SyncState.Failed, out int failedCount), "Failed count should be available.");
    Assert.Equal(1, failedCount);

    await repository.DeleteAsync(item.Id, CancellationToken.None);
    SyncQueueItem? deleted = await repository.GetByIdAsync(item.Id, CancellationToken.None);
    Assert.Null(deleted, "Deleted queue item should not load by id.");
}

static async Task SyncServiceQueuesLocalChanges()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 12, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    SyncService service = new(repository, () => now);
    Guid taskId = Guid.NewGuid();

    SyncQueueItem queued = await service.QueueUpdateAsync(
        SourceLinkItemType.Task,
        taskId,
        SourceSystem.MicrosoftToDo,
        """{"title":"Call the pharmacy"}""",
        cancellationToken: CancellationToken.None);

    SyncQueueItem? loaded = await repository.GetByIdAsync(queued.Id, CancellationToken.None);

    Assert.NotNull(loaded, "SyncService should persist a queue item for the local change.");
    Assert.Equal(queued.Id, loaded!.Id);
    Assert.Equal(taskId, loaded.LocalItemId);
    Assert.Equal(SourceLinkItemType.Task, loaded.LocalItemType);
    Assert.Equal(SourceSystem.MicrosoftToDo, loaded.SourceSystem);
    Assert.Equal(SyncQueueActionType.Update, loaded.ActionType);
    Assert.Equal(SyncState.Pending, loaded.SyncState);
    Assert.Equal("""{"title":"Call the pharmacy"}""", loaded.PayloadJson);
    Assert.Equal(now.ToUniversalTime(), loaded.CreatedAt);
}

static async Task SyncServiceReportsReadyWorkAndStatus()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 13, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    SyncService service = new(repository, () => now);

    SyncQueueItem pending = await service.QueueCreateAsync(
        SourceLinkItemType.Note,
        Guid.NewGuid(),
        SourceSystem.LocalImport,
        """{"text":"Remember the win"}""",
        cancellationToken: CancellationToken.None);
    SyncQueueItem syncing = await service.QueueDeleteAsync(
        SourceLinkItemType.Task,
        Guid.NewGuid(),
        SourceSystem.MicrosoftToDo,
        cancellationToken: CancellationToken.None);
    await service.MarkSyncingAsync(syncing.Id, now.AddMinutes(1), CancellationToken.None);

    IReadOnlyList<SyncQueueItem> readyItems = await service.GetReadyItemsAsync(10, CancellationToken.None);
    SyncQueueStatus status = await service.GetStatusAsync(CancellationToken.None);

    Assert.Equal(1, readyItems.Count);
    Assert.Equal(pending.Id, readyItems[0].Id);
    Assert.Equal(1, status.PendingCount);
    Assert.Equal(1, status.SyncingCount);
    Assert.Equal(0, status.FailedCount);
    Assert.Equal(2, status.TotalCount);
    Assert.True(status.HasActiveWork, "Pending or syncing work should be visible to the service.");
    Assert.Equal("1 sync item is running.", status.SummaryText);
}

static async Task SyncServiceUpdatesQueueItemStates()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 14, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    SyncService service = new(repository, () => now);
    SyncQueueItem queued = await service.QueueCreateAsync(
        SourceLinkItemType.Project,
        Guid.NewGuid(),
        SourceSystem.MicrosoftPlanner,
        "{}",
        cancellationToken: CancellationToken.None);

    SyncQueueItem? syncing = await service.MarkSyncingAsync(queued.Id, now.AddMinutes(1), CancellationToken.None);
    Assert.NotNull(syncing, "SyncService should return the item after marking it syncing.");
    Assert.Equal(SyncState.Syncing, syncing!.SyncState);
    Assert.Equal(now.AddMinutes(1).ToUniversalTime(), syncing.LastAttemptedAt);

    SyncQueueItem? failed = await service.MarkFailedAsync(
        queued.Id,
        "Planner adapter not connected.",
        nextAttemptAt: now.AddMinutes(30),
        failedAt: now.AddMinutes(2),
        cancellationToken: CancellationToken.None);
    Assert.NotNull(failed, "SyncService should return the item after marking it failed.");
    Assert.Equal(SyncState.Failed, failed!.SyncState);
    Assert.Equal(1, failed.RetryCount);
    Assert.Equal(now.AddMinutes(30).ToUniversalTime(), failed.NextAttemptAt);
    Assert.Equal("Planner adapter not connected.", failed.FailureMessage);

    SyncQueueItem? storedFailed = await repository.GetByIdAsync(queued.Id, CancellationToken.None);
    Assert.NotNull(storedFailed, "Failed sync queue item should remain stored for retry.");
    Assert.Equal(SyncState.Failed, storedFailed!.SyncState);

    SyncQueueItem? synced = await service.MarkSyncedAsync(queued.Id, now.AddMinutes(35), CancellationToken.None);
    Assert.NotNull(synced, "SyncService should return the item after marking it synced.");
    Assert.Equal(SyncState.Synced, synced!.SyncState);
    Assert.Equal(0, synced.RetryCount);
    Assert.Equal(string.Empty, synced.FailureMessage);
}

static async Task BackgroundSyncWorkerProcessesReadyQueueItems()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 15, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    SyncService service = new(repository, () => now);
    SyncQueueItem queued = await service.QueueUpdateAsync(
        SourceLinkItemType.Task,
        Guid.NewGuid(),
        SourceSystem.MicrosoftToDo,
        """{"title":"Call the pharmacy"}""",
        cancellationToken: CancellationToken.None);
    List<Guid> processedIds = [];
    BackgroundSyncWorker worker = new(
        service,
        (item, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            processedIds.Add(item.Id);
            return Task.CompletedTask;
        },
        pollInterval: TimeSpan.FromMilliseconds(50),
        nowProvider: () => now);

    BackgroundSyncWorkerRunResult result = await worker.RunOnceAsync(CancellationToken.None);
    SyncQueueItem? stored = await repository.GetByIdAsync(queued.Id, CancellationToken.None);

    Assert.Equal(1, result.ReadyItemCount);
    Assert.Equal(1, result.ProcessedItemCount);
    Assert.Equal(0, result.FailedItemCount);
    Assert.Equal(1, processedIds.Count);
    Assert.Equal(queued.Id, processedIds[0]);
    Assert.Equal("1 ready item was synced by the background worker.", worker.LastStatusText);
    Assert.NotNull(stored, "Processed sync queue item should still load.");
    Assert.Equal(SyncState.Synced, stored!.SyncState);
    Assert.Equal(0, stored.RetryCount);
    Assert.Null(stored.NextAttemptAt, "Successful sync should clear retry scheduling.");
    Assert.Equal(string.Empty, stored.FailureMessage);
}

static async Task BackgroundSyncWorkerRecordsProcessorFailures()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 16, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    TaskRepository taskRepository = new(paths);
    SyncService service = new(repository, () => now);
    TaskItem task = TaskItem.Capture("Draft Planner adapter notes");
    await taskRepository.AddAsync(task, CancellationToken.None);
    SyncQueueItem queued = await service.QueueCreateAsync(
        SourceLinkItemType.Task,
        task.Id,
        SourceSystem.MicrosoftPlanner,
        "{}",
        cancellationToken: CancellationToken.None);
    SyncRetryPolicy retryPolicy = new([TimeSpan.FromMinutes(2)]);
    BackgroundSyncWorker worker = new(
        service,
        (_, _) => throw new InvalidOperationException("Planner adapter missing."),
        pollInterval: TimeSpan.FromMilliseconds(50),
        retryPolicy: retryPolicy,
        nowProvider: () => now);

    BackgroundSyncWorkerRunResult result = await worker.RunOnceAsync(CancellationToken.None);
    SyncQueueItem? stored = await repository.GetByIdAsync(queued.Id, CancellationToken.None);
    TaskItem? storedTask = await taskRepository.GetByIdAsync(task.Id, CancellationToken.None);
    IReadOnlyList<SyncQueueItem> readyItems = await service.GetReadyItemsAsync(10, CancellationToken.None);
    SyncQueueStatus status = await service.GetStatusAsync(CancellationToken.None);

    Assert.Equal(1, result.ReadyItemCount);
    Assert.Equal(0, result.ProcessedItemCount);
    Assert.Equal(1, result.FailedItemCount);
    Assert.Equal("Planner adapter missing.", worker.LastErrorText);
    Assert.Equal("1 ready item failed in the background worker.", worker.LastStatusText);
    Assert.NotNull(stored, "Processor failure should not remove local queue work.");
    Assert.Equal(SyncState.Failed, stored!.SyncState);
    Assert.Equal(1, stored.RetryCount);
    Assert.Equal(now.ToUniversalTime(), stored.LastAttemptedAt);
    Assert.Equal(now.AddMinutes(2).ToUniversalTime(), stored.NextAttemptAt);
    Assert.Equal("Planner adapter missing.", stored.FailureMessage);
    Assert.NotNull(storedTask, "Failed sync must not remove the local task.");
    Assert.Equal(task.Id, storedTask!.Id);
    Assert.Equal(0, readyItems.Count);
    Assert.Equal(1, status.FailedCount);
    Assert.Equal("1 sync item needs attention.", status.SummaryText);
}

static async Task BackgroundSyncWorkerRetriesFailedQueueItems()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 6, 1, 17, 0, 0, TimeSpan.Zero);
    SyncQueueRepository repository = new(paths);
    SyncService service = new(repository, () => now);
    SyncQueueItem failedReadyItem = SyncQueueItem.Rehydrate(
        Guid.NewGuid(),
        SourceLinkItemType.Task,
        Guid.NewGuid(),
        SourceSystem.MicrosoftToDo,
        SyncQueueActionType.Update,
        """{"title":"Retry this"}""",
        SyncState.Failed,
        retryCount: 1,
        createdAt: now.AddMinutes(-20),
        updatedAt: now.AddMinutes(-10),
        nextAttemptAt: now.AddMinutes(-1),
        lastAttemptedAt: now.AddMinutes(-10),
        failureMessage: "Temporary network failure.");

    await repository.SaveAsync(failedReadyItem, CancellationToken.None);

    List<Guid> processedIds = [];
    BackgroundSyncWorker worker = new(
        service,
        (item, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            processedIds.Add(item.Id);
            Assert.Equal(SyncState.Syncing, item.SyncState);
            Assert.Equal(1, item.RetryCount);
            return Task.CompletedTask;
        },
        pollInterval: TimeSpan.FromMilliseconds(50),
        nowProvider: () => now);

    BackgroundSyncWorkerRunResult result = await worker.RunOnceAsync(CancellationToken.None);
    SyncQueueItem? stored = await repository.GetByIdAsync(failedReadyItem.Id, CancellationToken.None);

    Assert.Equal(1, result.ReadyItemCount);
    Assert.Equal(1, result.ProcessedItemCount);
    Assert.Equal(0, result.FailedItemCount);
    Assert.Equal(failedReadyItem.Id, processedIds[0]);
    Assert.NotNull(stored, "Retried queue item should remain available.");
    Assert.Equal(SyncState.Synced, stored!.SyncState);
    Assert.Equal(0, stored.RetryCount);
    Assert.Null(stored.NextAttemptAt, "Successful retry should clear next attempt.");
    Assert.Equal(string.Empty, stored.FailureMessage);
}

static async Task BackgroundWorkerHostStartsAndStopsWorkers()
{
    TestBackgroundWorker firstWorker = new();
    TestBackgroundWorker secondWorker = new();
    BackgroundWorkerHost host = new(firstWorker, secondWorker);

    await host.StartAsync(CancellationToken.None);

    Assert.True(host.IsRunning, "Host should report running after start.");
    Assert.True(firstWorker.IsRunning, "First worker should start.");
    Assert.True(secondWorker.IsRunning, "Second worker should start.");
    Assert.Equal(1, firstWorker.StartCount);
    Assert.Equal(1, secondWorker.StartCount);

    await host.StopAsync(CancellationToken.None);

    Assert.False(host.IsRunning, "Host should stop.");
    Assert.False(firstWorker.IsRunning, "First worker should stop.");
    Assert.False(secondWorker.IsRunning, "Second worker should stop.");
    Assert.Equal(1, firstWorker.StopCount);
    Assert.Equal(1, secondWorker.StopCount);
}

static async Task MicrosoftToDoConnectionTestServiceRespectsDisabledSetting()
{
    DateTimeOffset now = new(2026, 6, 1, 18, 0, 0, TimeSpan.Zero);
    TestMicrosoftToDoConnectionProbe probe = new((_, _) =>
        throw new InvalidOperationException("Disabled connection should not call the probe."));
    MicrosoftToDoConnectionTestService service = new(probe, () => now);
    ConnectionSettings connectionSettings = new()
    {
        EnableMicrosoftToDo = false
    };

    MicrosoftToDoConnectionStatus initialStatus = service.GetInitialStatus(connectionSettings);
    MicrosoftToDoConnectionStatus testedStatus =
        await service.TestConnectionAsync(connectionSettings, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.Disabled, initialStatus.State);
    Assert.Equal(MicrosoftToDoConnectionState.Disabled, testedStatus.State);
    Assert.False(testedStatus.CanTestConnection, "Disabled To Do status should not be testable.");
    Assert.Equal(0, probe.CallCount);
}

static async Task MicrosoftToDoConnectionTestServiceUsesConfiguredProbe()
{
    DateTimeOffset now = new(2026, 6, 1, 18, 30, 0, TimeSpan.Zero);
    TestMicrosoftToDoConnectionProbe probe = new((checkedAt, cancellationToken) =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(MicrosoftToDoConnectionStatus.Connected("smith@example.com", checkedAt));
    });
    MicrosoftToDoConnectionTestService service = new(probe, () => now);
    ConnectionSettings connectionSettings = new()
    {
        EnableMicrosoftToDo = true
    };

    MicrosoftToDoConnectionStatus initialStatus = service.GetInitialStatus(connectionSettings);
    MicrosoftToDoConnectionStatus testedStatus =
        await service.TestConnectionAsync(connectionSettings, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NotConnected, initialStatus.State);
    Assert.Equal(MicrosoftToDoConnectionState.Connected, testedStatus.State);
    Assert.Equal("smith@example.com", testedStatus.AccountDisplayName);
    Assert.Equal(now, testedStatus.LastCheckedAt);
    Assert.True(testedStatus.CanSync, "Connected To Do status should sync.");
    Assert.Equal(1, probe.CallCount);
}

static async Task MicrosoftToDoConnectionTestServiceMapsProbeFailures()
{
    DateTimeOffset now = new(2026, 6, 1, 19, 0, 0, TimeSpan.Zero);
    ConnectionSettings connectionSettings = new()
    {
        EnableMicrosoftToDo = true
    };
    MicrosoftToDoConnectionTestService unavailableService = new(
        new TestMicrosoftToDoConnectionProbe((_, _) =>
            throw new HttpRequestException("Network unavailable.")),
        () => now);
    MicrosoftToDoConnectionTestService failedService = new(
        new TestMicrosoftToDoConnectionProbe((_, _) =>
            throw new InvalidOperationException("Unexpected token cache error.")),
        () => now);

    MicrosoftToDoConnectionStatus unavailable =
        await unavailableService.TestConnectionAsync(connectionSettings, CancellationToken.None);
    MicrosoftToDoConnectionStatus failed =
        await failedService.TestConnectionAsync(connectionSettings, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.Unavailable, unavailable.State);
    Assert.Equal("Network unavailable.", unavailable.DetailText);
    Assert.Equal(now, unavailable.LastCheckedAt);
    Assert.True(unavailable.NeedsAttention, "Unavailable To Do status should need attention.");

    Assert.Equal(MicrosoftToDoConnectionState.Failed, failed.State);
    Assert.Equal("Unexpected token cache error.", failed.DetailText);
    Assert.Equal(now, failed.LastCheckedAt);
    Assert.True(failed.NeedsAttention, "Failed To Do status should need attention.");
}

static async Task MicrosoftToDoGraphConnectionProbeMapsTokenAndGraphResponses()
{
    DateTimeOffset now = new(2026, 6, 1, 19, 30, 0, TimeSpan.Zero);
    TestHttpMessageHandler noTokenHandler = new(_ =>
        throw new InvalidOperationException("No-token probe should not call Graph."));
    MicrosoftToDoGraphConnectionProbe noTokenProbe = new(
        new HttpClient(noTokenHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider(null));

    MicrosoftToDoConnectionStatus noTokenStatus =
        await noTokenProbe.TestConnectionAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, noTokenStatus.State);
    Assert.Equal(0, noTokenHandler.CallCount);
    Assert.Equal(now, noTokenStatus.LastCheckedAt);

    TestHttpMessageHandler successHandler = new(request =>
    {
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("token-123", request.Headers.Authorization?.Parameter);
        Assert.Contains("me/todo/lists", request.RequestUri?.ToString() ?? string.Empty);
        return new HttpResponseMessage(HttpStatusCode.OK);
    });
    MicrosoftToDoGraphConnectionProbe successProbe = new(
        new HttpClient(successHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider(" token-123 "));

    MicrosoftToDoConnectionStatus connected =
        await successProbe.TestConnectionAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.Connected, connected.State);
    Assert.True(connected.CanSync, "Successful Graph probe should allow sync.");
    Assert.Equal(1, successHandler.CallCount);

    TestHttpMessageHandler unauthorizedHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.Unauthorized));
    MicrosoftToDoGraphConnectionProbe unauthorizedProbe = new(
        new HttpClient(unauthorizedHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("expired-token"));

    MicrosoftToDoConnectionStatus needsSignIn =
        await unauthorizedProbe.TestConnectionAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, needsSignIn.State);
    Assert.True(needsSignIn.CanStartSignIn, "Unauthorized Graph response should allow sign-in.");

    TestHttpMessageHandler unavailableHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
    MicrosoftToDoGraphConnectionProbe unavailableProbe = new(
        new HttpClient(unavailableHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("token-123"));

    MicrosoftToDoConnectionStatus unavailable =
        await unavailableProbe.TestConnectionAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.Unavailable, unavailable.State);
    Assert.Contains("503", unavailable.DetailText);
}

static async Task MicrosoftPlannerAvailabilityTestServiceRespectsDisabledSetting()
{
    DateTimeOffset now = new(2026, 6, 6, 21, 0, 0, TimeSpan.Zero);
    TestMicrosoftPlannerAvailabilityProbe probe = new((_, _) =>
        throw new InvalidOperationException("Disabled Planner should not call the probe."));
    MicrosoftPlannerAvailabilityTestService service = new(probe, () => now);
    ConnectionSettings connectionSettings = new()
    {
        EnablePlanner = false
    };

    MicrosoftPlannerConnectionStatus initialStatus = service.GetInitialStatus(connectionSettings);
    MicrosoftPlannerConnectionStatus testedStatus =
        await service.TestAvailabilityAsync(connectionSettings, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.Disabled, initialStatus.State);
    Assert.Equal(MicrosoftPlannerConnectionState.Disabled, testedStatus.State);
    Assert.False(testedStatus.CanTestAvailability, "Disabled Planner status should not be testable.");
    Assert.Equal(0, probe.CallCount);
}

static async Task MicrosoftPlannerAvailabilityTestServiceUsesConfiguredProbe()
{
    DateTimeOffset now = new(2026, 6, 6, 21, 15, 0, TimeSpan.Zero);
    TestMicrosoftPlannerAvailabilityProbe probe = new((checkedAt, cancellationToken) =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(MicrosoftPlannerConnectionStatus.Available("smith@example.com", checkedAt));
    });
    MicrosoftPlannerAvailabilityTestService service = new(probe, () => now);
    ConnectionSettings connectionSettings = new()
    {
        EnablePlanner = true
    };

    MicrosoftPlannerConnectionStatus initialStatus = service.GetInitialStatus(connectionSettings);
    MicrosoftPlannerConnectionStatus testedStatus =
        await service.TestAvailabilityAsync(connectionSettings, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.NotConnected, initialStatus.State);
    Assert.Equal(MicrosoftPlannerConnectionState.Available, testedStatus.State);
    Assert.Equal("smith@example.com", testedStatus.AccountDisplayName);
    Assert.Equal(now, testedStatus.LastCheckedAt);
    Assert.True(testedStatus.CanReadPlannerTasks, "Available Planner status should allow reading tasks.");
    Assert.Equal(1, probe.CallCount);
}

static async Task MicrosoftPlannerAvailabilityTestServiceMapsProbeFailures()
{
    DateTimeOffset now = new(2026, 6, 6, 21, 30, 0, TimeSpan.Zero);
    ConnectionSettings connectionSettings = new()
    {
        EnablePlanner = true
    };
    MicrosoftPlannerAvailabilityTestService unavailableService = new(
        new TestMicrosoftPlannerAvailabilityProbe((_, _) =>
            throw new HttpRequestException("Network unavailable.")),
        () => now);
    MicrosoftPlannerAvailabilityTestService failedService = new(
        new TestMicrosoftPlannerAvailabilityProbe((_, _) =>
            throw new InvalidOperationException("Unexpected Planner probe error.")),
        () => now);

    MicrosoftPlannerConnectionStatus unavailable =
        await unavailableService.TestAvailabilityAsync(connectionSettings, CancellationToken.None);
    MicrosoftPlannerConnectionStatus failed =
        await failedService.TestAvailabilityAsync(connectionSettings, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.Unavailable, unavailable.State);
    Assert.Equal("Network unavailable.", unavailable.DetailText);
    Assert.Equal(now, unavailable.LastCheckedAt);
    Assert.True(unavailable.NeedsAttention, "Unavailable Planner status should need attention.");

    Assert.Equal(MicrosoftPlannerConnectionState.Failed, failed.State);
    Assert.Equal("Unexpected Planner probe error.", failed.DetailText);
    Assert.Equal(now, failed.LastCheckedAt);
    Assert.True(failed.NeedsAttention, "Failed Planner status should need attention.");
}

static async Task MicrosoftPlannerGraphAvailabilityProbeMapsTokenAndGraphResponses()
{
    DateTimeOffset now = new(2026, 6, 6, 22, 0, 0, TimeSpan.Zero);
    TestHttpMessageHandler noTokenHandler = new(_ =>
        throw new InvalidOperationException("No-token Planner probe should not call Graph."));
    MicrosoftPlannerGraphAvailabilityProbe noTokenProbe = new(
        new HttpClient(noTokenHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider(null));

    MicrosoftPlannerConnectionStatus noTokenStatus =
        await noTokenProbe.TestAvailabilityAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.NeedsSignIn, noTokenStatus.State);
    Assert.Equal(0, noTokenHandler.CallCount);
    Assert.Equal(now, noTokenStatus.LastCheckedAt);

    TestHttpMessageHandler successHandler = new(request =>
    {
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("planner-token", request.Headers.Authorization?.Parameter);
        Assert.Contains("me/planner/tasks", request.RequestUri?.ToString() ?? string.Empty);
        return JsonResponse("""{ "value": [] }""");
    });
    MicrosoftPlannerGraphAvailabilityProbe successProbe = new(
        new HttpClient(successHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider(" planner-token "));

    MicrosoftPlannerConnectionStatus available =
        await successProbe.TestAvailabilityAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.Available, available.State);
    Assert.True(available.CanReadPlannerTasks, "Successful Planner probe should allow reading tasks.");
    Assert.Equal(1, successHandler.CallCount);

    TestHttpMessageHandler unauthorizedHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.Unauthorized));
    MicrosoftPlannerGraphAvailabilityProbe unauthorizedProbe = new(
        new HttpClient(unauthorizedHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider("expired-token"));

    MicrosoftPlannerConnectionStatus needsSignIn =
        await unauthorizedProbe.TestAvailabilityAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.NeedsSignIn, needsSignIn.State);
    Assert.True(needsSignIn.CanStartSignIn, "Unauthorized Planner response should allow sign-in.");

    TestHttpMessageHandler unavailableHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.Forbidden));
    MicrosoftPlannerGraphAvailabilityProbe unavailableProbe = new(
        new HttpClient(unavailableHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider("token-123"));

    MicrosoftPlannerConnectionStatus unavailable =
        await unavailableProbe.TestAvailabilityAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.Unavailable, unavailable.State);
    Assert.Contains("work or school account", unavailable.DetailText);
    Assert.False(unavailable.CanReadPlannerTasks, "Unavailable Planner should not read tasks.");

    TestHttpMessageHandler failedHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.Conflict));
    MicrosoftPlannerGraphAvailabilityProbe failedProbe = new(
        new HttpClient(failedHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider("token-123"));

    MicrosoftPlannerConnectionStatus failed =
        await failedProbe.TestAvailabilityAsync(now, CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.Failed, failed.State);
    Assert.Contains("409", failed.DetailText);
}

static async Task MicrosoftPlannerGraphTaskAdapterNeedsSignInWithoutToken()
{
    DateTimeOffset now = new(2026, 6, 7, 8, 0, 0, TimeSpan.Zero);
    TestHttpMessageHandler handler = new(_ =>
        throw new InvalidOperationException("No-token Planner import should not call Graph."));
    MicrosoftPlannerGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider(null),
        () => now);

    MicrosoftPlannerPullResult result = await adapter.PullAssignedTasksAsync(CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.NeedsSignIn, result.ConnectionStatus.State);
    Assert.Equal(now, result.ConnectionStatus.LastCheckedAt);
    Assert.Equal(0, handler.CallCount);
    Assert.False(result.HasData, "No-token Planner pull should not return data.");
    Assert.False(result.CanUseData, "No-token Planner data should not be usable.");
}

static async Task MicrosoftPlannerGraphTaskAdapterPullsAssignedTasks()
{
    DateTimeOffset now = new(2026, 6, 7, 8, 15, 0, TimeSpan.Zero);
    TestHttpMessageHandler handler = new(request =>
    {
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("planner-token", request.Headers.Authorization?.Parameter);
        Assert.Contains("me/planner/tasks", request.RequestUri?.ToString() ?? string.Empty);

        return JsonResponse(
            """
            {
              "value": [
                {
                  "id": "planner-task-1",
                  "title": "Draft the personal plan",
                  "planId": "plan-1",
                  "bucketId": "bucket-1",
                  "percentComplete": 50,
                  "priority": 1,
                  "createdDateTime": "2026-06-01T12:00:00Z",
                  "startDateTime": "2026-06-07T14:00:00Z",
                  "dueDateTime": "2026-06-08T22:00:00Z"
                },
                {
                  "id": "planner-task-2",
                  "title": "Celebrate the small win",
                  "planId": "plan-1",
                  "percentComplete": 100,
                  "completedDateTime": "2026-06-07T15:30:00Z"
                },
                {
                  "id": "",
                  "title": "Ignored without id"
                }
              ]
            }
            """);
    });
    MicrosoftPlannerGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider(" planner-token "),
        () => now);

    MicrosoftPlannerPullResult result = await adapter.PullAssignedTasksAsync(CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.Available, result.ConnectionStatus.State);
    Assert.True(result.CanUseData, "Successful Planner pull should be usable.");
    Assert.Equal(2, result.Tasks.Count);
    Assert.Equal(1, handler.CallCount);

    MicrosoftPlannerTaskItem activeTask = result.Tasks[0];
    Assert.Equal("planner-task-1", activeTask.Id);
    Assert.Equal("Draft the personal plan", activeTask.Title);
    Assert.Equal("plan-1", activeTask.PlanId);
    Assert.Equal("bucket-1", activeTask.BucketId);
    Assert.Equal(50, activeTask.PercentComplete);
    Assert.Equal(1, activeTask.Priority);
    Assert.Equal(new DateTimeOffset(2026, 6, 8, 22, 0, 0, TimeSpan.Zero), activeTask.DueAt);

    MicrosoftPlannerTaskItem completedTask = result.Tasks[1];
    Assert.True(completedTask.IsComplete, "Completed Planner task should be recognized.");
}

static async Task MicrosoftPlannerGraphTaskAdapterMapsPullFailures()
{
    DateTimeOffset now = new(2026, 6, 7, 8, 30, 0, TimeSpan.Zero);
    MicrosoftPlannerGraphTaskAdapter unauthorizedAdapter = new(
        new HttpClient(new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Unauthorized)))
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider("expired-token"),
        () => now);
    MicrosoftPlannerGraphTaskAdapter unavailableAdapter = new(
        new HttpClient(new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Forbidden)))
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftPlannerAccessTokenProvider("token-123"),
        () => now);

    MicrosoftPlannerPullResult unauthorized =
        await unauthorizedAdapter.PullAssignedTasksAsync(CancellationToken.None);
    MicrosoftPlannerPullResult unavailable =
        await unavailableAdapter.PullAssignedTasksAsync(CancellationToken.None);

    Assert.Equal(MicrosoftPlannerConnectionState.NeedsSignIn, unauthorized.ConnectionStatus.State);
    Assert.False(unauthorized.CanUseData, "Unauthorized Planner pull should not be usable.");

    Assert.Equal(MicrosoftPlannerConnectionState.Unavailable, unavailable.ConnectionStatus.State);
    Assert.Contains("work or school account", unavailable.ConnectionStatus.DetailText);
    Assert.False(unavailable.CanUseData, "Unavailable Planner pull should not be usable.");
}

static async Task MicrosoftPlannerTaskImportServiceImportsAssignedTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePlanner = true;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset importedAt = new(2026, 6, 7, 9, 0, 0, TimeSpan.Zero);
    TestMicrosoftPlannerTaskAdapter adapter = new(new MicrosoftPlannerPullResult(
        MicrosoftPlannerConnectionStatus.Available(checkedAt: importedAt),
        [
            new MicrosoftPlannerTaskItem(
                "planner-task-1",
                "Draft the personal plan",
                "plan-1",
                "bucket-1",
                percentComplete: 50,
                priority: 1,
                startAt: new DateTimeOffset(2026, 6, 7, 14, 0, 0, TimeSpan.Zero),
                dueAt: new DateTimeOffset(2026, 6, 8, 22, 0, 0, TimeSpan.Zero)),
            new MicrosoftPlannerTaskItem(
                "planner-task-2",
                "Celebrate the small win",
                "plan-1",
                percentComplete: 100,
                completedAt: new DateTimeOffset(2026, 6, 7, 15, 30, 0, TimeSpan.Zero))
        ]));
    MicrosoftPlannerTaskImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository,
        adapter,
        nowProvider: () => importedAt);

    MicrosoftPlannerImportResult result =
        await service.ImportAssignedTasksAsync(CancellationToken.None);

    Assert.True(result.WasSuccessful, "Planner import should report success.");
    Assert.Equal("Planner tasks imported", result.StatusText);
    Assert.Contains("Imported 2 Planner tasks", result.DetailText);
    Assert.Equal(2, result.ImportedCount);
    Assert.Equal(1, adapter.CallCount);

    IReadOnlyList<TaskItem> localTasks = await taskRepository.GetAllAsync(CancellationToken.None);
    Assert.Equal(2, localTasks.Count);

    TaskItem activeTask = localTasks.Single(task => task.Title == "Draft the personal plan");
    Assert.Equal(TaskItemStatus.InProgress, activeTask.Status);
    Assert.Equal(TaskPriority.High, activeTask.Priority);
    Assert.Equal(new DateOnly(2026, 6, 7), activeTask.StartDate);
    Assert.Equal(new DateOnly(2026, 6, 8), activeTask.DueDate);
    Assert.Contains("Imported read-only from Microsoft Planner.", activeTask.Notes);
    Assert.Contains("Planner Task Id: planner-task-1", activeTask.Notes);
    Assert.Contains("Planner plan id: plan-1", activeTask.Notes);
    Assert.Contains("Planner bucket id: bucket-1", activeTask.Notes);
    Assert.Contains("Planner percent complete: 50%", activeTask.Notes);
    Assert.Contains("Microsoft Planner", activeTask.Tags);
    Assert.Contains("Planner Import", activeTask.Tags);
    Assert.Contains("Read Only", activeTask.Tags);
    Assert.Contains(activeTask.Id, result.LocalTaskIds);

    IReadOnlyList<SourceLink> activeTaskLinks = await sourceLinkRepository.GetForLocalItemAsync(
        SourceLinkItemType.Task,
        activeTask.Id,
        CancellationToken.None);
    Assert.Equal(1, activeTaskLinks.Count);
    SourceLink activeTaskLink = activeTaskLinks[0];
    Assert.Equal(SourceSystem.MicrosoftPlanner, activeTaskLink.SourceSystem);
    Assert.Equal("planner-task-1", activeTaskLink.ExternalId);
    Assert.Equal("plan-1", activeTaskLink.ExternalContainerId);
    Assert.Equal("Draft the personal plan", activeTaskLink.ExternalDisplayName);
    Assert.Equal(SyncState.Synced, activeTaskLink.SyncState);
    Assert.True(activeTaskLink.IsReadOnly, "Planner imports should be tracked as read-only source links.");
    Assert.Equal(importedAt, activeTaskLink.LastSyncedAt!.Value);
    Assert.Contains(activeTaskLink.Id, result.SourceLinkIds);

    SourceLink? lookup = await sourceLinkRepository.GetByExternalReferenceAsync(
        SourceSystem.MicrosoftPlanner,
        "planner-task-1",
        CancellationToken.None);
    Assert.NotNull(lookup, "Planner source link should be found by external reference.");
    Assert.Equal(activeTaskLink.Id, lookup!.Id);

    TaskItem completedTask = localTasks.Single(task => task.Title == "Celebrate the small win");
    Assert.Equal(TaskItemStatus.Done, completedTask.Status);
    Assert.True(completedTask.CompletedAt.HasValue, "A completed Planner task should map to done locally.");
}

static async Task MicrosoftPlannerTaskImportServiceSkipsExistingTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePlanner = true;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset importedAt = new(2026, 6, 7, 9, 30, 0, TimeSpan.Zero);
    MicrosoftPlannerPullResult pullResult = new(
        MicrosoftPlannerConnectionStatus.Available(checkedAt: importedAt),
        [new MicrosoftPlannerTaskItem("planner-task-1", "Draft the personal plan", "plan-1")]);
    TestMicrosoftPlannerTaskAdapter adapter = new(pullResult, pullResult);
    MicrosoftPlannerTaskImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository,
        adapter,
        nowProvider: () => importedAt);

    MicrosoftPlannerImportResult firstImport =
        await service.ImportAssignedTasksAsync(CancellationToken.None);
    MicrosoftPlannerImportResult secondImport =
        await service.ImportAssignedTasksAsync(CancellationToken.None);

    Assert.Equal(1, firstImport.ImportedCount);
    Assert.Equal(0, firstImport.SkippedExistingCount);
    Assert.Equal(0, secondImport.ImportedCount);
    Assert.Equal(1, secondImport.SkippedExistingCount);
    Assert.Equal("Planner tasks already local", secondImport.StatusText);
    Assert.Equal(1, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task MicrosoftPlannerTaskImportServiceReportsDisabledPlannerImport()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePlanner = false;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    TestMicrosoftPlannerTaskAdapter adapter = new(new MicrosoftPlannerPullResult(
        MicrosoftPlannerConnectionStatus.Available(),
        [new MicrosoftPlannerTaskItem("planner-task-1", "Should not import")]));
    MicrosoftPlannerTaskImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository,
        adapter);

    MicrosoftPlannerImportResult result =
        await service.ImportAssignedTasksAsync(CancellationToken.None);

    Assert.False(result.WasSuccessful, "Disabled Planner import should report a recoverable failure.");
    Assert.Equal("Planner import off", result.StatusText);
    Assert.Contains("Turn on Planner", result.DetailText);
    Assert.Equal(0, adapter.CallCount);
    Assert.Equal(0, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task PhoneCompanionCaptureImportServiceImportsCapturesIntoLocalInbox()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePhoneCompanion = true;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset importedAt = new(2026, 6, 7, 10, 30, 0, TimeSpan.Zero);
    DateTimeOffset capturedAt = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
    PhoneCompanionCaptureImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository,
        nowProvider: () => importedAt);
    PhoneCompanionSyncBatch batch = new(
        "batch-1",
        "device-1",
        "Smith phone",
        capturedAt,
        [
            new PhoneCompanionQuickCaptureItem(
                "phone-1",
                "Call pharmacy",
                capturedAt,
                "Ask about refill timing.",
                tags: ["Health"]),
            new PhoneCompanionQuickCaptureItem(
                "phone-2",
                "Buy walking shoes",
                capturedAt.AddMinutes(1),
                dueDate: new DateOnly(2026, 6, 8),
                destination: PhoneCompanionCaptureDestination.LetSuccessPlannerChoose)
        ]);

    PhoneCompanionSyncResult result = await service.ImportBatchAsync(batch, CancellationToken.None);

    Assert.True(result.WasSuccessful, "Phone import should report success.");
    Assert.Equal("Phone captures imported", result.StatusText);
    Assert.Equal(2, result.ImportedCount);
    Assert.Equal(0, result.SkippedCount);
    Assert.Equal(0, result.RejectedCount);

    IReadOnlyList<TaskItem> localTasks = await taskRepository.GetAllAsync(CancellationToken.None);
    Assert.Equal(2, localTasks.Count);

    TaskItem pharmacyTask = localTasks.Single(task => task.Title == "Call pharmacy");
    Assert.Equal(TaskItemStatus.Captured, pharmacyTask.Status);
    Assert.Equal(capturedAt, pharmacyTask.CreatedAt);
    Assert.Equal("Ask about refill timing.", pharmacyTask.Notes);
    Assert.Contains("Phone Companion", pharmacyTask.Tags);
    Assert.Contains("Phone Capture", pharmacyTask.Tags);
    Assert.Contains("Health", pharmacyTask.Tags);
    Assert.Contains(pharmacyTask.Id, result.Outcomes.Select(outcome => outcome.LocalTaskId.GetValueOrDefault()));

    IReadOnlyList<SourceLink> pharmacyLinks = await sourceLinkRepository.GetForLocalItemAsync(
        SourceLinkItemType.Task,
        pharmacyTask.Id,
        CancellationToken.None);
    Assert.Equal(1, pharmacyLinks.Count);
    SourceLink pharmacyLink = pharmacyLinks[0];
    Assert.Equal(SourceSystem.PhoneCompanion, pharmacyLink.SourceSystem);
    Assert.Equal("device-1:phone-1", pharmacyLink.ExternalId);
    Assert.Equal("device-1", pharmacyLink.ExternalContainerId);
    Assert.Equal("Call pharmacy", pharmacyLink.ExternalDisplayName);
    Assert.Equal(SyncState.Synced, pharmacyLink.SyncState);
    Assert.False(pharmacyLink.IsReadOnly, "Phone captures should remain editable local tasks.");
    Assert.Equal(importedAt, pharmacyLink.LastSyncedAt!.Value);

    TaskItem shoesTask = localTasks.Single(task => task.Title == "Buy walking shoes");
    Assert.Equal(TaskItemStatus.Planned, shoesTask.Status);
    Assert.Equal(new DateOnly(2026, 6, 8), shoesTask.DueDate);
    Assert.Contains("MCP Chosen", shoesTask.Tags);
}

static async Task PhoneCompanionCaptureImportServiceSkipsExistingCaptures()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePhoneCompanion = true;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset capturedAt = new(2026, 6, 7, 11, 0, 0, TimeSpan.Zero);
    PhoneCompanionCaptureImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository,
        nowProvider: () => capturedAt);
    PhoneCompanionSyncBatch batch = new(
        "batch-duplicate",
        "device-1",
        "Smith phone",
        capturedAt,
        [new PhoneCompanionQuickCaptureItem("phone-1", "Call pharmacy", capturedAt)]);

    PhoneCompanionSyncResult firstImport = await service.ImportBatchAsync(batch, CancellationToken.None);
    PhoneCompanionSyncResult secondImport = await service.ImportBatchAsync(batch, CancellationToken.None);

    Assert.Equal(1, firstImport.ImportedCount);
    Assert.Equal(0, firstImport.SkippedCount);
    Assert.Equal(0, secondImport.ImportedCount);
    Assert.Equal(1, secondImport.SkippedCount);
    Assert.Equal("No new phone captures", secondImport.StatusText);
    Assert.Contains("already existed", secondImport.DetailText);
    Assert.Equal(1, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task PhoneCompanionCaptureImportServiceReportsDisabledImport()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePhoneCompanion = false;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset capturedAt = new(2026, 6, 7, 12, 0, 0, TimeSpan.Zero);
    PhoneCompanionCaptureImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository);
    PhoneCompanionSyncBatch batch = new(
        "batch-disabled",
        "device-1",
        "Smith phone",
        capturedAt,
        [new PhoneCompanionQuickCaptureItem("phone-1", "Should not import", capturedAt)]);

    PhoneCompanionSyncResult result = await service.ImportBatchAsync(batch, CancellationToken.None);

    Assert.False(result.WasSuccessful, "Disabled Phone Companion import should report a recoverable failure.");
    Assert.Equal("Phone sync unavailable", result.StatusText);
    Assert.Contains("Turn on Phone Companion", result.DetailText);
    Assert.Equal(0, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task PhoneCompanionCaptureImportServiceRejectsUnsupportedDestinations()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnablePhoneCompanion = true;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset capturedAt = new(2026, 6, 7, 13, 0, 0, TimeSpan.Zero);
    PhoneCompanionCaptureImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository);
    PhoneCompanionSyncBatch batch = new(
        "batch-partial",
        "device-1",
        "Smith phone",
        capturedAt,
        [
            new PhoneCompanionQuickCaptureItem(
                "phone-1",
                "Save locally",
                capturedAt,
                destination: PhoneCompanionCaptureDestination.LocalInbox),
            new PhoneCompanionQuickCaptureItem(
                "phone-2",
                "Send to To Do later",
                capturedAt,
                destination: PhoneCompanionCaptureDestination.MicrosoftToDo)
        ]);

    PhoneCompanionSyncResult result = await service.ImportBatchAsync(batch, CancellationToken.None);

    Assert.Equal(PhoneCompanionSyncResultState.Partial, result.State);
    Assert.True(result.NeedsAttention, "Unsupported destinations should stay visible.");
    Assert.Equal(1, result.ImportedCount);
    Assert.Equal(1, result.RejectedCount);
    Assert.Contains("Only local inbox", result.Outcomes.Single(outcome =>
        outcome.State == PhoneCompanionCaptureImportState.Rejected).Message);
    Assert.Equal(1, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task MicrosoftToDoGraphTaskAdapterNeedsSignInWithoutToken()
{
    DateTimeOffset now = new(2026, 6, 1, 20, 0, 0, TimeSpan.Zero);
    TestHttpMessageHandler handler = new(_ =>
        throw new InvalidOperationException("No-token adapter should not call Graph."));
    MicrosoftToDoGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider(null),
        () => now);

    MicrosoftToDoPullResult result = await adapter.PullListsAndTasksAsync(CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, result.ConnectionStatus.State);
    Assert.Equal(now, result.ConnectionStatus.LastCheckedAt);
    Assert.Equal(0, handler.CallCount);
    Assert.False(result.HasData, "No-token pull should not return data.");
    Assert.False(result.CanUseData, "No-token pull data should not be usable.");
}

static async Task MicrosoftToDoGraphTaskAdapterPullsListsAndTasks()
{
    DateTimeOffset now = new(2026, 6, 1, 20, 30, 0, TimeSpan.Zero);
    List<string> requestedUris = [];
    TestHttpMessageHandler handler = new(request =>
    {
        requestedUris.Add(request.RequestUri?.ToString() ?? string.Empty);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("token-123", request.Headers.Authorization?.Parameter);

        string requestUri = request.RequestUri?.ToString() ?? string.Empty;
        if (requestUri.Contains("me/todo/lists?", StringComparison.Ordinal))
        {
            return JsonResponse(
                """
                {
                  "value": [
                    {
                      "id": "list-1",
                      "displayName": "Tasks",
                      "wellknownListName": "defaultList"
                    }
                  ]
                }
                """);
        }

        if (requestUri.Contains("me/todo/lists/list-1/tasks?", StringComparison.Ordinal))
        {
            return JsonResponse(
                """
                {
                  "value": [
                    {
                      "id": "task-1",
                      "title": "Call the pharmacy",
                      "status": "notStarted",
                      "importance": "high",
                      "body": {
                        "content": "Pick up refill",
                        "contentType": "text"
                      },
                      "dueDateTime": {
                        "dateTime": "2026-06-02T09:00:00Z",
                        "timeZone": "UTC"
                      },
                      "lastModifiedDateTime": "2026-06-01T19:45:00Z",
                      "webLink": "https://to-do.office.com/tasks/task-1"
                    },
                    {
                      "id": "task-2",
                      "title": "Already done",
                      "status": "completed",
                      "importance": "normal",
                      "completedDateTime": {
                        "dateTime": "2026-06-01T20:15:00Z",
                        "timeZone": "UTC"
                      }
                    }
                  ]
                }
                """);
        }

        return new HttpResponseMessage(HttpStatusCode.NotFound);
    });
    MicrosoftToDoGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider(" token-123 "),
        () => now);

    MicrosoftToDoPullResult result = await adapter.PullListsAndTasksAsync(CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.Connected, result.ConnectionStatus.State);
    Assert.Equal(now, result.ConnectionStatus.LastCheckedAt);
    Assert.Contains("Pulled 1 list and 2 tasks", result.ConnectionStatus.DetailText);
    Assert.True(result.HasData, "Successful pull should return data.");
    Assert.True(result.CanUseData, "Successful pull data should be usable.");
    Assert.Equal(1, result.TaskLists.Count);
    Assert.Equal("list-1", result.TaskLists[0].Id);
    Assert.Equal("Tasks", result.TaskLists[0].DisplayName);
    Assert.Equal("defaultList", result.TaskLists[0].WellKnownListName);
    Assert.Equal(2, result.Tasks.Count);
    Assert.Equal("task-1", result.Tasks[0].Id);
    Assert.Equal("list-1", result.Tasks[0].ListId);
    Assert.Equal("Call the pharmacy", result.Tasks[0].Title);
    Assert.Equal("notStarted", result.Tasks[0].Status);
    Assert.Equal("high", result.Tasks[0].Importance);
    Assert.Equal("Pick up refill", result.Tasks[0].BodyContent);
    Assert.Equal(new DateTimeOffset(2026, 6, 2, 9, 0, 0, TimeSpan.Zero), result.Tasks[0].DueAt);
    Assert.Equal(new DateTimeOffset(2026, 6, 1, 19, 45, 0, TimeSpan.Zero), result.Tasks[0].LastModifiedAt);
    Assert.Equal("https://to-do.office.com/tasks/task-1", result.Tasks[0].WebLink);
    Assert.False(result.Tasks[0].IsCompleted, "First pulled To Do task should not be complete.");
    Assert.Equal("task-2", result.Tasks[1].Id);
    Assert.True(result.Tasks[1].IsCompleted, "Second pulled To Do task should be complete.");
    Assert.Equal(new DateTimeOffset(2026, 6, 1, 20, 15, 0, TimeSpan.Zero), result.Tasks[1].CompletedAt);
    Assert.Equal(2, requestedUris.Count);
}

static async Task MicrosoftToDoGraphTaskAdapterMapsPullFailures()
{
    DateTimeOffset now = new(2026, 6, 1, 21, 0, 0, TimeSpan.Zero);
    TestHttpMessageHandler unauthorizedHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.Unauthorized));
    MicrosoftToDoGraphTaskAdapter unauthorizedAdapter = new(
        new HttpClient(unauthorizedHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("expired-token"),
        () => now);
    TestHttpMessageHandler unavailableHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
    MicrosoftToDoGraphTaskAdapter unavailableAdapter = new(
        new HttpClient(unavailableHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("token-123"),
        () => now);

    MicrosoftToDoPullResult unauthorized =
        await unauthorizedAdapter.PullListsAndTasksAsync(CancellationToken.None);
    MicrosoftToDoPullResult unavailable =
        await unavailableAdapter.PullListsAndTasksAsync(CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, unauthorized.ConnectionStatus.State);
    Assert.True(unauthorized.ConnectionStatus.CanStartSignIn, "Unauthorized pull should allow sign-in.");
    Assert.False(unauthorized.HasData, "Unauthorized pull should not return data.");

    Assert.Equal(MicrosoftToDoConnectionState.Unavailable, unavailable.ConnectionStatus.State);
    Assert.Contains("503", unavailable.ConnectionStatus.DetailText);
    Assert.False(unavailable.HasData, "Unavailable pull should not return data.");
}

static async Task MicrosoftToDoGraphTaskAdapterPushesCapturedTasks()
{
    DateTimeOffset now = new(2026, 6, 1, 21, 30, 0, TimeSpan.Zero);
    string postedBody = string.Empty;
    TaskItem task = TaskItem.Capture("  Call the pharmacy  ");
    task.UpdateNotes("Pick up refill");
    task.Schedule(new DateOnly(2026, 6, 2));
    task.SetPriority(TaskPriority.High);

    TestHttpMessageHandler handler = new(request =>
    {
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("token-123", request.Headers.Authorization?.Parameter);
        Assert.Equal(
            "https://graph.test/v1.0/me/todo/lists/list-1/tasks",
            request.RequestUri?.ToString() ?? string.Empty);

        Assert.NotNull(request.Content, "Push request should include JSON content.");
        postedBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
        using JsonDocument postedJson = JsonDocument.Parse(postedBody);
        JsonElement root = postedJson.RootElement;

        Assert.Equal("Call the pharmacy", root.GetProperty("title").GetString());
        Assert.Equal("high", root.GetProperty("importance").GetString());
        Assert.Equal("Pick up refill", root.GetProperty("body").GetProperty("content").GetString());
        Assert.Equal("text", root.GetProperty("body").GetProperty("contentType").GetString());
        Assert.Equal("2026-06-02T00:00:00", root.GetProperty("dueDateTime").GetProperty("dateTime").GetString());
        Assert.Equal("UTC", root.GetProperty("dueDateTime").GetProperty("timeZone").GetString());

        return JsonResponse(
            """
            {
              "id": "todo-task-1",
              "title": "Call the pharmacy",
              "status": "notStarted",
              "importance": "high",
              "body": {
                "content": "Pick up refill",
                "contentType": "text"
              },
              "dueDateTime": {
                "dateTime": "2026-06-02T00:00:00Z",
                "timeZone": "UTC"
              },
              "lastModifiedDateTime": "2026-06-01T21:29:00Z",
              "webLink": "https://to-do.office.com/tasks/todo-task-1",
              "@odata.etag": "W/\"etag-1\""
            }
            """,
            HttpStatusCode.Created);
    });
    MicrosoftToDoGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider(" token-123 "),
        () => now);

    MicrosoftToDoPushResult result = await adapter.PushCapturedTaskAsync(
        new MicrosoftToDoPushRequest(task, " list-1 "),
        CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.Connected, result.ConnectionStatus.State);
    Assert.Equal(now, result.ConnectionStatus.LastCheckedAt);
    Assert.Contains("Pushed", result.ConnectionStatus.DetailText);
    Assert.True(result.WasPushed, "Successful push should expose created task and source link.");
    Assert.Equal(1, handler.CallCount);
    Assert.True(!string.IsNullOrWhiteSpace(postedBody), "Push should send a JSON body.");

    Assert.NotNull(result.PushedTask, "Successful push should include the To Do task.");
    MicrosoftToDoTaskItem pushedTask = result.PushedTask!;
    Assert.Equal("todo-task-1", pushedTask.Id);
    Assert.Equal("list-1", pushedTask.ListId);
    Assert.Equal("Call the pharmacy", pushedTask.Title);
    Assert.Equal("high", pushedTask.Importance);
    Assert.Equal("Pick up refill", pushedTask.BodyContent);
    Assert.Equal("https://to-do.office.com/tasks/todo-task-1", pushedTask.WebLink);

    Assert.NotNull(result.SourceLink, "Successful push should include a source link.");
    SourceLink sourceLink = result.SourceLink!;
    Assert.Equal(SourceLinkItemType.Task, sourceLink.LocalItemType);
    Assert.Equal(task.Id, sourceLink.LocalItemId);
    Assert.Equal(SourceSystem.MicrosoftToDo, sourceLink.SourceSystem);
    Assert.Equal("todo-task-1", sourceLink.ExternalId);
    Assert.Equal("list-1", sourceLink.ExternalContainerId);
    Assert.Equal("Call the pharmacy", sourceLink.ExternalDisplayName);
    Assert.Equal("https://to-do.office.com/tasks/todo-task-1", sourceLink.ExternalWebUrl);
    Assert.Equal("W/\"etag-1\"", sourceLink.SourceVersion);
    Assert.Equal(SyncState.Synced, sourceLink.SyncState);
    Assert.Equal(now, sourceLink.LastSyncedAt);
}

static async Task MicrosoftToDoGraphTaskAdapterDoesNotPushWithoutToken()
{
    DateTimeOffset now = new(2026, 6, 1, 21, 45, 0, TimeSpan.Zero);
    TaskItem task = TaskItem.Capture("Write local success note");
    TestHttpMessageHandler handler = new(_ =>
        throw new InvalidOperationException("No-token adapter should not call Graph."));
    MicrosoftToDoGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider(null),
        () => now);

    MicrosoftToDoPushResult result = await adapter.PushCapturedTaskAsync(
        new MicrosoftToDoPushRequest(task, "list-1"),
        CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, result.ConnectionStatus.State);
    Assert.Equal(now, result.ConnectionStatus.LastCheckedAt);
    Assert.False(result.WasPushed, "No-token push should not report a pushed task.");
    Assert.Null(result.PushedTask, "No-token push should not return a To Do task.");
    Assert.Null(result.SourceLink, "No-token push should not return a source link.");
    Assert.Equal(0, handler.CallCount);
}

static async Task MicrosoftToDoGraphTaskAdapterMapsPushFailures()
{
    DateTimeOffset now = new(2026, 6, 1, 22, 0, 0, TimeSpan.Zero);
    TaskItem task = TaskItem.Capture("Send test push");
    TestHttpMessageHandler unauthorizedHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.Unauthorized));
    MicrosoftToDoGraphTaskAdapter unauthorizedAdapter = new(
        new HttpClient(unauthorizedHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("expired-token"),
        () => now);
    TestHttpMessageHandler unavailableHandler = new(_ =>
        new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
    MicrosoftToDoGraphTaskAdapter unavailableAdapter = new(
        new HttpClient(unavailableHandler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("token-123"),
        () => now);

    MicrosoftToDoPushResult unauthorized = await unauthorizedAdapter.PushCapturedTaskAsync(
        new MicrosoftToDoPushRequest(task, "list-1"),
        CancellationToken.None);
    MicrosoftToDoPushResult unavailable = await unavailableAdapter.PushCapturedTaskAsync(
        new MicrosoftToDoPushRequest(task, "list-1"),
        CancellationToken.None);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, unauthorized.ConnectionStatus.State);
    Assert.True(unauthorized.ConnectionStatus.CanStartSignIn, "Unauthorized push should allow sign-in.");
    Assert.False(unauthorized.WasPushed, "Unauthorized push should not report success.");

    Assert.Equal(MicrosoftToDoConnectionState.Unavailable, unavailable.ConnectionStatus.State);
    Assert.Contains("503", unavailable.ConnectionStatus.DetailText);
    Assert.False(unavailable.WasPushed, "Unavailable push should not report success.");
}

static async Task MicrosoftToDoTaskPushServiceSavesSourceLinks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);
    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    DateTimeOffset now = new(2026, 6, 1, 22, 15, 0, TimeSpan.Zero);
    TaskItem task = TaskItem.Capture("File the receipt");
    SourceLinkRepository sourceLinkRepository = new(paths);
    TestHttpMessageHandler handler = new(_ =>
        JsonResponse(
            """
            {
              "id": "todo-task-2",
              "title": "File the receipt",
              "status": "notStarted",
              "importance": "normal",
              "lastModifiedDateTime": "2026-06-01T22:14:00Z",
              "webLink": "https://to-do.office.com/tasks/todo-task-2",
              "@odata.etag": "W/\"etag-2\""
            }
            """,
            HttpStatusCode.Created));
    MicrosoftToDoGraphTaskAdapter adapter = new(
        new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.test/v1.0/")
        },
        new TestMicrosoftToDoAccessTokenProvider("token-123"),
        () => now);
    MicrosoftToDoTaskPushService service = new(adapter, sourceLinkRepository);

    MicrosoftToDoPushResult result = await service.PushCapturedTaskAsync(
        task,
        "list-2",
        CancellationToken.None);

    Assert.True(result.WasPushed, "Push service should report successful Graph creation.");

    IReadOnlyList<SourceLink> links = await sourceLinkRepository.GetForLocalItemAsync(
        SourceLinkItemType.Task,
        task.Id,
        CancellationToken.None);
    Assert.Equal(1, links.Count);
    SourceLink savedLink = links[0];
    Assert.Equal(SourceSystem.MicrosoftToDo, savedLink.SourceSystem);
    Assert.Equal("todo-task-2", savedLink.ExternalId);
    Assert.Equal("list-2", savedLink.ExternalContainerId);
    Assert.Equal("File the receipt", savedLink.ExternalDisplayName);
    Assert.Equal("W/\"etag-2\"", savedLink.SourceVersion);
    Assert.Equal(SyncState.Synced, savedLink.SyncState);
    Assert.Equal(now, savedLink.LastSyncedAt);

    SourceLink? reloadedById = await sourceLinkRepository.GetByIdAsync(
        savedLink.Id,
        CancellationToken.None);
    Assert.NotNull(reloadedById, "Saved source link should reload by id.");
    Assert.Equal(savedLink.ExternalId, reloadedById!.ExternalId);
}

static async Task MicrosoftProjectDesktopDetectorFindsClickToRunProject()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    string programFilesRoot = Path.Combine(workspace.Path, "Program Files");
    string executablePath = Path.Combine(
        programFilesRoot,
        "Microsoft Office",
        "root",
        "Office16",
        MicrosoftProjectDesktopDetectionResult.ExecutableName);
    Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
    await File.WriteAllTextAsync(executablePath, "fake project executable", CancellationToken.None);
    MicrosoftProjectDesktopDetector detector = new(
        [programFilesRoot],
        pathDirectories: []);

    MicrosoftProjectDesktopDetectionResult result =
        await detector.DetectAsync(CancellationToken.None);

    Assert.True(result.IsDetected, "Detector should find Project in the Office Click-to-Run path.");
    Assert.Equal(executablePath, result.ExecutablePath);
    Assert.Equal("Microsoft Project Desktop", result.DisplayName);
    Assert.Equal("Project detected", result.StatusText);
    Assert.Contains(MicrosoftProjectDesktopDetectionResult.ExecutableName, result.DetailText);
    Assert.Contains(executablePath, result.SearchedPaths);
}

static async Task MicrosoftProjectDesktopDetectorFindsProjectOnPath()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    string pathDirectory = Path.Combine(workspace.Path, "ProjectBin");
    string executablePath = Path.Combine(
        pathDirectory,
        MicrosoftProjectDesktopDetectionResult.ExecutableName);
    Directory.CreateDirectory(pathDirectory);
    await File.WriteAllTextAsync(executablePath, "fake project executable", CancellationToken.None);
    MicrosoftProjectDesktopDetector detector = new(
        programFilesRoots: [],
        pathDirectories: [pathDirectory]);

    MicrosoftProjectDesktopDetectionResult result =
        await detector.DetectAsync(CancellationToken.None);

    Assert.True(result.IsDetected, "Detector should find Project when WINPROJ.EXE is on PATH.");
    Assert.Equal(executablePath, result.ExecutablePath);
    Assert.Contains(executablePath, result.SearchedPaths);
}

static async Task MicrosoftProjectDesktopDetectorReportsProjectNotFound()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    string programFilesRoot = Path.Combine(workspace.Path, "Program Files");
    MicrosoftProjectDesktopDetector detector = new(
        [programFilesRoot],
        pathDirectories: []);

    MicrosoftProjectDesktopDetectionResult result =
        await detector.DetectAsync(CancellationToken.None);

    Assert.False(result.IsDetected, "Detector should report not found when no Project executable exists.");
    Assert.Equal(string.Empty, result.ExecutablePath);
    Assert.Equal("Project not found", result.StatusText);
    Assert.Contains("common Microsoft Office install paths", result.DetailText);
    Assert.True(
        result.SearchedPaths.Any(path => path.EndsWith(
            Path.Combine("Microsoft Office", "root", "Office16", MicrosoftProjectDesktopDetectionResult.ExecutableName),
            StringComparison.OrdinalIgnoreCase)),
        "Detector should search the modern Microsoft 365 Office16 Click-to-Run path.");
}

static async Task MicrosoftProjectTaskImportServiceImportsSelectedProjectTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    string projectFilePath = Path.Combine(workspace.Path, "Plans", "Personal Success Plan.mpp");
    Directory.CreateDirectory(Path.GetDirectoryName(projectFilePath)!);
    await File.WriteAllTextAsync(projectFilePath, "fake project file", CancellationToken.None);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.ProjectDesktop.LocalProjectFilePath = projectFilePath;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    SourceLinkRepository sourceLinkRepository = new(paths);
    DateTimeOffset importedAt = new(2026, 6, 2, 10, 30, 0, TimeSpan.Zero);
    TestMicrosoftProjectAutomationAdapter adapter = new(
        new MicrosoftProjectImportedTask(
            "42",
            "Frame the 20-minute plan",
            new DateTimeOffset(2026, 6, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 6, 4, 17, 0, 0, TimeSpan.Zero),
            25,
            "Block the smallest useful slice.",
            durationMinutes: 20,
            outlineLevel: 3,
            isCritical: true,
            projectPriority: 900),
        new MicrosoftProjectImportedTask(
            "43",
            "Review the next success action",
            finishAt: new DateTimeOffset(2026, 6, 5, 17, 0, 0, TimeSpan.Zero),
            percentComplete: 100,
            durationMinutes: 0,
            outlineLevel: 2,
            isMilestone: true,
            projectPriority: 500));
    MicrosoftProjectTaskImportService service = new(
        settingsService,
        taskRepository,
        sourceLinkRepository,
        adapter,
        nowProvider: () => importedAt);

    MicrosoftProjectImportResult result =
        await service.ImportSelectedProjectFileAsync(CancellationToken.None);

    Assert.True(result.WasSuccessful, "Project import should report success.");
    Assert.Equal("Project tasks imported", result.StatusText);
    Assert.Contains("Imported 2 Project tasks", result.DetailText);
    Assert.Equal(2, result.ImportedCount);
    Assert.Equal(1, adapter.CallCount);
    Assert.Equal(projectFilePath, adapter.LastProjectFilePath);

    IReadOnlyList<TaskItem> localTasks = await taskRepository.GetAllAsync(CancellationToken.None);
    Assert.Equal(2, localTasks.Count);

    TaskItem plannedTask = localTasks.Single(task => task.Title == "Frame the 20-minute plan");
    Assert.Equal(TaskItemStatus.InProgress, plannedTask.Status);
    Assert.Equal(new DateOnly(2026, 6, 1), plannedTask.StartDate);
    Assert.Equal(new DateOnly(2026, 6, 4), plannedTask.DueDate);
    Assert.Contains("Imported from Microsoft Project: Personal Success Plan.mpp", plannedTask.Notes);
    Assert.Contains("Project Task Id: 42", plannedTask.Notes);
    Assert.Contains("Project outline level: 3", plannedTask.Notes);
    Assert.Contains("Project duration: 20 minutes", plannedTask.Notes);
    Assert.Contains("Project priority: 900", plannedTask.Notes);
    Assert.Contains("Project percent complete: 25%", plannedTask.Notes);
    Assert.Contains("Block the smallest useful slice.", plannedTask.Notes);
    Assert.Equal(TaskPriority.Critical, plannedTask.Priority);
    Assert.Equal(20, plannedTask.EstimatedMinutes);
    Assert.True(plannedTask.IsTinyStep, "A 20-minute Project task should map to a tiny local step.");
    Assert.Contains("Microsoft Project", plannedTask.Tags);
    Assert.Contains("Project Import", plannedTask.Tags);
    Assert.Contains("Critical Path", plannedTask.Tags);
    Assert.Contains(plannedTask.Id, result.LocalTaskIds);

    IReadOnlyList<SourceLink> plannedTaskLinks = await sourceLinkRepository.GetForLocalItemAsync(
        SourceLinkItemType.Task,
        plannedTask.Id,
        CancellationToken.None);
    Assert.Equal(1, plannedTaskLinks.Count);
    SourceLink plannedTaskLink = plannedTaskLinks[0];
    Assert.Equal(SourceSystem.MicrosoftProjectDesktop, plannedTaskLink.SourceSystem);
    Assert.Equal("42", plannedTaskLink.ExternalId);
    Assert.Equal(projectFilePath, plannedTaskLink.ExternalContainerId);
    Assert.Equal("Frame the 20-minute plan", plannedTaskLink.ExternalDisplayName);
    Assert.Equal(SyncState.Synced, plannedTaskLink.SyncState);
    Assert.True(plannedTaskLink.IsReadOnly, "Project desktop imports should be tracked as read-only source links.");
    Assert.True(plannedTaskLink.LastSyncedAt.HasValue, "Project source link should record the import sync time.");
    Assert.Equal(importedAt, plannedTaskLink.LastSyncedAt!.Value);
    Assert.Contains("percent=25", plannedTaskLink.SourceVersion);
    Assert.Contains(plannedTaskLink.Id, result.SourceLinkIds);

    TaskItem milestoneTask = localTasks.Single(task => task.Title == "Review the next success action");
    Assert.Equal(TaskItemStatus.Done, milestoneTask.Status);
    Assert.Equal(new DateOnly(2026, 6, 5), milestoneTask.DueDate);
    Assert.True(milestoneTask.CompletedAt.HasValue, "A 100 percent Project task should map to a done local task.");
    Assert.Contains("Project task type: Milestone", milestoneTask.Notes);
    Assert.Contains("Milestone", milestoneTask.Tags);
    Assert.Contains(milestoneTask.Id, result.LocalTaskIds);

    IReadOnlyList<SourceLink> milestoneTaskLinks = await sourceLinkRepository.GetForLocalItemAsync(
        SourceLinkItemType.Task,
        milestoneTask.Id,
        CancellationToken.None);
    Assert.Equal(1, milestoneTaskLinks.Count);
    Assert.Equal("43", milestoneTaskLinks[0].ExternalId);
    Assert.Contains(milestoneTaskLinks[0].Id, result.SourceLinkIds);
}

static async Task MicrosoftProjectTaskImportServiceReportsDisabledProjectImport()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    string projectFilePath = Path.Combine(workspace.Path, "Plans", "Personal Success Plan.mpp");
    Directory.CreateDirectory(Path.GetDirectoryName(projectFilePath)!);
    await File.WriteAllTextAsync(projectFilePath, "fake project file", CancellationToken.None);

    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.Connections.EnableProjectDesktop = false;
    settings.ProjectDesktop.LocalProjectFilePath = projectFilePath;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    TestMicrosoftProjectAutomationAdapter adapter = new(
        new MicrosoftProjectImportedTask("1", "Should not import"));
    MicrosoftProjectTaskImportService service = new(settingsService, taskRepository, adapter);

    MicrosoftProjectImportResult result =
        await service.ImportSelectedProjectFileAsync(CancellationToken.None);

    Assert.False(result.WasSuccessful, "Disabled Project import should report a recoverable failure.");
    Assert.Equal("Project import off", result.StatusText);
    Assert.Contains("Turn on Project Desktop", result.DetailText);
    Assert.Equal(0, adapter.CallCount);
    Assert.Equal(0, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task MicrosoftProjectTaskImportServiceReportsMissingProjectFile()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    string projectFilePath = Path.Combine(workspace.Path, "Plans", "Missing Plan.mpp");
    SettingsService settingsService = new(paths);
    AppSettings settings = AppSettings.CreateDefault();
    settings.ProjectDesktop.LocalProjectFilePath = projectFilePath;
    await settingsService.SaveAsync(settings, CancellationToken.None);

    TaskRepository taskRepository = new(paths);
    TestMicrosoftProjectAutomationAdapter adapter = new(
        new MicrosoftProjectImportedTask("1", "Should not import"));
    MicrosoftProjectTaskImportService service = new(settingsService, taskRepository, adapter);

    MicrosoftProjectImportResult result =
        await service.ImportSelectedProjectFileAsync(CancellationToken.None);

    Assert.False(result.WasSuccessful, "Missing Project file should report a recoverable failure.");
    Assert.Equal(projectFilePath, result.ProjectFilePath);
    Assert.Equal("Project file not found", result.StatusText);
    Assert.Contains("Select an existing Microsoft Project file", result.DetailText);
    Assert.Equal(0, adapter.CallCount);
    Assert.Equal(0, (await taskRepository.GetAllAsync(CancellationToken.None)).Count);
}

static async Task DatabaseServiceReportsHealthyDatabase()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    DatabaseHealthCheckResult health = await database.CheckHealthAsync(CancellationToken.None);
    await database.HealthCheckAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.True(health.IsHealthy, "Migrated database should report healthy.");
    Assert.Equal("Local database is healthy.", health.Summary);
    Assert.Equal("ok", health.QuickCheckResult);
    Assert.Equal(3, health.AppliedMigrationCount);
    Assert.Equal(3, health.LatestAppliedMigration);
    Assert.Equal(3, health.RequiredMigrationCount);
    Assert.Equal(3, health.LatestRequiredMigration);
    Assert.Equal(0, health.Findings.Count);
}

static async Task DatabaseServiceReportsMissingMigrationHealthFailures()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);

    await database.OpenAsync(CancellationToken.None);
    DatabaseHealthCheckResult health = await database.CheckHealthAsync(CancellationToken.None);
    InvalidOperationException failure = await Assert.ThrowsAsync<InvalidOperationException>(
        () => database.HealthCheckAsync(CancellationToken.None));
    await database.CloseAsync(CancellationToken.None);

    Assert.False(health.IsHealthy, "Database should report unhealthy before migrations run.");
    Assert.Equal("Local database needs attention.", health.Summary);
    Assert.Contains("Database migration 1 is missing.", health.Findings);
    Assert.Contains("Required table 'tasks' is missing.", health.Findings);
    Assert.Contains("Local database needs attention.", failure.Message);
}

static async Task DatabaseStartupMigrationServiceMigratesNewDatabaseAtStartup()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService database = new(paths);
    DatabaseStartupMigrationService startupMigration = new(database);

    DatabaseStartupMigrationResult result = await startupMigration.RunAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.True(result.Health.IsHealthy, "Startup migration should leave the database healthy.");
    Assert.True(result.Migration.AppliedMigrations, "New database should apply startup migrations.");
    Assert.Equal(3, result.Migration.AppliedCountThisRun);
    Assert.Equal(3, result.Migration.TotalAppliedCount);
    Assert.Equal(3, result.Migration.LatestAppliedVersion);
    Assert.Equal("Ready - Data Updated", result.StatusText);
    Assert.True(await TableExistsAsync(paths.DatabasePath, "tasks"), "Startup migration should create task storage.");
    Assert.True(await TableExistsAsync(paths.DatabasePath, "settings_metadata"), "Startup migration should create settings metadata storage.");
}

static async Task DatabaseStartupMigrationServicePreservesDataOnRestart()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    DatabaseService firstDatabase = new(paths);
    DatabaseStartupMigrationService firstStartupMigration = new(firstDatabase);

    DatabaseStartupMigrationResult firstResult = await firstStartupMigration.RunAsync(CancellationToken.None);
    await firstDatabase.CloseAsync(CancellationToken.None);

    TaskRepository repository = new(paths);
    TaskItem task = TaskItem.Capture("Keep this after restart");
    await repository.AddAsync(task, CancellationToken.None);

    DatabaseService secondDatabase = new(paths);
    DatabaseStartupMigrationService secondStartupMigration = new(secondDatabase);
    DatabaseStartupMigrationResult secondResult = await secondStartupMigration.RunAsync(CancellationToken.None);

    TaskItem? loaded = await repository.GetByIdAsync(task.Id, CancellationToken.None);
    await secondDatabase.CloseAsync(CancellationToken.None);

    Assert.Equal(3, firstResult.Migration.AppliedCountThisRun);
    Assert.Equal(0, secondResult.Migration.AppliedCountThisRun);
    Assert.Equal("Ready - Data OK", secondResult.StatusText);
    Assert.NotNull(loaded, "Task should remain after startup migration on restart.");
    Assert.Equal("Keep this after restart", loaded!.Title);
}

static async Task AppBootstrapperShowsSimpleDatabaseFailureMessage()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    Directory.CreateDirectory(paths.AppDataDirectory);
    await File.WriteAllTextAsync(paths.DatabasePath, "not a Success Planner database");

    AppBootstrapper bootstrapper = new(paths);
    BootstrapResult result = await bootstrapper.StartAsync(CancellationToken.None);

    Assert.False(result.Success, "Bootstrap should fail for an invalid local data file.");
    Assert.Null(result.MainWindow, "Failed bootstrap should not create a main window.");
    Assert.Contains("could not start", result.UserMessage);
}

static async Task TaskRepositorySavesAndLoadsTaskState()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository repository = new(paths);
    TaskItem task = TaskItem.Capture("  Plan next tiny step  ");
    task.UpdateNotes("Keep it small.");
    task.Schedule(new DateOnly(2026, 6, 1), new DateOnly(2026, 5, 31));
    task.SetPriority(TaskPriority.High);
    task.SetEstimate(20);
    task.SetEnergyLevel("Low");
    task.MarkTinyStep();
    task.MarkPhysicalActivity();
    task.AddTag("Focus");

    await repository.AddAsync(task, CancellationToken.None);

    TaskItem? loaded = await repository.GetByIdAsync(task.Id, CancellationToken.None);
    Assert.NotNull(loaded, "Saved task should load by id.");
    Assert.Equal(task.Id, loaded!.Id);
    Assert.Equal("Plan next tiny step", loaded.Title);
    Assert.Equal("Keep it small.", loaded.Notes);
    Assert.Equal(TaskItemStatus.Planned, loaded.Status);
    Assert.Equal(TaskPriority.High, loaded.Priority);
    Assert.Equal(new DateOnly(2026, 6, 1), loaded.DueDate);
    Assert.Equal(new DateOnly(2026, 5, 31), loaded.StartDate);
    Assert.Equal(20, loaded.EstimatedMinutes);
    Assert.Equal("Low", loaded.EnergyLevel);
    Assert.True(loaded.IsTinyStep, "Tiny-step flag should round-trip.");
    Assert.True(loaded.IsPhysicalActivity, "Physical-activity flag should round-trip.");
    Assert.Contains("Move", loaded.Tags);
    Assert.Contains("Focus", loaded.Tags);

    task.Start();
    task.Complete(new DateTimeOffset(2026, 6, 1, 15, 0, 0, TimeSpan.Zero));
    task.UpdateNotes("Finished with a small win.");
    await repository.SaveAsync(task, CancellationToken.None);

    TaskItem? updated = await repository.GetByIdAsync(task.Id, CancellationToken.None);
    Assert.NotNull(updated, "Updated task should load by id.");
    Assert.Equal(TaskItemStatus.Done, updated!.Status);
    Assert.Equal("Finished with a small win.", updated.Notes);
    Assert.True(updated.CompletedAt.HasValue, "Completed time should round-trip.");

    IReadOnlyList<TaskItem> allTasks = await repository.GetAllAsync(CancellationToken.None);
    Assert.Equal(1, allTasks.Count);
}

static async Task TaskRepositoryDeletesTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository repository = new(paths);
    TaskItem task = TaskItem.Capture("Delete me");
    await repository.AddAsync(task, CancellationToken.None);

    await repository.DeleteAsync(task.Id, CancellationToken.None);

    TaskItem? deleted = await repository.GetByIdAsync(task.Id, CancellationToken.None);
    Assert.Null(deleted, "Deleted task should not load by id.");
}

static async Task TaskRepositoryLoadsUnplannedTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskItem highCapture = CreateRepositoryTask("High loose capture", priority: TaskPriority.High);
    TaskItem normalCapture = CreateRepositoryTask("Normal loose capture");
    TaskItem plannedTask = CreateRepositoryTask("Dated task", dueDate: today);
    TaskItem inProgressTask = CreateRepositoryTask("Started task", inProgress: true);
    TaskItem doneTask = CreateRepositoryTask("Done task", done: true);
    TaskItem assignedProject = CreateRepositoryTask("Assigned project");
    Guid projectId = Guid.NewGuid();
    await InsertProjectRowAsync(paths, projectId, "Existing project");
    assignedProject.AssignProject(projectId);

    TaskRepository repository = new(paths);
    TaskItem[] tasks = [normalCapture, plannedTask, doneTask, highCapture, assignedProject, inProgressTask];
    foreach (TaskItem task in tasks)
    {
        await repository.AddAsync(task, CancellationToken.None);
    }

    IReadOnlyList<TaskItem> unplannedTasks = await repository.GetUnplannedAsync(CancellationToken.None);

    Assert.Equal(2, unplannedTasks.Count);
    Assert.Equal("High loose capture", unplannedTasks[0].Title);
    Assert.Equal("Normal loose capture", unplannedTasks[1].Title);
    Assert.False(unplannedTasks.Any(task => task.Title == "Dated task"), "Planned dated tasks should not load into Plan inbox.");
    Assert.False(unplannedTasks.Any(task => task.Title == "Started task"), "In-progress tasks should not load into Plan inbox.");
    Assert.False(unplannedTasks.Any(task => task.Title == "Done task"), "Completed tasks should not load into Plan inbox.");
    Assert.False(unplannedTasks.Any(task => task.Title == "Assigned project"), "Project-assigned captures should not load into Plan inbox.");
}

static async Task TaskRepositoryLoadsTodayTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskItem overdue = CreateRepositoryTask("Call the pharmacy", dueDate: today.AddDays(-1));
    TaskItem dueToday = CreateRepositoryTask("Pay the bill", dueDate: today, priority: TaskPriority.High);
    TaskItem selectedToday = CreateRepositoryTask("Draft next plan", dueDate: today.AddDays(5), startDate: today);
    TaskItem inProgress = CreateRepositoryTask("Finish active task", inProgress: true);
    TaskItem future = CreateRepositoryTask("Future task", dueDate: today.AddDays(1));
    TaskItem doneToday = CreateRepositoryTask("Already done", dueDate: today, done: true);
    TaskItem looseCapture = CreateRepositoryTask("Loose capture");

    TaskRepository repository = new(paths);
    TaskItem[] tasks = [future, selectedToday, doneToday, dueToday, inProgress, overdue, looseCapture];
    foreach (TaskItem task in tasks)
    {
        await repository.AddAsync(task, CancellationToken.None);
    }

    IReadOnlyList<TaskItem> todayTasks = await repository.GetTodayAsync(today, CancellationToken.None);

    Assert.Equal(4, todayTasks.Count);
    Assert.Equal("Call the pharmacy", todayTasks[0].Title);
    Assert.Equal("Pay the bill", todayTasks[1].Title);
    Assert.Equal("Draft next plan", todayTasks[2].Title);
    Assert.Equal("Finish active task", todayTasks[3].Title);
    Assert.False(todayTasks.Any(task => task.Title == "Future task"), "Future tasks should not load into Today.");
    Assert.False(todayTasks.Any(task => task.Title == "Already done"), "Completed tasks should not load into Today.");
    Assert.False(todayTasks.Any(task => task.Title == "Loose capture"), "Undated captured tasks should not load into Today.");
}

static async Task TaskRepositoryLoadsRecentActiveTasks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskItem dueToday = CreateRepositoryTask("Pay the bill", dueDate: today, priority: TaskPriority.High);
    TaskItem selectedYesterday = CreateRepositoryTask("Review notes", dueDate: today.AddDays(5), startDate: today.AddDays(-1));
    TaskItem inProgress = CreateRepositoryTask("Finish active task", inProgress: true);
    TaskItem blocked = CreateRepositoryTask("Resolve blocked item");
    blocked.MarkBlocked();
    TaskItem oldOverdue = CreateRepositoryTask("Old stale task", dueDate: today.AddDays(-15));
    TaskItem future = CreateRepositoryTask("Future task", dueDate: today.AddDays(1));
    TaskItem doneToday = CreateRepositoryTask("Already done", dueDate: today, done: true);
    TaskItem looseCapture = CreateRepositoryTask("Loose capture");

    TaskRepository repository = new(paths);
    TaskItem[] tasks = [future, selectedYesterday, doneToday, dueToday, inProgress, oldOverdue, blocked, looseCapture];
    foreach (TaskItem task in tasks)
    {
        await repository.AddAsync(task, CancellationToken.None);
    }

    IReadOnlyList<TaskItem> recentActiveTasks = await repository.GetRecentActiveAsync(today, CancellationToken.None);

    Assert.Equal(4, recentActiveTasks.Count);
    Assert.Equal("Finish active task", recentActiveTasks[0].Title);
    Assert.Equal("Pay the bill", recentActiveTasks[1].Title);
    Assert.Equal("Review notes", recentActiveTasks[2].Title);
    Assert.Equal("Resolve blocked item", recentActiveTasks[3].Title);
    Assert.False(recentActiveTasks.Any(task => task.Title == "Old stale task"), "Older inactive tasks should not load into Done.");
    Assert.False(recentActiveTasks.Any(task => task.Title == "Future task"), "Future tasks should not load into Done.");
    Assert.False(recentActiveTasks.Any(task => task.Title == "Already done"), "Completed tasks should not load into Done.");
    Assert.False(recentActiveTasks.Any(task => task.Title == "Loose capture"), "Undated captured tasks should not load into Done.");
}

static async Task TodayViewModelSavesTaskActionsThroughTaskRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskItem startTask = CreateRepositoryTask("Start the sketch", dueDate: today);
    TaskItem doneTask = CreateRepositoryTask("Finish the sketch", dueDate: today);
    TaskItem snoozeTask = CreateRepositoryTask("Call the printer", dueDate: today);
    TaskItem noteTask = CreateRepositoryTask("Choose calmer color set", dueDate: today);
    noteTask.UpdateNotes("Use the calmer color set.");

    TaskRepository repository = new(paths);
    foreach (TaskItem task in new[] { startTask, doneTask, snoozeTask, noteTask })
    {
        await repository.AddAsync(task, CancellationToken.None);
    }

    TodayViewModel viewModel = new(repository.GetTodayAsync, repository.SaveAsync, () => today);
    await viewModel.LoadTasksAsync(CancellationToken.None);

    await viewModel.ExecuteTaskActionAsync(
        viewModel.TaskCards.First(card => card.Id == startTask.Id),
        TodayTaskAction.Start,
        CancellationToken.None);
    await viewModel.ExecuteTaskActionAsync(
        viewModel.TaskCards.First(card => card.Id == doneTask.Id),
        TodayTaskAction.Done,
        CancellationToken.None);
    await viewModel.ExecuteTaskActionAsync(
        viewModel.TaskCards.First(card => card.Id == snoozeTask.Id),
        TodayTaskAction.Snooze,
        CancellationToken.None);
    await viewModel.ExecuteTaskActionAsync(
        viewModel.TaskCards.First(card => card.Id == noteTask.Id),
        TodayTaskAction.Note,
        CancellationToken.None);
    viewModel.NoteDraft = "Remember the calmer color set.";
    await viewModel.SaveSelectedNoteAsync(CancellationToken.None);

    TaskItem? savedStart = await repository.GetByIdAsync(startTask.Id, CancellationToken.None);
    TaskItem? savedDone = await repository.GetByIdAsync(doneTask.Id, CancellationToken.None);
    TaskItem? savedSnooze = await repository.GetByIdAsync(snoozeTask.Id, CancellationToken.None);
    TaskItem? savedNote = await repository.GetByIdAsync(noteTask.Id, CancellationToken.None);
    IReadOnlyList<TaskItem> todayTasks = await repository.GetTodayAsync(today, CancellationToken.None);

    Assert.NotNull(savedStart, "Started task should remain in SQLite.");
    Assert.Equal(TaskItemStatus.InProgress, savedStart!.Status);
    Assert.NotNull(savedDone, "Done task should remain in SQLite.");
    Assert.Equal(TaskItemStatus.Done, savedDone!.Status);
    Assert.True(savedDone.CompletedAt.HasValue, "Done task should persist a completed time.");
    Assert.NotNull(savedSnooze, "Snoozed task should remain in SQLite.");
    Assert.Equal(today.AddDays(1), savedSnooze!.DueDate);
    Assert.Equal(today.AddDays(1), savedSnooze.StartDate);
    Assert.NotNull(savedNote, "Note task should remain in SQLite.");
    Assert.Equal("Remember the calmer color set.", savedNote!.Notes);
    Assert.True(todayTasks.Any(task => task.Id == startTask.Id), "Started task should still appear in Today.");
    Assert.True(todayTasks.Any(task => task.Id == noteTask.Id), "Note task should still appear in Today.");
    Assert.False(todayTasks.Any(task => task.Id == doneTask.Id), "Done task should leave Today.");
    Assert.False(todayTasks.Any(task => task.Id == snoozeTask.Id), "Snoozed task should leave Today.");
}

static async Task DoneViewModelCompletesSelectedTaskThroughTaskRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskItem completedTask = CreateRepositoryTask("Finish active task", inProgress: true);
    TaskItem remainingTask = CreateRepositoryTask("Pay the bill", dueDate: today);

    TaskRepository repository = new(paths);
    NoteRepository noteRepository = new(paths);
    foreach (TaskItem task in new[] { completedTask, remainingTask })
    {
        await repository.AddAsync(task, CancellationToken.None);
    }

    DoneViewModel viewModel = new(
        repository.GetRecentActiveAsync,
        repository.SaveAsync,
        noteRepository.AddTaskSmallWinAsync,
        () => today);
    await viewModel.LoadTasksAsync(CancellationToken.None);

    await viewModel.CompleteSelectedTaskAsync(
        viewModel.TaskCards.First(card => card.Id == completedTask.Id),
        CancellationToken.None);

    TaskItem? savedCompletedTask = await repository.GetByIdAsync(completedTask.Id, CancellationToken.None);
    IReadOnlyList<TaskItem> recentActiveTasks = await repository.GetRecentActiveAsync(today, CancellationToken.None);
    IReadOnlyList<NoteItem> taskNotes = await noteRepository.GetForOwnerAsync(
        NoteOwnerType.Task,
        completedTask.Id,
        CancellationToken.None);
    IReadOnlyList<NoteItem> reviewHighlights = await noteRepository.GetReviewHighlightsAsync(CancellationToken.None);

    Assert.NotNull(savedCompletedTask, "Completed task should remain in SQLite.");
    Assert.Equal(TaskItemStatus.Done, savedCompletedTask!.Status);
    Assert.True(savedCompletedTask.CompletedAt.HasValue, "Completed task should persist a completed time.");
    Assert.False(recentActiveTasks.Any(task => task.Id == completedTask.Id), "Completed task should leave Done recent active tasks.");
    Assert.True(recentActiveTasks.Any(task => task.Id == remainingTask.Id), "Remaining recent active task should still load.");
    Assert.Equal(1, taskNotes.Count);
    Assert.Equal("Small win: Finish active task", taskNotes[0].Text);
    Assert.Equal(NoteOwnerType.Task, taskNotes[0].OwnerType);
    Assert.Equal(completedTask.Id, taskNotes[0].OwnerId.GetValueOrDefault());
    Assert.True(taskNotes[0].IsReviewHighlight, "Small win note should be marked for Review.");
    Assert.Contains("Review", taskNotes[0].Tags);
    Assert.Contains("Win", taskNotes[0].Tags);
    Assert.Contains("Small Win", taskNotes[0].Tags);
    Assert.Equal(taskNotes[0].Id, viewModel.LastSmallWinNoteId.GetValueOrDefault());
    Assert.Equal("Small win: Finish active task", viewModel.LastSmallWinText);
    Assert.True(reviewHighlights.Any(note => note.Id == taskNotes[0].Id), "Small win should appear in review highlights.");

    await noteRepository.AddTaskSmallWinAsync(savedCompletedTask, CancellationToken.None);
    IReadOnlyList<NoteItem> taskNotesAfterSecondRecord = await noteRepository.GetForOwnerAsync(
        NoteOwnerType.Task,
        completedTask.Id,
        CancellationToken.None);
    Assert.Equal(1, taskNotesAfterSecondRecord.Count);
}

static async Task CaptureViewModelSavesCapturedTasksThroughTaskRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository repository = new(paths);
    CaptureViewModel viewModel = new(repository.AddAsync)
    {
        TaskTitle = "  Capture a local task  "
    };
    viewModel.TomorrowDateCommand.Execute(null);
    viewModel.MicrosoftToDoDestinationCommand.Execute(null);

    await viewModel.SaveCapturedTaskAsync(CancellationToken.None);

    Assert.True(viewModel.HasSavedTask, "Capture should mark the task saved after repository write.");
    Assert.True(viewModel.LastSavedTaskId.HasValue, "Capture should expose the saved task id.");
    Guid savedTaskId = viewModel.LastSavedTaskId.GetValueOrDefault();

    DatabaseService restartDatabase = new(paths);
    DatabaseStartupMigrationService restartMigration = new(restartDatabase);
    await restartMigration.RunAsync(CancellationToken.None);

    TaskRepository restartedRepository = new(paths);
    TaskItem? savedTask = await restartedRepository.GetByIdAsync(savedTaskId, CancellationToken.None);
    await restartDatabase.CloseAsync(CancellationToken.None);

    Assert.NotNull(savedTask, "Saved capture task should load from SQLite.");
    Assert.Equal("Capture a local task", savedTask!.Title);
    Assert.Equal(DateOnly.FromDateTime(DateTime.Today).AddDays(1), savedTask.DueDate);
    Assert.Equal(TaskItemStatus.Planned, savedTask.Status);
    Assert.Equal(CaptureDestinationPreference.MicrosoftToDo, viewModel.SelectedDestination);
}

static async Task PlanViewModelSavesPlanningChangesThroughTaskRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskRepository repository = new(paths);
    TaskItem original = CreateRepositoryTask("Plan the local workflow");
    original.UpdateNotes("Keep the planning step small.");
    await repository.AddAsync(original, CancellationToken.None);

    PlanViewModel viewModel = new(
        repository.GetUnplannedAsync,
        repository.SaveAsync,
        () => today);

    await viewModel.LoadInboxAsync(CancellationToken.None);
    viewModel.SelectInboxItem(viewModel.InboxItems[0]);
    viewModel.ChooseHighPriorityCommand.Execute(null);
    viewModel.TodayDateCommand.Execute(null);
    viewModel.ProjectName = "Success Planner MCP";
    viewModel.MinimumWinDraft = "Save one local plan";
    viewModel.SplitIntoTinyStepsCommand.Execute(null);

    await viewModel.SavePlanAsync(CancellationToken.None);

    Assert.True(viewModel.HasSavedPlan, "Plan should expose the saved task id after repository write.");
    Assert.Equal(original.Id, viewModel.LastSavedTaskId.GetValueOrDefault());
    Assert.Equal(3, viewModel.SavedTinyStepIds.Count);

    TaskItem? savedOriginal = await repository.GetByIdAsync(original.Id, CancellationToken.None);
    Assert.NotNull(savedOriginal, "Original task should still exist after planning.");
    Assert.Equal(TaskItemStatus.Planned, savedOriginal!.Status);
    Assert.Equal(TaskPriority.High, savedOriginal.Priority);
    Assert.Equal(today, savedOriginal.DueDate);
    Assert.Equal(today, savedOriginal.StartDate);
    Assert.Contains("Keep the planning step small.", savedOriginal.Notes);
    Assert.Contains("Minimum Win: Save one local plan", savedOriginal.Notes);
    Assert.Contains("Project: Success Planner MCP", savedOriginal.Notes);
    Assert.Contains("Tiny Steps:", savedOriginal.Notes);

    IReadOnlyList<TaskItem> unplannedTasks = await repository.GetUnplannedAsync(CancellationToken.None);
    Assert.False(unplannedTasks.Any(task => task.Id == original.Id), "Saved planned task should leave the unplanned query.");

    IReadOnlyList<TaskItem> allTasks = await repository.GetAllAsync(CancellationToken.None);
    IReadOnlyList<TaskItem> savedTinySteps = allTasks
        .Where(task => viewModel.SavedTinyStepIds.Contains(task.Id))
        .OrderBy(task => task.Title, StringComparer.OrdinalIgnoreCase)
        .ToList();
    Assert.Equal(3, savedTinySteps.Count);
    Assert.True(savedTinySteps.All(task => task.IsTinyStep), "Tiny steps should be stored as task records.");
    Assert.True(savedTinySteps.All(task => task.Status == TaskItemStatus.Planned), "Tiny steps should persist planned state.");
    Assert.True(savedTinySteps.All(task => task.Notes.Contains("Split from: Plan the local workflow", StringComparison.Ordinal)), "Tiny steps should keep the source context.");
}

static async Task ReviewViewModelLoadsSmallWinsThroughNoteRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository taskRepository = new(paths);
    NoteRepository noteRepository = new(paths);
    TaskItem completedTask = CreateRepositoryTask("Finish active task", inProgress: true);
    completedTask.Complete(new DateTimeOffset(2026, 5, 30, 14, 0, 0, TimeSpan.Zero));
    await taskRepository.AddAsync(completedTask, CancellationToken.None);
    NoteItem smallWin = await noteRepository.AddTaskSmallWinAsync(completedTask, CancellationToken.None);
    NoteItem regularNote = NoteItem.Create(NoteOwnerType.Task, completedTask.Id, "Private task note.");
    await noteRepository.AddAsync(regularNote, CancellationToken.None);

    ReviewViewModel viewModel = new(noteRepository.GetReviewHighlightsAsync);

    await viewModel.LoadReviewAsync(CancellationToken.None);

    Assert.True(viewModel.HasSmallWins, "Review should load small wins from note review highlights.");
    Assert.True(viewModel.HasReviewData, "Review should expose loaded review data.");
    Assert.Equal(1, viewModel.SmallWins.Count);
    Assert.Equal(smallWin.Id, viewModel.SmallWins[0].Id);
    Assert.Equal("Small win: Finish active task", viewModel.SmallWins[0].Text);
    Assert.Equal("Task win", viewModel.SmallWins[0].SourceText);
    Assert.Equal("1 review item", viewModel.ReviewCountText);
    Assert.Equal("1 small win this review.", viewModel.WeekSummaryText);
    Assert.Equal("1 small win ready.", viewModel.SmallWinsText);
    Assert.Equal("Small wins ready.", viewModel.StatusText);
}

static async Task ReviewViewModelLoadsStuckItemsThroughTaskRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskRepository taskRepository = new(paths);
    TaskItem blockedTask = CreateRepositoryTask("Call the supplier", dueDate: today.AddDays(-1), priority: TaskPriority.High);
    blockedTask.UpdateNotes("Waiting on a return call.");
    blockedTask.MarkBlocked();
    TaskItem repeatedSnooze = CreateRepositoryTask("Review insurance paperwork", dueDate: today.AddDays(2));
    repeatedSnooze.AddTag("Repeated Snooze");
    TaskItem ordinaryTask = CreateRepositoryTask("Pay the bill", dueDate: today);

    foreach (TaskItem task in new[] { ordinaryTask, repeatedSnooze, blockedTask })
    {
        await taskRepository.AddAsync(task, CancellationToken.None);
    }

    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        taskRepository.GetAllAsync);

    await viewModel.LoadReviewAsync(CancellationToken.None);

    Assert.True(viewModel.HasStuckItems, "Review should load blocked or repeated-snooze tasks as stuck items.");
    Assert.True(viewModel.HasReviewData, "Review should expose stuck items as review data.");
    Assert.Equal(2, viewModel.StuckItems.Count);
    Assert.Equal(blockedTask.Id, viewModel.StuckItems[0].Id);
    Assert.Equal("Blocked", viewModel.StuckItems[0].StatusText);
    Assert.Equal(repeatedSnooze.Id, viewModel.StuckItems[1].Id);
    Assert.Equal("Repeated Snooze", viewModel.StuckItems[1].StatusText);
    Assert.Equal("2 review items", viewModel.ReviewCountText);
    Assert.Equal("2 stuck items this review.", viewModel.WeekSummaryText);
    Assert.Equal("2 stuck items ready.", viewModel.StuckItemsText);
    Assert.Equal("Stuck items ready.", viewModel.StatusText);
}

static async Task ReviewViewModelLoadsNeedsDecisionItemsThroughTaskRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskRepository taskRepository = new(paths);
    TaskItem decisionTask = CreateRepositoryTask("Choose the project scope", dueDate: today.AddDays(1), priority: TaskPriority.High);
    decisionTask.UpdateNotes("Pick the smallest shippable shape.");
    decisionTask.AddTag("Needs Decision");
    TaskItem ordinaryTask = CreateRepositoryTask("Pay the bill", dueDate: today);
    TaskItem doneDecision = CreateRepositoryTask("Already decided", dueDate: today, done: true);
    doneDecision.AddTag("Decision");

    foreach (TaskItem task in new[] { ordinaryTask, doneDecision, decisionTask })
    {
        await taskRepository.AddAsync(task, CancellationToken.None);
    }

    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        taskRepository.GetAllAsync);

    await viewModel.LoadReviewAsync(CancellationToken.None);

    Assert.True(viewModel.HasNeedsDecisionItems, "Review should load tagged tasks as needs-decision items.");
    Assert.True(viewModel.HasReviewData, "Review should expose needs-decision items as review data.");
    Assert.Equal(1, viewModel.NeedsDecisionItems.Count);
    Assert.Equal(decisionTask.Id, viewModel.NeedsDecisionItems[0].Id);
    Assert.Equal("Choose the project scope", viewModel.NeedsDecisionItems[0].Title);
    Assert.Equal("1 review item", viewModel.ReviewCountText);
    Assert.Equal("1 needs-decision item this review.", viewModel.WeekSummaryText);
    Assert.Equal("1 needs-decision item ready.", viewModel.NeedsDecisionText);
    Assert.Equal("Needs-decision items ready.", viewModel.StatusText);
}

static async Task ReviewViewModelSavesNextFocusThroughSettingsMetadataRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskRepository taskRepository = new(paths);
    SettingsMetadataRepository metadataRepository = new(paths);
    DateTimeOffset savedAt = new(2026, 5, 30, 16, 0, 0, TimeSpan.Zero);
    TaskItem decisionTask = CreateRepositoryTask("Choose the project scope", dueDate: today.AddDays(1), priority: TaskPriority.High);
    decisionTask.AddTag("Needs Decision");
    await taskRepository.AddAsync(decisionTask, CancellationToken.None);

    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        taskRepository.GetAllAsync,
        async (selection, cancellationToken) =>
        {
            await metadataRepository.UpsertAsync(
                ReviewNextFocusMetadataKeys.Kind,
                selection.Kind.ToString(),
                savedAt,
                cancellationToken);
            await metadataRepository.UpsertAsync(
                ReviewNextFocusMetadataKeys.ItemId,
                selection.ItemId.ToString("D"),
                savedAt,
                cancellationToken);
            await metadataRepository.UpsertAsync(
                ReviewNextFocusMetadataKeys.Title,
                selection.Title,
                savedAt,
                cancellationToken);
            await metadataRepository.UpsertAsync(
                ReviewNextFocusMetadataKeys.Source,
                selection.SourceText,
                savedAt,
                cancellationToken);
            await metadataRepository.UpsertAsync(
                ReviewNextFocusMetadataKeys.SelectedAt,
                selection.SelectedAt.ToUniversalTime().ToString("O"),
                savedAt,
                cancellationToken);
        });

    await viewModel.LoadReviewAsync(CancellationToken.None);
    viewModel.NeedsDecisionItems[0].ChooseNextFocusCommand.Execute(null);
    await viewModel.SaveReviewAsync(CancellationToken.None);

    SettingsMetadataEntry? kind = await metadataRepository.GetAsync(ReviewNextFocusMetadataKeys.Kind, CancellationToken.None);
    SettingsMetadataEntry? itemId = await metadataRepository.GetAsync(ReviewNextFocusMetadataKeys.ItemId, CancellationToken.None);
    SettingsMetadataEntry? title = await metadataRepository.GetAsync(ReviewNextFocusMetadataKeys.Title, CancellationToken.None);
    SettingsMetadataEntry? source = await metadataRepository.GetAsync(ReviewNextFocusMetadataKeys.Source, CancellationToken.None);
    SettingsMetadataEntry? selectedAt = await metadataRepository.GetAsync(ReviewNextFocusMetadataKeys.SelectedAt, CancellationToken.None);

    Assert.NotNull(kind, "Next focus kind should save in settings metadata.");
    Assert.NotNull(itemId, "Next focus item id should save in settings metadata.");
    Assert.NotNull(title, "Next focus title should save in settings metadata.");
    Assert.NotNull(source, "Next focus source should save in settings metadata.");
    Assert.NotNull(selectedAt, "Next focus selected timestamp should save in settings metadata.");
    Assert.Equal(ReviewNextFocusKind.NeedsDecision.ToString(), kind!.Value);
    Assert.Equal(decisionTask.Id.ToString("D"), itemId!.Value);
    Assert.Equal("Choose the project scope", title!.Value);
    Assert.Equal("Needs Decision", source!.Value);
    Assert.True(DateTimeOffset.TryParse(selectedAt!.Value, out _), "Next focus selected timestamp should be round-trippable.");
    Assert.Equal(savedAt.ToUniversalTime(), kind.UpdatedAt);
    Assert.True(viewModel.HasSavedNextFocus, "Saved review focus should be visible in the view model.");
    Assert.Equal(decisionTask.Id, viewModel.LastSavedNextFocusId.GetValueOrDefault());
}

static async Task ReviewViewModelLoadsFocusAndMovementSuccessesThroughRepositories()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    FocusSessionRepository focusSessionRepository = new(paths);
    MovementSessionRepository movementSessionRepository = new(paths);
    DateTimeOffset startedAt = new(2026, 5, 30, 14, 0, 0, TimeSpan.Zero);
    FocusSession focusWin = FocusSession.Rehydrate(
        Guid.NewGuid(),
        null,
        "Write the Review bridge",
        15,
        FocusSessionStatus.Completed,
        startedAt,
        completedAt: startedAt.AddMinutes(15),
        endedAt: startedAt.AddMinutes(15),
        actualFocusMinutes: 15,
        winNote: "Completed 15 minute focus: Write the Review bridge",
        tags: ["Win"]);
    MovementSession movementWin = MovementSession.Rehydrate(
        Guid.NewGuid(),
        MovementActivityType.Walk,
        "Walk",
        20,
        MovementSessionStatus.Completed,
        startedAt.AddHours(1),
        actualMinutes: 20,
        startedAt: startedAt.AddHours(1).AddMinutes(5),
        completedAt: startedAt.AddHours(1).AddMinutes(25),
        endedAt: startedAt.AddHours(1).AddMinutes(25),
        winNote: "Movement completed: Walk",
        tags: ["Win"]);

    await focusSessionRepository.SaveAsync(focusWin, CancellationToken.None);
    await movementSessionRepository.SaveAsync(movementWin, CancellationToken.None);

    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([]),
        cancellationToken => focusSessionRepository.GetRecentAsync(20, cancellationToken),
        cancellationToken => movementSessionRepository.GetRecentAsync(20, cancellationToken),
        (_, _) => Task.CompletedTask);

    await viewModel.LoadReviewAsync(CancellationToken.None);

    Assert.True(viewModel.HasSmallWins, "Review should load focus and movement wins as small wins.");
    Assert.Equal(2, viewModel.SmallWins.Count);
    Assert.True(
        viewModel.SmallWins.Any(card => card.Id == focusWin.Id
            && card.OwnerType == NoteOwnerType.FocusSession
            && card.SourceText == "Focus win"),
        "Completed focus session should load through the Review repository path.");
    Assert.True(
        viewModel.SmallWins.Any(card => card.Id == movementWin.Id
            && card.OwnerType == NoteOwnerType.MovementSession
            && card.SourceText == "Movement win"),
        "Completed movement session should load through the Review repository path.");
    Assert.Equal("2 small wins this review.", viewModel.WeekSummaryText);
}

static async Task FocusSessionRepositorySavesAndLoadsFocusSessionState()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository taskRepository = new(paths);
    TaskItem task = CreateRepositoryTask("Draft the focus repository", dueDate: new DateOnly(2026, 5, 30));
    await taskRepository.AddAsync(task, CancellationToken.None);

    FocusSessionRepository repository = new(paths);
    FocusSession session = FocusSession.StartForTask(task.Id, "Draft the focus repository", 15);
    await repository.SaveAsync(session, CancellationToken.None);

    FocusSession? started = await repository.GetByIdAsync(session.Id, CancellationToken.None);
    Assert.NotNull(started, "Started focus session should load by id.");
    Assert.Equal(FocusSessionStatus.InProgress, started!.Status);
    Assert.Equal(task.Id, started.TaskId.GetValueOrDefault());
    Assert.Equal(15, started.PlannedMinutes);
    Assert.Equal("Draft the focus repository", started.Intention);

    session.Pause();
    await repository.SaveAsync(session, CancellationToken.None);
    FocusSession? paused = await repository.GetByIdAsync(session.Id, CancellationToken.None);
    Assert.NotNull(paused, "Paused focus session should load by id.");
    Assert.Equal(FocusSessionStatus.Paused, paused!.Status);
    Assert.True(paused.PausedAt.HasValue, "Paused time should persist.");

    session.Resume();
    session.Complete("Small focus win.");
    await repository.SaveAsync(session, CancellationToken.None);

    FocusSession? completed = await repository.GetByIdAsync(session.Id, CancellationToken.None);
    IReadOnlyList<FocusSession> taskSessions = await repository.GetForTaskAsync(task.Id, CancellationToken.None);
    IReadOnlyList<FocusSession> recentSessions = await repository.GetRecentAsync(5, CancellationToken.None);

    Assert.NotNull(completed, "Completed focus session should load by id.");
    Assert.Equal(FocusSessionStatus.Completed, completed!.Status);
    Assert.True(completed.CompletedAt.HasValue, "Completed time should persist.");
    Assert.True(completed.EndedAt.HasValue, "Ended time should persist.");
    Assert.Equal("Small focus win.", completed.WinNote);
    Assert.Contains("Win", completed.Tags);
    Assert.Equal(1, taskSessions.Count);
    Assert.Equal(session.Id, taskSessions[0].Id);
    Assert.Equal(1, recentSessions.Count);
    Assert.Equal(session.Id, recentSessions[0].Id);
}

static async Task StartWorkViewModelRecordsFocusSessionsThroughRepositories()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateOnly today = new(2026, 5, 30);
    TaskRepository taskRepository = new(paths);
    FocusSessionRepository focusSessionRepository = new(paths);
    TaskItem focusTask = CreateRepositoryTask("Save the focus session", dueDate: today);
    await taskRepository.AddAsync(focusTask, CancellationToken.None);

    StartWorkViewModel viewModel = new(
        taskRepository.GetTodayAsync,
        focusSessionRepository.SaveAsync,
        taskRepository.SaveAsync,
        () => today);

    await viewModel.LoadTasksAsync(CancellationToken.None);
    await viewModel.UseSuggestedTaskAsync(CancellationToken.None);
    await viewModel.StartFocusAsync(CancellationToken.None);
    await viewModel.PauseFocusAsync(CancellationToken.None);
    await viewModel.ResumeFocusAsync(CancellationToken.None);
    await viewModel.CompleteFocusAsync(CancellationToken.None);

    Assert.True(viewModel.LastSavedFocusSessionId.HasValue, "Start should expose the saved focus session id.");
    FocusSession? savedSession = await focusSessionRepository.GetByIdAsync(
        viewModel.LastSavedFocusSessionId.GetValueOrDefault(),
        CancellationToken.None);
    TaskItem? savedTask = await taskRepository.GetByIdAsync(focusTask.Id, CancellationToken.None);

    Assert.NotNull(savedSession, "Completed focus session should persist in SQLite.");
    Assert.Equal(FocusSessionStatus.Completed, savedSession!.Status);
    Assert.Equal(focusTask.Id, savedSession.TaskId.GetValueOrDefault());
    Assert.Equal(FocusSession.DefaultPlannedMinutes, savedSession.PlannedMinutes);
    Assert.Contains("Completed 20 minute focus", savedSession.WinNote);
    Assert.Equal("Saved locally: completed focus session.", viewModel.FocusSessionStorageText);
    Assert.Equal("Focus session completed and saved locally.", viewModel.StatusText);
    Assert.NotNull(savedTask, "Starting focus should save the task status locally.");
    Assert.Equal(TaskItemStatus.InProgress, savedTask!.Status);
}

static async Task MovementSessionRepositorySavesAndLoadsMovementSessionState()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset scheduledFor = new(2026, 5, 31, 15, 15, 0, TimeSpan.FromHours(-5));
    MovementSessionRepository repository = new(paths);
    MovementSession session = MovementSession.Schedule(
        MovementActivityType.Walk,
        scheduledFor,
        activityName: "Walk");
    session.SetMindOccupier("Podcast");
    session.MarkWithSpouse();
    session.UpdateNotes("Mind: Podcast; Support: With spouse.");

    await repository.SaveAsync(session, CancellationToken.None);

    MovementSession? planned = await repository.GetByIdAsync(session.Id, CancellationToken.None);
    Assert.NotNull(planned, "Saved movement session should load by id.");
    Assert.Equal(MovementActivityType.Walk, planned!.ActivityType);
    Assert.Equal("Walk", planned.ActivityName);
    Assert.Equal(MovementSessionStatus.Planned, planned.Status);
    Assert.Equal(MovementSession.DefaultPlannedMinutes, planned.PlannedMinutes);
    Assert.Equal(scheduledFor.ToUniversalTime(), planned.ScheduledFor!.Value.ToUniversalTime());
    Assert.Equal("Podcast", planned.MindOccupier);
    Assert.True(planned.IsWithSpouse, "With spouse flag should persist.");
    Assert.Equal("Mind: Podcast; Support: With spouse.", planned.Notes);
    Assert.Contains("With spouse", planned.Tags);

    session.Start();
    await repository.SaveAsync(session, CancellationToken.None);

    MovementSession? active = await repository.GetByIdAsync(session.Id, CancellationToken.None);
    IReadOnlyList<MovementSession> recentSessions = await repository.GetRecentAsync(5, CancellationToken.None);

    Assert.NotNull(active, "Updated movement session should load by id.");
    Assert.Equal(MovementSessionStatus.Active, active!.Status);
    Assert.True(active.StartedAt.HasValue, "Started time should persist.");
    Assert.Equal(1, recentSessions.Count);
    Assert.Equal(session.Id, recentSessions[0].Id);
}

static async Task MoveViewModelSavesMovementSessionsThroughRepository()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 5, 31, 14, 15, 0, TimeSpan.FromHours(-5));
    MovementSessionRepository repository = new(paths);
    MoveViewModel viewModel = new(repository.SaveAsync, () => now);

    viewModel.ChooseWorkoutCommand.Execute(null);
    viewModel.ChooseScheduleCommand.Execute(null);
    viewModel.ChooseAudiobookCommand.Execute(null);
    viewModel.ChooseSoloCommand.Execute(null);
    await viewModel.SaveMovementAsync(CancellationToken.None);

    Assert.True(viewModel.LastSavedMovementSessionId.HasValue, "Move should expose the saved movement session id.");
    MovementSession? savedSession = await repository.GetByIdAsync(
        viewModel.LastSavedMovementSessionId.GetValueOrDefault(),
        CancellationToken.None);

    Assert.NotNull(savedSession, "Saved movement session should persist in SQLite.");
    Assert.Equal(MovementActivityType.Workout, savedSession!.ActivityType);
    Assert.Equal(MovementSessionStatus.Planned, savedSession.Status);
    Assert.Equal(now.AddHours(1).ToUniversalTime(), savedSession.ScheduledFor!.Value.ToUniversalTime());
    Assert.Equal("Audiobook", savedSession.MindOccupier);
    Assert.False(savedSession.IsWithSpouse, "Solo movement should persist without spouse flag.");
    Assert.Equal("Mind: Audiobook; Support: Solo.", savedSession.Notes);
    Assert.Equal("Saved locally: planned movement session.", viewModel.SaveStatusText);
    Assert.Equal("Movement scheduled and saved locally.", viewModel.StatusText);
}

static async Task MoveViewModelMovementSaveAppearsInReview()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    DateTimeOffset now = new(2026, 5, 31, 14, 15, 0, TimeSpan.FromHours(-5));
    MovementSessionRepository movementSessionRepository = new(paths);
    MoveViewModel moveViewModel = new(movementSessionRepository.SaveAsync, () => now);

    moveViewModel.ChooseWorkoutCommand.Execute(null);
    moveViewModel.ChooseNowCommand.Execute(null);
    moveViewModel.ChoosePodcastCommand.Execute(null);
    moveViewModel.ChooseWithSpouseCommand.Execute(null);
    await moveViewModel.SaveMovementAsync(CancellationToken.None);

    ReviewViewModel reviewViewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([]),
        _ => Task.FromResult<IReadOnlyList<FocusSession>>([]),
        cancellationToken => movementSessionRepository.GetRecentAsync(20, cancellationToken),
        (_, _) => Task.CompletedTask);

    await reviewViewModel.LoadReviewAsync(CancellationToken.None);

    Assert.True(moveViewModel.LastSavedMovementSessionId.HasValue, "Move should save a movement session locally.");
    Assert.True(reviewViewModel.HasSmallWins, "Review should expose saved movement as a success item.");
    Assert.Equal(1, reviewViewModel.SmallWins.Count);
    Assert.Equal(moveViewModel.LastSavedMovementSessionId.GetValueOrDefault(), reviewViewModel.SmallWins[0].Id);
    Assert.Equal(NoteOwnerType.MovementSession, reviewViewModel.SmallWins[0].OwnerType);
    Assert.Equal("Movement win", reviewViewModel.SmallWins[0].SourceText);
    Assert.Equal("Movement started: Workout", reviewViewModel.SmallWins[0].Text);
    Assert.Equal("1 small win this review.", reviewViewModel.WeekSummaryText);
}

static async Task SearchServiceFindsTasksNotesProjectsAndSourceLinks()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository taskRepository = new(paths);
    NoteRepository noteRepository = new(paths);
    TaskItem pharmacyTask = CreateRepositoryTask("Call pharmacy about refills");
    pharmacyTask.UpdateNotes("Ask about the glucose meter prescription.");
    await taskRepository.AddAsync(pharmacyTask, CancellationToken.None);
    NoteItem note = NoteItem.Create(NoteOwnerType.Task, pharmacyTask.Id, "Bring therapy notebook to the next visit.");
    await noteRepository.AddAsync(note, CancellationToken.None);
    Guid projectId = Guid.NewGuid();
    await InsertProjectRowAsync(paths, projectId, "Garden project");
    Guid sourceLinkId = Guid.NewGuid();
    await InsertSourceLinkRowAsync(
        paths,
        sourceLinkId,
        SourceLinkItemType.Task,
        pharmacyTask.Id,
        SourceSystem.MicrosoftPlanner,
        "planner-card-77",
        "Planner Card 77");

    SearchService searchService = new(paths);

    IReadOnlyList<LocalSearchResult> taskTitleResults =
        await searchService.SearchAsync("pharmacy", CancellationToken.None);
    IReadOnlyList<LocalSearchResult> taskNoteResults =
        await searchService.SearchAsync("glucose meter", CancellationToken.None);
    IReadOnlyList<LocalSearchResult> noteResults =
        await searchService.SearchAsync("therapy notebook", CancellationToken.None);
    IReadOnlyList<LocalSearchResult> projectResults =
        await searchService.SearchAsync("garden", CancellationToken.None);
    IReadOnlyList<LocalSearchResult> sourceLinkResults =
        await searchService.SearchAsync("planner-card-77", CancellationToken.None);

    Assert.True(
        taskTitleResults.Any(result => result.Kind == LocalSearchResultKind.Task
            && result.ItemId == pharmacyTask.Id
            && result.Title == "Call pharmacy about refills"),
        "Search should find task titles.");
    Assert.True(
        taskNoteResults.Any(result => result.Kind == LocalSearchResultKind.Task
            && result.ItemId == pharmacyTask.Id
            && result.Detail.Contains("glucose meter", StringComparison.OrdinalIgnoreCase)),
        "Search should find task notes.");
    Assert.True(
        noteResults.Any(result => result.Kind == LocalSearchResultKind.Note
            && result.ItemId == note.Id
            && result.Title.Contains("therapy notebook", StringComparison.OrdinalIgnoreCase)),
        "Search should find note text.");
    Assert.True(
        projectResults.Any(result => result.Kind == LocalSearchResultKind.Project
            && result.ItemId == projectId
            && result.Title == "Garden project"),
        "Search should find projects.");
    Assert.True(
        sourceLinkResults.Any(result => result.Kind == LocalSearchResultKind.SourceLink
            && result.ItemId == sourceLinkId
            && result.LocalItemId == pharmacyTask.Id
            && result.Title == "Planner Card 77"),
        "Search should find source links.");
}

static async Task FindViewModelSearchesLocalDataThroughSearchService()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    TaskRepository taskRepository = new(paths);
    TaskItem task = CreateRepositoryTask("Local search view model");
    await taskRepository.AddAsync(task, CancellationToken.None);
    SearchService searchService = new(paths);
    FindViewModel viewModel = new(searchService.SearchAsync)
    {
        SearchText = "view model"
    };

    await viewModel.SearchAsync(CancellationToken.None);

    Assert.True(viewModel.HasResults, "Find should load local search results through SearchService.");
    Assert.Equal(1, viewModel.Results.Count);
    Assert.Equal(task.Id, viewModel.Results[0].Id);
    Assert.Equal(LocalSearchResultKind.Task, viewModel.Results[0].Kind);
    Assert.Equal("Local search view model", viewModel.Results[0].Title);
    Assert.Equal("Search complete.", viewModel.StatusText);
    Assert.Equal("1 result", viewModel.ResultsCountText);
}

static async Task SettingsServiceSavesProjectFileSelection()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    SettingsService settingsService = new(paths);
    string projectFilePath = Path.Combine(
        workspace.Path,
        "Plans",
        "Personal Success Plan.mpp");
    AppSettings settings = AppSettings.CreateDefault();
    settings.ProjectDesktop.LocalProjectFilePath = $"  {projectFilePath}  ";

    await settingsService.SaveAsync(settings, CancellationToken.None);
    AppSettings loaded = await settingsService.LoadOrCreateAsync(CancellationToken.None);

    Assert.Equal(projectFilePath, loaded.ProjectDesktop.LocalProjectFilePath);
}

static async Task SettingsMetadataRepositoryUpsertsAndDeletesMetadata()
{
    using TestWorkspace workspace = TestWorkspace.Create();
    AppPaths paths = new(workspace.Path);
    await CreateMigratedDatabaseAsync(paths);

    SettingsMetadataRepository repository = new(paths);
    DateTimeOffset firstUpdate = new(2026, 5, 30, 8, 0, 0, TimeSpan.Zero);
    DateTimeOffset secondUpdate = new(2026, 5, 30, 9, 0, 0, TimeSpan.Zero);

    await repository.UpsertAsync("last_opened_screen", "Home", firstUpdate, CancellationToken.None);
    SettingsMetadataEntry? entry = await repository.GetAsync("last_opened_screen", CancellationToken.None);
    Assert.NotNull(entry, "Metadata should load after insert.");
    Assert.Equal("last_opened_screen", entry!.Key);
    Assert.Equal("Home", entry.Value);
    Assert.Equal(firstUpdate.ToUniversalTime(), entry.UpdatedAt);

    await repository.UpsertAsync("last_opened_screen", "Settings", secondUpdate, CancellationToken.None);
    SettingsMetadataEntry? updated = await repository.GetAsync("last_opened_screen", CancellationToken.None);
    Assert.NotNull(updated, "Metadata should load after update.");
    Assert.Equal("Settings", updated!.Value);
    Assert.Equal(secondUpdate.ToUniversalTime(), updated.UpdatedAt);

    IReadOnlyList<SettingsMetadataEntry> all = await repository.GetAllAsync(CancellationToken.None);
    Assert.Equal(1, all.Count);

    await repository.DeleteAsync("last_opened_screen", CancellationToken.None);
    SettingsMetadataEntry? deleted = await repository.GetAsync("last_opened_screen", CancellationToken.None);
    Assert.Null(deleted, "Deleted metadata should not load by key.");
}

static async Task CreateMigratedDatabaseAsync(AppPaths paths)
{
    DatabaseService database = new(paths);
    await database.OpenAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);
}

static async Task InsertProjectRowAsync(AppPaths paths, Guid projectId, string name)
{
    await using SqliteConnection connection = new($"Data Source={paths.DatabasePath};Pooling=False");
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText =
        """
        INSERT INTO projects (
            id,
            name,
            notes,
            status,
            priority,
            created_at,
            immediate_need,
            minimum_win,
            task_ids_json,
            milestone_ids_json,
            tags_json)
        VALUES (
            $id,
            $name,
            '',
            $status,
            $priority,
            $createdAt,
            '',
            '',
            '[]',
            '[]',
            '[]');
        """;
    command.Parameters.AddWithValue("$id", projectId.ToString("D"));
    command.Parameters.AddWithValue("$name", name);
    command.Parameters.AddWithValue("$status", ProjectStatus.Active.ToString());
    command.Parameters.AddWithValue("$priority", TaskPriority.Normal.ToString());
    command.Parameters.AddWithValue("$createdAt", DateTimeOffset.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
}

static async Task InsertSourceLinkRowAsync(
    AppPaths paths,
    Guid sourceLinkId,
    SourceLinkItemType localItemType,
    Guid localItemId,
    SourceSystem sourceSystem,
    string externalId,
    string externalDisplayName)
{
    await using SqliteConnection connection = new($"Data Source={paths.DatabasePath};Pooling=False");
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText =
        """
        INSERT INTO source_links (
            id,
            local_item_type,
            local_item_id,
            source_system,
            external_id,
            external_container_id,
            external_display_name,
            external_web_url,
            source_version,
            sync_state,
            created_at,
            retry_count,
            failure_message,
            is_read_only)
        VALUES (
            $id,
            $localItemType,
            $localItemId,
            $sourceSystem,
            $externalId,
            'planner-bucket',
            $externalDisplayName,
            '',
            '',
            $syncState,
            $createdAt,
            0,
            '',
            0);
        """;
    command.Parameters.AddWithValue("$id", sourceLinkId.ToString("D"));
    command.Parameters.AddWithValue("$localItemType", localItemType.ToString());
    command.Parameters.AddWithValue("$localItemId", localItemId.ToString("D"));
    command.Parameters.AddWithValue("$sourceSystem", sourceSystem.ToString());
    command.Parameters.AddWithValue("$externalId", externalId);
    command.Parameters.AddWithValue("$externalDisplayName", externalDisplayName);
    command.Parameters.AddWithValue("$syncState", SyncState.Synced.ToString());
    command.Parameters.AddWithValue("$createdAt", DateTimeOffset.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
}

static TaskItem CreateRepositoryTask(
    string title,
    DateOnly? dueDate = null,
    DateOnly? startDate = null,
    TaskPriority priority = TaskPriority.Normal,
    bool inProgress = false,
    bool done = false)
{
    TaskItem task = TaskItem.Capture(title);
    if (dueDate.HasValue || startDate.HasValue)
    {
        task.Schedule(dueDate, startDate);
    }

    task.SetPriority(priority);
    if (inProgress)
    {
        task.Start();
    }

    if (done)
    {
        task.Complete();
    }

    return task;
}

static async Task<object?> ReadScalarAsync(string databasePath, string commandText)
{
    await using SqliteConnection connection = new($"Data Source={databasePath};Pooling=False");
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText = commandText;
    return await command.ExecuteScalarAsync();
}

static async Task<bool> TableExistsAsync(string databasePath, string tableName)
{
    object? count = await ReadScalarAsync(
        databasePath,
        $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = '{tableName}';");

    return Convert.ToInt64(count) == 1;
}

static async Task<bool> ColumnExistsAsync(string databasePath, string tableName, string columnName)
{
    await using SqliteConnection connection = new($"Data Source={databasePath};Pooling=False");
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText = $"PRAGMA table_info({tableName});";

    await using SqliteDataReader reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}

static async Task<bool> IndexExistsAsync(string databasePath, string indexName)
{
    object? count = await ReadScalarAsync(
        databasePath,
        $"SELECT COUNT(*) FROM sqlite_master WHERE type = 'index' AND name = '{indexName}';");

    return Convert.ToInt64(count) == 1;
}

static bool IsSqliteDatabase(string path)
{
    byte[] header = File.ReadAllBytes(path).Take(16).ToArray();
    string headerText = System.Text.Encoding.ASCII.GetString(header);
    return headerText.StartsWith("SQLite format 3", StringComparison.Ordinal);
}

static HttpResponseMessage JsonResponse(
    string json,
    HttpStatusCode statusCode = HttpStatusCode.OK)
{
    return new HttpResponseMessage(statusCode)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
}

internal sealed class TestWorkspace : IDisposable
{
    private TestWorkspace(string path)
    {
        Path = path;
    }

    public string Path { get; }

    public static TestWorkspace Create()
    {
        string path = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "SuccessPlannerMCP",
            "InfrastructureTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(path);
        return new TestWorkspace(path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            TryDeleteWorkspace();
        }
    }

    private void TryDeleteWorkspace()
    {
        for (int attempt = 0; attempt < 5; attempt++)
        {
            try
            {
                SqliteConnection.ClearAllPools();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Directory.Delete(Path, recursive: true);
                return;
            }
            catch (IOException)
            {
                Thread.Sleep(100);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(100);
            }
        }
    }
}

internal sealed class TestBackgroundWorker : IBackgroundWorker
{
    public bool IsRunning { get; private set; }

    public int StartCount { get; private set; }

    public int StopCount { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StartCount++;
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        StopCount++;
        IsRunning = false;
        return Task.CompletedTask;
    }
}

internal sealed class TestMicrosoftToDoConnectionProbe : IMicrosoftToDoConnectionProbe
{
    private readonly Func<DateTimeOffset, CancellationToken, Task<MicrosoftToDoConnectionStatus>> _testAsync;

    public TestMicrosoftToDoConnectionProbe(
        Func<DateTimeOffset, CancellationToken, Task<MicrosoftToDoConnectionStatus>> testAsync)
    {
        _testAsync = testAsync;
    }

    public int CallCount { get; private set; }

    public Task<MicrosoftToDoConnectionStatus> TestConnectionAsync(
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        return _testAsync(checkedAt, cancellationToken);
    }
}

internal sealed class TestMicrosoftToDoAccessTokenProvider : IMicrosoftToDoAccessTokenProvider
{
    private readonly string? _accessToken;

    public TestMicrosoftToDoAccessTokenProvider(string? accessToken)
    {
        _accessToken = accessToken;
    }

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_accessToken);
    }
}

internal sealed class TestMicrosoftPlannerAvailabilityProbe : IMicrosoftPlannerAvailabilityProbe
{
    private readonly Func<DateTimeOffset, CancellationToken, Task<MicrosoftPlannerConnectionStatus>> _testAsync;

    public TestMicrosoftPlannerAvailabilityProbe(
        Func<DateTimeOffset, CancellationToken, Task<MicrosoftPlannerConnectionStatus>> testAsync)
    {
        _testAsync = testAsync;
    }

    public int CallCount { get; private set; }

    public Task<MicrosoftPlannerConnectionStatus> TestAvailabilityAsync(
        DateTimeOffset checkedAt,
        CancellationToken cancellationToken = default)
    {
        CallCount++;
        return _testAsync(checkedAt, cancellationToken);
    }
}

internal sealed class TestMicrosoftPlannerAccessTokenProvider : IMicrosoftPlannerAccessTokenProvider
{
    private readonly string? _accessToken;

    public TestMicrosoftPlannerAccessTokenProvider(string? accessToken)
    {
        _accessToken = accessToken;
    }

    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_accessToken);
    }
}

internal sealed class TestMicrosoftPlannerTaskAdapter : IMicrosoftPlannerTaskAdapter
{
    private readonly Queue<MicrosoftPlannerPullResult> _results;

    public TestMicrosoftPlannerTaskAdapter(params MicrosoftPlannerPullResult[] results)
    {
        _results = new Queue<MicrosoftPlannerPullResult>(results);
    }

    public int CallCount { get; private set; }

    public Task<MicrosoftPlannerPullResult> PullAssignedTasksAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;

        if (_results.Count == 0)
        {
            return Task.FromResult(new MicrosoftPlannerPullResult(
                MicrosoftPlannerConnectionStatus.Available()));
        }

        return Task.FromResult(_results.Dequeue());
    }
}

internal sealed class TestMicrosoftProjectAutomationAdapter : IMicrosoftProjectAutomationAdapter
{
    private readonly IReadOnlyList<MicrosoftProjectImportedTask> _tasks;

    public TestMicrosoftProjectAutomationAdapter(params MicrosoftProjectImportedTask[] tasks)
    {
        _tasks = tasks;
    }

    public int CallCount { get; private set; }

    public string LastProjectFilePath { get; private set; } = string.Empty;

    public Task<IReadOnlyList<MicrosoftProjectImportedTask>> ImportTasksAsync(
        string projectFilePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        LastProjectFilePath = projectFilePath;
        return Task.FromResult(_tasks);
    }
}

internal sealed class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handleRequest;

    public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handleRequest)
    {
        _handleRequest = handleRequest;
    }

    public int CallCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        return Task.FromResult(_handleRequest(request));
    }
}

internal static class TestRunner
{
    public static void RunAll(params (string Name, Func<Task> Test)[] tests)
    {
        int passed = 0;

        foreach ((string name, Func<Task> test) in tests)
        {
            try
            {
                test().GetAwaiter().GetResult();
                passed++;
                Console.WriteLine($"PASS {name}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"FAIL {name}");
                Console.Error.WriteLine(ex);
                Environment.ExitCode = 1;
                return;
            }
        }

        Console.WriteLine($"{passed} infrastructure tests passed.");
    }
}

internal static class Assert
{
    public static void Equal<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
        }
    }

    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void False(bool condition, string message)
    {
        if (condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void Null(object? value, string message)
    {
        if (value is not null)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void NotNull(object? value, string message)
    {
        if (value is null)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void Contains<T>(T expected, IEnumerable<T> values)
    {
        if (!values.Contains(expected))
        {
            throw new InvalidOperationException($"Expected collection to contain '{expected}'.");
        }
    }

    public static void Contains(string expectedSubstring, string value)
    {
        if (!value.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Expected '{value}' to contain '{expectedSubstring}'.");
        }
    }

    public static async Task<TException> ThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
        }
        catch (TException ex)
        {
            return ex;
        }

        throw new InvalidOperationException($"Expected exception of type '{typeof(TException).Name}'.");
    }
}
