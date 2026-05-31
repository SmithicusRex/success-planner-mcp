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
    ("TaskRepository loads today tasks", TaskRepositoryLoadsTodayTasks),
    ("TaskRepository loads recent active tasks", TaskRepositoryLoadsRecentActiveTasks),
    ("TodayViewModel saves task actions through TaskRepository", TodayViewModelSavesTaskActionsThroughTaskRepository),
    ("TaskRepository deletes tasks", TaskRepositoryDeletesTasks),
    ("CaptureViewModel saves captured tasks through TaskRepository", CaptureViewModelSavesCapturedTasksThroughTaskRepository),
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
