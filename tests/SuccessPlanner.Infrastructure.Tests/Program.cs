using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Bootstrap;
using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.ViewModels;

TestRunner.RunAll(
    ("DatabaseService creates a real SQLite database", DatabaseServiceCreatesSqliteDatabase),
    ("DatabaseService replaces the legacy bootstrap marker", DatabaseServiceReplacesLegacyMarker),
    ("DatabaseService records repeatable migrations", DatabaseServiceRecordsRepeatableMigrations),
    ("DatabaseService creates core application tables", DatabaseServiceCreatesCoreApplicationTables),
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

    Assert.Equal(2, firstRun.AppliedCountThisRun);
    Assert.Equal(0, secondRun.AppliedCountThisRun);
    Assert.Equal(2, secondRun.TotalAppliedCount);
    Assert.Equal(2L, await ReadScalarAsync(paths.DatabasePath, "SELECT COUNT(*) FROM schema_migrations;"));
    Assert.Equal(1L, await ReadScalarAsync(paths.DatabasePath, "SELECT version FROM schema_migrations;"));
    Assert.Equal(
        "Create local store metadata",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 1;"));
    Assert.Equal(
        "Create core application tables",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 2;"));
    Assert.Equal(
        "Success Planner MCP SQLite local store",
        await ReadScalarAsync(paths.DatabasePath, "SELECT value FROM local_store_metadata WHERE key = 'store_kind';"));
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
    Assert.Equal(2, health.AppliedMigrationCount);
    Assert.Equal(2, health.LatestAppliedMigration);
    Assert.Equal(2, health.RequiredMigrationCount);
    Assert.Equal(2, health.LatestRequiredMigration);
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
    Assert.Equal(2, result.Migration.AppliedCountThisRun);
    Assert.Equal(2, result.Migration.TotalAppliedCount);
    Assert.Equal(2, result.Migration.LatestAppliedVersion);
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

    Assert.Equal(2, firstResult.Migration.AppliedCountThisRun);
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

static bool IsSqliteDatabase(string path)
{
    byte[] header = File.ReadAllBytes(path).Take(16).ToArray();
    string headerText = System.Text.Encoding.ASCII.GetString(header);
    return headerText.StartsWith("SQLite format 3", StringComparison.Ordinal);
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
