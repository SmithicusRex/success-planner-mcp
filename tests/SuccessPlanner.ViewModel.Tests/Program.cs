using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Infrastructure;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.Services;
using SuccessPlanner.App.ViewModels;

TestRunner.RunAll(
    ("AppShellViewModel refreshes visible sync status", AppShellViewModelRefreshesVisibleSyncStatus),
    ("AppShellViewModel reports sync status read failure", AppShellViewModelReportsSyncStatusReadFailure),
    ("SettingsViewModel shows To Do connection status", SettingsViewModelShowsToDoConnectionStatus),
    ("SettingsViewModel updates To Do status when disabled", SettingsViewModelUpdatesToDoStatusWhenDisabled),
    ("SettingsViewModel tests To Do connection", SettingsViewModelTestsToDoConnection),
    ("SettingsViewModel shows failed To Do connection status", SettingsViewModelShowsFailedToDoConnectionStatus),
    ("SettingsViewModel shows Project desktop detection status", SettingsViewModelShowsProjectDesktopDetectionStatus),
    ("SettingsViewModel detects Project desktop", SettingsViewModelDetectsProjectDesktop),
    ("SettingsViewModel shows missing Project desktop status", SettingsViewModelShowsMissingProjectDesktopStatus),
    ("SettingsViewModel updates Project status when disabled", SettingsViewModelUpdatesProjectStatusWhenDisabled),
    ("SettingsViewModel shows Project file selection status", SettingsViewModelShowsProjectFileSelectionStatus),
    ("SettingsViewModel selects Project file", SettingsViewModelSelectsProjectFile),
    ("SettingsViewModel clears Project file selection", SettingsViewModelClearsProjectFileSelection),
    ("CaptureViewModel starts in a simple ready state", CaptureViewModelStartsReady),
    ("CaptureViewModel applies date hint buttons", CaptureViewModelAppliesDateHintButtons),
    ("CaptureViewModel applies destination choices", CaptureViewModelAppliesDestinationChoices),
    ("CaptureViewModel validates an empty title", CaptureViewModelValidatesEmptyTitle),
    ("CaptureViewModel creates a captured task draft", CaptureViewModelCreatesCapturedTaskDraft),
    ("CaptureViewModel creates a scheduled task draft when date is selected", CaptureViewModelCreatesScheduledTaskDraftWhenDateIsSelected),
    ("CaptureViewModel saves a captured task locally", CaptureViewModelSavesCapturedTaskLocally),
    ("CaptureViewModel captures another task after success", CaptureViewModelCapturesAnotherTaskAfterSuccess),
    ("CaptureViewModel resets the capture form", CaptureViewModelResetsCaptureForm),
    ("CaptureViewModel raises property change notifications", CaptureViewModelRaisesPropertyChangeNotifications),
    ("TodayViewModel starts in a simple ready state", TodayViewModelStartsReady),
    ("TodayViewModel loads today tasks", TodayViewModelLoadsTodayTasks),
    ("TodayViewModel creates task card display state", TodayViewModelCreatesTaskCardDisplayState),
    ("TodayViewModel handles task card actions", TodayViewModelHandlesTaskCardActions),
    ("TodayViewModel shows an empty today state", TodayViewModelShowsEmptyTodayState),
    ("TodayViewModel reports load failures", TodayViewModelReportsLoadFailures),
    ("DoneViewModel starts in a simple ready state", DoneViewModelStartsReady),
    ("DoneViewModel loads recent active tasks", DoneViewModelLoadsRecentActiveTasks),
    ("DoneViewModel creates task card display state", DoneViewModelCreatesTaskCardDisplayState),
    ("DoneViewModel completes selected task", DoneViewModelCompletesSelectedTask),
    ("DoneViewModel shows brief success feedback", DoneViewModelShowsBriefSuccessFeedback),
    ("DoneViewModel shows an empty done state", DoneViewModelShowsEmptyDoneState),
    ("DoneViewModel reports load failures", DoneViewModelReportsLoadFailures),
    ("PlanViewModel starts in a simple ready state", PlanViewModelStartsReady),
    ("PlanViewModel loads unplanned inbox", PlanViewModelLoadsUnplannedInbox),
    ("PlanViewModel applies planning controls", PlanViewModelAppliesPlanningControls),
    ("PlanViewModel splits selected item into tiny steps", PlanViewModelSplitsSelectedItemIntoTinySteps),
    ("PlanViewModel saves planning changes locally", PlanViewModelSavesPlanningChangesLocally),
    ("PlanViewModel reports save failures", PlanViewModelReportsSaveFailures),
    ("PlanViewModel creates inbox card display state", PlanViewModelCreatesInboxCardDisplayState),
    ("PlanViewModel creates tiny step display state", PlanViewModelCreatesTinyStepDisplayState),
    ("PlanViewModel shows an empty unplanned inbox", PlanViewModelShowsEmptyUnplannedInbox),
    ("PlanViewModel reports load failures", PlanViewModelReportsLoadFailures),
    ("ReviewViewModel starts in a simple ready state", ReviewViewModelStartsReady),
    ("ReviewViewModel loads small wins", ReviewViewModelLoadsSmallWins),
    ("ReviewViewModel loads focus and movement success items", ReviewViewModelLoadsFocusAndMovementSuccessItems),
    ("ReviewViewModel loads stuck items", ReviewViewModelLoadsStuckItems),
    ("ReviewViewModel loads needs-decision items", ReviewViewModelLoadsNeedsDecisionItems),
    ("ReviewViewModel lets user choose next focus", ReviewViewModelLetsUserChooseNextFocus),
    ("ReviewViewModel saves next focus", ReviewViewModelSavesNextFocus),
    ("ReviewViewModel shows empty small wins state", ReviewViewModelShowsEmptySmallWinsState),
    ("ReviewViewModel reports load failures", ReviewViewModelReportsLoadFailures),
    ("ReviewViewModel creates small win display state", ReviewViewModelCreatesSmallWinDisplayState),
    ("ReviewViewModel creates stuck item display state", ReviewViewModelCreatesStuckItemDisplayState),
    ("ReviewViewModel creates needs-decision display state", ReviewViewModelCreatesNeedsDecisionDisplayState),
    ("FindViewModel starts in a simple ready state", FindViewModelStartsReady),
    ("FindViewModel updates query state", FindViewModelUpdatesQueryState),
    ("FindViewModel clears search state", FindViewModelClearsSearchState),
    ("FindViewModel searches local results", FindViewModelSearchesLocalResults),
    ("FindViewModel opens selected local item", FindViewModelOpensSelectedLocalItem),
    ("FindViewModel handles no results", FindViewModelHandlesNoResults),
    ("FindViewModel reports search failures", FindViewModelReportsSearchFailures),
    ("StartWorkViewModel starts in a simple ready state", StartWorkViewModelStartsReady),
    ("StartWorkViewModel applies session choices", StartWorkViewModelAppliesSessionChoices),
    ("StartWorkViewModel loads focus task options", StartWorkViewModelLoadsFocusTaskOptions),
    ("StartWorkViewModel suggests a best next action", StartWorkViewModelSuggestsBestNextAction),
    ("StartWorkViewModel uses the suggested action", StartWorkViewModelUsesSuggestedAction),
    ("StartWorkViewModel selects a focus task", StartWorkViewModelSelectsFocusTask),
    ("StartWorkViewModel starts a focus session", StartWorkViewModelStartsFocusSession),
    ("StartWorkViewModel pauses and resumes a focus session", StartWorkViewModelPausesAndResumesFocusSession),
    ("StartWorkViewModel completes a focus session", StartWorkViewModelCompletesFocusSession),
    ("StartWorkViewModel blocks a focus session without completing the task", StartWorkViewModelBlocksFocusSessionWithoutCompletingTask),
    ("StartWorkViewModel saves focus session state changes", StartWorkViewModelSavesFocusSessionStateChanges),
    ("StartWorkViewModel suggests a break after completion", StartWorkViewModelSuggestsBreakAfterCompletion),
    ("StartWorkViewModel creates task option display state", StartWorkViewModelCreatesTaskOptionDisplayState),
    ("StartWorkViewModel shows an empty focus state", StartWorkViewModelShowsEmptyFocusState),
    ("StartWorkViewModel reports load failures", StartWorkViewModelReportsLoadFailures),
    ("MoveViewModel starts in a simple ready state", MoveViewModelStartsReady),
    ("MoveViewModel applies movement activity choices", MoveViewModelAppliesMovementActivityChoices),
    ("MoveViewModel applies now and schedule choices", MoveViewModelAppliesNowAndScheduleChoices),
    ("MoveViewModel applies mind occupier choices", MoveViewModelAppliesMindOccupierChoices),
    ("MoveViewModel applies spouse option choices", MoveViewModelAppliesSpouseOptionChoices),
    ("MoveViewModel saves movement activity locally", MoveViewModelSavesMovementActivityLocally));

static void AppShellViewModelRefreshesVisibleSyncStatus()
{
    NavigationService navigationService = CreateShellNavigationService();
    AppShellViewModel viewModel = new(
        "Ready",
        "Local control center",
        navigationService,
        _ => Task.FromResult(new SyncQueueStatus(
            PendingCount: 2,
            SyncingCount: 1,
            SyncedCount: 4,
            FailedCount: 0,
            ConflictCount: 0,
            DisabledCount: 0)));

    viewModel.RefreshSyncStatusAsync().GetAwaiter().GetResult();

    Assert.Equal("Syncing now", viewModel.SyncStatusText);
    Assert.Equal("#EAF2FF", viewModel.SyncStatusBackgroundColor);
    Assert.Equal("#2F6FED", viewModel.SyncStatusDotColor);
    Assert.Contains("Pending: 2", viewModel.SyncStatusDetailText);
    Assert.Contains("Syncing: 1", viewModel.SyncStatusDetailText);
}

static void AppShellViewModelReportsSyncStatusReadFailure()
{
    NavigationService navigationService = CreateShellNavigationService();
    AppShellViewModel viewModel = new(
        "Ready",
        "Local control center",
        navigationService,
        _ => throw new InvalidOperationException("database unavailable"));

    viewModel.RefreshSyncStatusAsync().GetAwaiter().GetResult();

    Assert.Equal("Sync unavailable", viewModel.SyncStatusText);
    Assert.Equal("#FFF1D6", viewModel.SyncStatusBackgroundColor);
    Assert.Contains("Local data is still stored safely.", viewModel.SyncStatusDetailText);
}

static void SettingsViewModelShowsToDoConnectionStatus()
{
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbe(
        AppSettings.CreateDefault(),
        (_, _) => Task.FromResult(MicrosoftToDoConnectionStatus.Connected()));

    Assert.Equal("Ready to connect", viewModel.MicrosoftToDoStatusText);
    Assert.Contains("Connect Microsoft To Do", viewModel.MicrosoftToDoStatusDetailText);
    Assert.Equal("#F4F7FB", viewModel.MicrosoftToDoStatusBackgroundColor);
    Assert.Equal("#4E5965", viewModel.MicrosoftToDoStatusAccentColor);
    Assert.True(viewModel.CanTestMicrosoftToDoConnection, "Enabled To Do status should allow testing.");
    Assert.False(viewModel.MicrosoftToDoNeedsAttention, "Initial enabled To Do status should not need attention.");
}

static void SettingsViewModelUpdatesToDoStatusWhenDisabled()
{
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbe(
        AppSettings.CreateDefault(),
        (_, _) => Task.FromResult(MicrosoftToDoConnectionStatus.Connected()));

    viewModel.EnableMicrosoftToDo = false;

    Assert.Equal("To Do is off", viewModel.MicrosoftToDoStatusText);
    Assert.Contains("turned off", viewModel.MicrosoftToDoStatusDetailText);
    Assert.Equal("#EEF0F3", viewModel.MicrosoftToDoStatusBackgroundColor);
    Assert.False(viewModel.CanTestMicrosoftToDoConnection, "Disabled To Do status should not allow testing.");
    Assert.True(viewModel.HasChanges, "Changing the To Do switch should mark Settings dirty.");

    viewModel.EnableMicrosoftToDo = true;

    Assert.Equal("Ready to connect", viewModel.MicrosoftToDoStatusText);
    Assert.True(viewModel.CanTestMicrosoftToDoConnection, "Re-enabled To Do status should allow testing.");
}

static void SettingsViewModelTestsToDoConnection()
{
    DateTimeOffset now = new(2026, 6, 1, 23, 0, 0, TimeSpan.Zero);
    TestMicrosoftToDoConnectionProbe probe = new((checkedAt, _) =>
        Task.FromResult(MicrosoftToDoConnectionStatus.Connected("smith@example.com", checkedAt)));
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbeInstance(
        AppSettings.CreateDefault(),
        probe,
        now);

    viewModel.TestMicrosoftToDoConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal(1, probe.CallCount);
    Assert.Equal("To Do connected", viewModel.MicrosoftToDoStatusText);
    Assert.Contains("smith@example.com", viewModel.MicrosoftToDoStatusDetailText);
    Assert.Equal("#E7F8EE", viewModel.MicrosoftToDoStatusBackgroundColor);
    Assert.Equal("#1E6B3A", viewModel.MicrosoftToDoStatusAccentColor);
    Assert.True(viewModel.CanTestMicrosoftToDoConnection, "Connected To Do status should allow retesting.");
}

static void SettingsViewModelShowsFailedToDoConnectionStatus()
{
    TestMicrosoftToDoConnectionProbe probe = new((checkedAt, _) =>
        Task.FromResult(MicrosoftToDoConnectionStatus.Failed("Token cache unavailable.", checkedAt)));
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbeInstance(
        AppSettings.CreateDefault(),
        probe,
        new DateTimeOffset(2026, 6, 1, 23, 10, 0, TimeSpan.Zero));

    viewModel.TestMicrosoftToDoConnectionAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal("Connection failed", viewModel.MicrosoftToDoStatusText);
    Assert.Contains("Token cache unavailable.", viewModel.MicrosoftToDoStatusDetailText);
    Assert.Equal("#FFE7E0", viewModel.MicrosoftToDoStatusBackgroundColor);
    Assert.Equal("#B8331F", viewModel.MicrosoftToDoStatusAccentColor);
    Assert.True(viewModel.MicrosoftToDoNeedsAttention, "Failed To Do status should stay visible.");
    Assert.True(viewModel.CanTestMicrosoftToDoConnection, "Failed To Do status should remain recoverable by retesting.");
}

static void SettingsViewModelShowsProjectDesktopDetectionStatus()
{
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbe(
        AppSettings.CreateDefault(),
        (_, _) => Task.FromResult(MicrosoftToDoConnectionStatus.Connected()));

    Assert.Equal("Ready to detect", viewModel.MicrosoftProjectDesktopStatusText);
    Assert.Contains("Detect Project", viewModel.MicrosoftProjectDesktopStatusDetailText);
    Assert.Equal("#F4F7FB", viewModel.MicrosoftProjectDesktopStatusBackgroundColor);
    Assert.Equal("#4E5965", viewModel.MicrosoftProjectDesktopStatusAccentColor);
    Assert.True(viewModel.CanDetectMicrosoftProjectDesktop, "Enabled Project desktop should allow detection.");
    Assert.False(viewModel.MicrosoftProjectDesktopNeedsAttention, "Project desktop should not need attention before detection.");
}

static void SettingsViewModelDetectsProjectDesktop()
{
    string programFilesRoot = CreateFakeProjectDesktopInstall(out string executablePath);
    SettingsViewModel viewModel = CreateSettingsViewModelWithProjectDetector(
        AppSettings.CreateDefault(),
        new MicrosoftProjectDesktopDetector([programFilesRoot], pathDirectories: []));

    viewModel.DetectMicrosoftProjectDesktopAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal("Project detected", viewModel.MicrosoftProjectDesktopStatusText);
    Assert.Contains(executablePath, viewModel.MicrosoftProjectDesktopStatusDetailText);
    Assert.Equal("#E7F8EE", viewModel.MicrosoftProjectDesktopStatusBackgroundColor);
    Assert.Equal("#1E6B3A", viewModel.MicrosoftProjectDesktopStatusAccentColor);
    Assert.True(viewModel.CanDetectMicrosoftProjectDesktop, "Detected Project desktop should allow a refresh detection.");
    Assert.False(viewModel.MicrosoftProjectDesktopNeedsAttention, "Detected Project desktop should not need attention.");
}

static void SettingsViewModelShowsMissingProjectDesktopStatus()
{
    string missingRoot = Path.Combine(
        Path.GetTempPath(),
        "SuccessPlannerMCP",
        "ViewModelTests",
        Guid.NewGuid().ToString("N"),
        "Program Files");
    SettingsViewModel viewModel = CreateSettingsViewModelWithProjectDetector(
        AppSettings.CreateDefault(),
        new MicrosoftProjectDesktopDetector([missingRoot], pathDirectories: []));

    viewModel.DetectMicrosoftProjectDesktopAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal("Project not found", viewModel.MicrosoftProjectDesktopStatusText);
    Assert.Contains("common Microsoft Office install paths", viewModel.MicrosoftProjectDesktopStatusDetailText);
    Assert.Equal("#FFF1D6", viewModel.MicrosoftProjectDesktopStatusBackgroundColor);
    Assert.Equal("#946200", viewModel.MicrosoftProjectDesktopStatusAccentColor);
    Assert.True(viewModel.MicrosoftProjectDesktopNeedsAttention, "Missing Project desktop should stay visible.");
    Assert.True(viewModel.CanDetectMicrosoftProjectDesktop, "Missing Project desktop should remain recoverable by retrying detection.");
}

static void SettingsViewModelUpdatesProjectStatusWhenDisabled()
{
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbe(
        AppSettings.CreateDefault(),
        (_, _) => Task.FromResult(MicrosoftToDoConnectionStatus.Connected()));

    viewModel.EnableProjectDesktop = false;

    Assert.Equal("Project detection off", viewModel.MicrosoftProjectDesktopStatusText);
    Assert.Contains("turned off", viewModel.MicrosoftProjectDesktopStatusDetailText);
    Assert.Equal("#EEF0F3", viewModel.MicrosoftProjectDesktopStatusBackgroundColor);
    Assert.False(viewModel.CanDetectMicrosoftProjectDesktop, "Disabled Project desktop should not allow detection.");
    Assert.True(viewModel.HasChanges, "Changing the Project desktop switch should mark Settings dirty.");

    viewModel.EnableProjectDesktop = true;

    Assert.Equal("Ready to detect", viewModel.MicrosoftProjectDesktopStatusText);
    Assert.True(viewModel.CanDetectMicrosoftProjectDesktop, "Re-enabled Project desktop should allow detection.");
}

static void SettingsViewModelShowsProjectFileSelectionStatus()
{
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbe(
        AppSettings.CreateDefault(),
        (_, _) => Task.FromResult(MicrosoftToDoConnectionStatus.Connected()));

    Assert.Equal("No Project file selected", viewModel.MicrosoftProjectFileStatusText);
    Assert.Equal("None", viewModel.MicrosoftProjectFileName);
    Assert.Contains(".mpp", viewModel.MicrosoftProjectFileDetailText);
    Assert.Equal("#F4F7FB", viewModel.MicrosoftProjectFileStatusBackgroundColor);
    Assert.Equal("#4E5965", viewModel.MicrosoftProjectFileStatusAccentColor);
    Assert.True(viewModel.CanSelectMicrosoftProjectFile, "Enabled Project desktop should allow file selection.");
    Assert.False(viewModel.CanClearMicrosoftProjectFile, "No file should disable Clear.");
}

static void SettingsViewModelSelectsProjectFile()
{
    string projectFilePath = CreateFakeProjectFile();
    TestMicrosoftProjectFilePicker picker = new(projectFilePath);
    SettingsViewModel viewModel = CreateSettingsViewModelWithProjectFilePicker(
        AppSettings.CreateDefault(),
        picker);

    viewModel.SelectMicrosoftProjectFileAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal(1, picker.CallCount);
    Assert.Equal(projectFilePath, viewModel.MicrosoftProjectFilePath);
    Assert.Equal("Project file selected", viewModel.MicrosoftProjectFileStatusText);
    Assert.Equal(Path.GetFileName(projectFilePath), viewModel.MicrosoftProjectFileName);
    Assert.Contains(projectFilePath, viewModel.MicrosoftProjectFileDetailText);
    Assert.Equal("#E7F8EE", viewModel.MicrosoftProjectFileStatusBackgroundColor);
    Assert.Equal("#1E6B3A", viewModel.MicrosoftProjectFileStatusAccentColor);
    Assert.True(viewModel.CanClearMicrosoftProjectFile, "Selected Project file should enable Clear.");
    Assert.True(viewModel.HasChanges, "Selecting a Project file should mark Settings dirty.");
    Assert.Contains("Save settings", viewModel.SaveStatus);
}

static void SettingsViewModelClearsProjectFileSelection()
{
    AppSettings settings = AppSettings.CreateDefault();
    settings.ProjectDesktop.LocalProjectFilePath = CreateFakeProjectFile();
    SettingsViewModel viewModel = CreateSettingsViewModelWithProbe(
        settings,
        (_, _) => Task.FromResult(MicrosoftToDoConnectionStatus.Connected()));

    Assert.True(viewModel.HasMicrosoftProjectFileSelection, "Fixture should start with a selected file.");

    viewModel.ClearMicrosoftProjectFileAsync().GetAwaiter().GetResult();

    Assert.Equal(string.Empty, viewModel.MicrosoftProjectFilePath);
    Assert.Equal("No Project file selected", viewModel.MicrosoftProjectFileStatusText);
    Assert.Equal("None", viewModel.MicrosoftProjectFileName);
    Assert.False(viewModel.CanClearMicrosoftProjectFile, "Cleared Project file should disable Clear.");
    Assert.True(viewModel.HasChanges, "Clearing a Project file should mark Settings dirty.");
    Assert.Contains("cleared", viewModel.SaveStatus);
}

static NavigationService CreateShellNavigationService()
{
    NavigationService navigationService = new();
    navigationService.Register(AppScreen.Home, () => new HomeScreenViewModel(navigationService));
    navigationService.GoHomeAsync().GetAwaiter().GetResult();
    return navigationService;
}

static SettingsViewModel CreateSettingsViewModelWithProbe(
    AppSettings settings,
    Func<DateTimeOffset, CancellationToken, Task<MicrosoftToDoConnectionStatus>> testAsync)
{
    return CreateSettingsViewModelWithProbeInstance(
        settings,
        new TestMicrosoftToDoConnectionProbe(testAsync),
        DateTimeOffset.Now);
}

static SettingsViewModel CreateSettingsViewModelWithProjectDetector(
    AppSettings settings,
    MicrosoftProjectDesktopDetector detector)
{
    return CreateSettingsViewModelWithProbeInstance(
        settings,
        new TestMicrosoftToDoConnectionProbe((checkedAt, _) =>
            Task.FromResult(MicrosoftToDoConnectionStatus.Connected(checkedAt: checkedAt))),
        DateTimeOffset.Now,
        detector);
}

static SettingsViewModel CreateSettingsViewModelWithProjectFilePicker(
    AppSettings settings,
    IMicrosoftProjectFilePicker picker)
{
    return CreateSettingsViewModelWithProbeInstance(
        settings,
        new TestMicrosoftToDoConnectionProbe((checkedAt, _) =>
            Task.FromResult(MicrosoftToDoConnectionStatus.Connected(checkedAt: checkedAt))),
        DateTimeOffset.Now,
        projectFilePicker: picker);
}

static SettingsViewModel CreateSettingsViewModelWithProbeInstance(
    AppSettings settings,
    TestMicrosoftToDoConnectionProbe probe,
    DateTimeOffset now,
    MicrosoftProjectDesktopDetector? projectDetector = null,
    IMicrosoftProjectFilePicker? projectFilePicker = null)
{
    string settingsRoot = Path.Combine(
        Path.GetTempPath(),
        "SuccessPlannerMCP",
        "ViewModelTests",
        Guid.NewGuid().ToString("N"));
    SettingsService settingsService = new(new AppPaths(settingsRoot));
    MicrosoftToDoConnectionTestService connectionTestService = new(probe, () => now);

    return new SettingsViewModel(
        settingsService,
        settings,
        settingsFileStatus: "Loaded settings",
        microsoftToDoConnectionTestService: connectionTestService,
        microsoftProjectDesktopDetector: projectDetector,
        microsoftProjectFilePicker: projectFilePicker);
}

static string CreateFakeProjectDesktopInstall(out string executablePath)
{
    string programFilesRoot = Path.Combine(
        Path.GetTempPath(),
        "SuccessPlannerMCP",
        "ViewModelTests",
        Guid.NewGuid().ToString("N"),
        "Program Files");
    executablePath = Path.Combine(
        programFilesRoot,
        "Microsoft Office",
        "root",
        "Office16",
        MicrosoftProjectDesktopDetectionResult.ExecutableName);
    Directory.CreateDirectory(Path.GetDirectoryName(executablePath)!);
    File.WriteAllText(executablePath, "fake project executable");
    return programFilesRoot;
}

static string CreateFakeProjectFile()
{
    string filePath = Path.Combine(
        Path.GetTempPath(),
        "SuccessPlannerMCP",
        "ViewModelTests",
        Guid.NewGuid().ToString("N"),
        "Personal Success Plan.mpp");
    Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
    File.WriteAllText(filePath, "fake project file");
    return filePath;
}

static void CaptureViewModelStartsReady()
{
    CaptureViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Capture, viewModel.Descriptor);
    Assert.Equal("Capture", viewModel.Title);
    Assert.Equal("Add the thought before it escapes.", viewModel.Subtitle);
    Assert.Equal("\uE710", viewModel.IconGlyph);
    Assert.Equal("#9DCCFF", viewModel.AccentColor);
    Assert.Equal(string.Empty, viewModel.TaskTitle);
    Assert.Equal(string.Empty, viewModel.Notes);
    Assert.Equal(string.Empty, viewModel.ValidationMessage);
    Assert.Equal("Ready to capture.", viewModel.StatusText);
    Assert.Null(viewModel.DueDate, "New capture should not have a date selected.");
    Assert.Equal("No date selected.", viewModel.DateHintText);
    Assert.Equal(CaptureDestinationPreference.LetMcpChoose, viewModel.SelectedDestination);
    Assert.Equal("Let MCP Choose.", viewModel.DestinationHintText);
    Assert.False(viewModel.CanCreateTask, "Blank capture title should not be ready.");
    Assert.False(viewModel.SaveTaskCommand.CanExecute(null), "Blank capture title should disable save.");
    Assert.False(viewModel.CaptureAnotherCommand.CanExecute(null), "Capture Another should wait for a saved task.");
    Assert.Equal("No task saved yet.", viewModel.SuccessFeedbackText);
}

static void CaptureViewModelAppliesDateHintButtons()
{
    CaptureViewModel viewModel = new();
    DateOnly today = DateOnly.FromDateTime(DateTime.Today);

    viewModel.TodayDateCommand.Execute(null);
    Assert.Equal(today, viewModel.DueDate);
    Assert.Contains("Today", viewModel.DateHintText);

    viewModel.TomorrowDateCommand.Execute(null);
    Assert.Equal(today.AddDays(1), viewModel.DueDate);
    Assert.Contains("Tomorrow", viewModel.DateHintText);

    viewModel.ThisWeekDateCommand.Execute(null);
    Assert.Equal(today.AddDays(7), viewModel.DueDate);
    Assert.Contains("This week", viewModel.DateHintText);

    viewModel.ClearDateCommand.Execute(null);
    Assert.Null(viewModel.DueDate, "No Date should clear the selected date.");
    Assert.Equal("No date selected.", viewModel.DateHintText);
}

static void CaptureViewModelAppliesDestinationChoices()
{
    CaptureViewModel viewModel = new();

    viewModel.LocalInboxDestinationCommand.Execute(null);
    Assert.Equal(CaptureDestinationPreference.LocalInbox, viewModel.SelectedDestination);
    Assert.Equal("Local.", viewModel.DestinationHintText);
    Assert.Equal("Destination set to Local.", viewModel.StatusText);

    viewModel.MicrosoftToDoDestinationCommand.Execute(null);
    Assert.Equal(CaptureDestinationPreference.MicrosoftToDo, viewModel.SelectedDestination);
    Assert.Equal("To Do.", viewModel.DestinationHintText);

    viewModel.MicrosoftPlannerDestinationCommand.Execute(null);
    Assert.Equal(CaptureDestinationPreference.MicrosoftPlanner, viewModel.SelectedDestination);
    Assert.Equal("Planner.", viewModel.DestinationHintText);

    viewModel.MicrosoftProjectDestinationCommand.Execute(null);
    Assert.Equal(CaptureDestinationPreference.MicrosoftProject, viewModel.SelectedDestination);
    Assert.Equal("Project.", viewModel.DestinationHintText);

    viewModel.LetMcpChooseDestinationCommand.Execute(null);
    Assert.Equal(CaptureDestinationPreference.LetMcpChoose, viewModel.SelectedDestination);
    Assert.Equal("Let MCP Choose.", viewModel.DestinationHintText);
}

static void CaptureViewModelValidatesEmptyTitle()
{
    CaptureViewModel viewModel = new();

    bool created = viewModel.TryCreateCapturedTask(out TaskItem? task);

    Assert.False(created, "Blank capture should not create a task.");
    Assert.Null(task, "Blank capture should not return a task.");
    Assert.Equal("Add one small action first.", viewModel.ValidationMessage);
    Assert.Equal("Capture needs a task title.", viewModel.StatusText);
}

static void CaptureViewModelCreatesCapturedTaskDraft()
{
    CaptureViewModel viewModel = new()
    {
        TaskTitle = "  Draft the capture screen  ",
        Notes = "  Keep it child-simple.  "
    };

    bool created = viewModel.TryCreateCapturedTask(out TaskItem? task);

    Assert.True(created, "Valid capture should create a task draft.");
    Assert.NotNull(task, "Valid capture should return a task.");
    Assert.Equal("Draft the capture screen", task!.Title);
    Assert.Equal("Keep it child-simple.", task.Notes);
    Assert.Equal(TaskItemStatus.Captured, task.Status);
    Assert.Equal(TaskPriority.Normal, task.Priority);
    Assert.Equal(string.Empty, viewModel.ValidationMessage);
    Assert.Equal("Task ready to save.", viewModel.StatusText);
}

static void CaptureViewModelCreatesScheduledTaskDraftWhenDateIsSelected()
{
    CaptureViewModel viewModel = new()
    {
        TaskTitle = "Make a phone call"
    };
    viewModel.TomorrowDateCommand.Execute(null);

    bool created = viewModel.TryCreateCapturedTask(out TaskItem? task);

    Assert.True(created, "Valid capture should create a task draft.");
    Assert.NotNull(task, "Valid capture should return a task.");
    Assert.Equal(DateOnly.FromDateTime(DateTime.Today).AddDays(1), task!.DueDate);
    Assert.Equal(TaskItemStatus.Planned, task.Status);
}

static void CaptureViewModelSavesCapturedTaskLocally()
{
    List<TaskItem> savedTasks = [];
    CaptureViewModel viewModel = new((task, _) =>
    {
        savedTasks.Add(task);
        return Task.CompletedTask;
    })
    {
        TaskTitle = "  Save this locally  ",
        Notes = "  Local first.  "
    };
    viewModel.TodayDateCommand.Execute(null);

    viewModel.SaveCapturedTaskAsync().GetAwaiter().GetResult();

    Assert.Equal(1, savedTasks.Count);
    Assert.Equal("Save this locally", savedTasks[0].Title);
    Assert.Equal("Local first.", savedTasks[0].Notes);
    Assert.Equal(DateOnly.FromDateTime(DateTime.Today), savedTasks[0].DueDate);
    Assert.Equal(TaskItemStatus.Planned, savedTasks[0].Status);
    Assert.True(viewModel.HasSavedTask, "Successful save should mark the current task saved.");
    Assert.Equal(savedTasks[0].Id, viewModel.LastSavedTaskId);
    Assert.Equal("Saved locally.", viewModel.StatusText);
    Assert.Equal("Saved locally: Save this locally", viewModel.SuccessFeedbackText);
    Assert.False(viewModel.SaveTaskCommand.CanExecute(null), "Saved task should not save again until changed.");
    Assert.True(viewModel.CaptureAnotherCommand.CanExecute(null), "Successful save should enable Capture Another.");

    viewModel.TaskTitle = "Changed after save";
    Assert.False(viewModel.HasSavedTask, "Editing after save should clear saved state.");
    Assert.Equal("No task saved yet.", viewModel.SuccessFeedbackText);
    Assert.True(viewModel.SaveTaskCommand.CanExecute(null), "Edited task should be saveable again.");
}

static void CaptureViewModelCapturesAnotherTaskAfterSuccess()
{
    CaptureViewModel viewModel = new((_, _) => Task.CompletedTask)
    {
        TaskTitle = "Saved task",
        Notes = "Keep the loop simple."
    };
    viewModel.TodayDateCommand.Execute(null);
    viewModel.MicrosoftToDoDestinationCommand.Execute(null);
    viewModel.SaveCapturedTaskAsync().GetAwaiter().GetResult();

    viewModel.CaptureAnotherCommand.Execute(null);

    Assert.Equal(string.Empty, viewModel.TaskTitle);
    Assert.Equal(string.Empty, viewModel.Notes);
    Assert.Null(viewModel.DueDate, "Capture Another should clear the selected date.");
    Assert.Equal(CaptureDestinationPreference.LetMcpChoose, viewModel.SelectedDestination);
    Assert.Equal("No task saved yet.", viewModel.SuccessFeedbackText);
    Assert.False(viewModel.HasSavedTask, "Capture Another should clear saved state.");
    Assert.False(viewModel.CaptureAnotherCommand.CanExecute(null), "Capture Another should disable until the next save.");
    Assert.Equal("Ready to capture.", viewModel.StatusText);
}

static void CaptureViewModelResetsCaptureForm()
{
    CaptureViewModel viewModel = new()
    {
        TaskTitle = "Plan the tiny step",
        Notes = "Notes"
    };
    viewModel.TodayDateCommand.Execute(null);
    viewModel.TryCreateCapturedTask(out _);

    viewModel.ResetCaptureForm();

    Assert.Equal(string.Empty, viewModel.TaskTitle);
    Assert.Equal(string.Empty, viewModel.Notes);
    Assert.Equal(string.Empty, viewModel.ValidationMessage);
    Assert.Equal("Ready to capture.", viewModel.StatusText);
    Assert.Null(viewModel.DueDate, "Reset should clear the selected date.");
    Assert.Equal("No date selected.", viewModel.DateHintText);
    Assert.Equal(CaptureDestinationPreference.LetMcpChoose, viewModel.SelectedDestination);
    Assert.Equal("Let MCP Choose.", viewModel.DestinationHintText);
    Assert.False(viewModel.HasSavedTask, "Reset should clear saved state.");
    Assert.Null(viewModel.LastSavedTaskId, "Reset should clear the last saved task id.");
    Assert.False(viewModel.CanCreateTask, "Reset form should not be ready to create a task.");
}

static void CaptureViewModelRaisesPropertyChangeNotifications()
{
    CaptureViewModel viewModel = new();
    List<string> changedProperties = [];
    viewModel.PropertyChanged += (_, args) =>
    {
        if (args.PropertyName is not null)
        {
            changedProperties.Add(args.PropertyName);
        }
    };

    viewModel.TaskTitle = "One small action";

    Assert.Contains(nameof(CaptureViewModel.TaskTitle), changedProperties);
    Assert.Contains(nameof(CaptureViewModel.CanCreateTask), changedProperties);
    Assert.True(viewModel.CanCreateTask, "Nonblank title should be ready to create a task.");
    Assert.True(viewModel.SaveTaskCommand.CanExecute(null), "Nonblank title should enable save.");
}

static void TodayViewModelStartsReady()
{
    TodayViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Today, viewModel.Descriptor);
    Assert.Equal("Today", viewModel.Title);
    Assert.Equal("See what matters now.", viewModel.Subtitle);
    Assert.Equal("\uE787", viewModel.IconGlyph);
    Assert.Equal("#A8E6B1", viewModel.AccentColor);
    Assert.Equal("Ready to load today.", viewModel.StatusText);
    Assert.Equal("No tasks due today.", viewModel.EmptyStateText);
    Assert.Equal("0 tasks", viewModel.TaskCountText);
    Assert.False(viewModel.HasTasks, "Today should start with no loaded tasks.");
    Assert.False(viewModel.IsLoading, "Today should not start in a loading state.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available when Today is idle.");
    Assert.Equal(TodayTaskAction.None, viewModel.SelectedAction);
    Assert.Null(viewModel.SelectedTaskId, "No task should start selected.");
    Assert.Equal("No task selected.", viewModel.SelectedTaskTitle);
    Assert.Equal("Choose One", viewModel.ActionPanelTitle);
    Assert.False(viewModel.IsNoteActionSelected, "Note composer should start hidden.");
}

static void TodayViewModelLoadsTodayTasks()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem overdue = CreateTask("Call the pharmacy", dueDate: today.AddDays(-1));
    TaskItem dueToday = CreateTask("Pay the bill", dueDate: today, priority: TaskPriority.High);
    TaskItem selectedToday = CreateTask("Draft next plan", dueDate: today.AddDays(5), startDate: today);
    TaskItem inProgress = CreateTask("Finish active task", inProgress: true);
    TaskItem future = CreateTask("Future task", dueDate: today.AddDays(1));
    TaskItem doneToday = CreateTask("Already done", dueDate: today, done: true);
    DateOnly? requestedToday = null;
    TodayViewModel viewModel = new(
        (todayToLoad, _) =>
        {
            requestedToday = todayToLoad;
            return Task.FromResult<IReadOnlyList<TaskItem>>(
                [future, selectedToday, doneToday, dueToday, inProgress, overdue]);
        },
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.True(requestedToday.HasValue, "Today load should pass the current date to its loader.");
    Assert.Equal(today, requestedToday.GetValueOrDefault());
    Assert.Equal(4, viewModel.Tasks.Count);
    Assert.Equal(4, viewModel.TaskCards.Count);
    Assert.Equal("Call the pharmacy", viewModel.Tasks[0].Title);
    Assert.True(viewModel.Tasks[0].IsOverdue, "Overdue task should be marked overdue.");
    Assert.Equal("Pay the bill", viewModel.Tasks[1].Title);
    Assert.True(viewModel.Tasks[1].IsDueToday, "Due-today task should be marked due today.");
    Assert.Equal("Draft next plan", viewModel.Tasks[2].Title);
    Assert.True(viewModel.Tasks[2].IsSelectedForToday, "Start-date task should be selected for today.");
    Assert.Equal("Finish active task", viewModel.Tasks[3].Title);
    Assert.True(viewModel.Tasks[3].IsInProgress, "In-progress task should stay visible.");
    Assert.False(viewModel.Tasks.Any(task => task.Title == "Future task"), "Future task should not load into Today.");
    Assert.False(viewModel.Tasks.Any(task => task.Title == "Already done"), "Done task should not load into Today.");
    Assert.True(viewModel.HasTasks, "Loaded Today tasks should set HasTasks.");
    Assert.Equal("4 tasks", viewModel.TaskCountText);
    Assert.Equal("Today is ready.", viewModel.StatusText);
    Assert.Equal("Choose one small action.", viewModel.EmptyStateText);
}

static void TodayViewModelCreatesTaskCardDisplayState()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem overdue = CreateTask("Call the pharmacy", dueDate: today.AddDays(-1), priority: TaskPriority.Critical);
    overdue.UpdateNotes("Ask about the refill, bring the glucose log, and keep the call under ten minutes.");

    TodayTaskCardViewModel overdueCard = TodayTaskCardViewModel.FromTask(overdue, today);

    Assert.Equal("Call the pharmacy", overdueCard.Title);
    Assert.True(overdueCard.HasNotes, "Task card should show notes when notes exist.");
    Assert.Contains("glucose log", overdueCard.NotesPreview);
    Assert.Equal("Overdue", overdueCard.DueBadgeText);
    Assert.Contains("Overdue", overdueCard.DueText);
    Assert.Equal("Critical", overdueCard.PriorityBadgeText);
    Assert.Equal("Planned", overdueCard.StatusBadgeText);
    Assert.Equal("#FFBE7A", overdueCard.CardAccentColor);
    Assert.Equal("\uE823", overdueCard.CardIconGlyph);
    Assert.Contains("Call the pharmacy", overdueCard.CardToolTip);

    TaskItem selectedToday = CreateTask("Sketch the next panel", startDate: today);
    TodayTaskCardViewModel selectedCard = TodayTaskCardViewModel.FromTask(selectedToday, today);

    Assert.False(selectedCard.HasNotes, "Task card should hide blank notes.");
    Assert.Equal(string.Empty, selectedCard.NotesPreview);
    Assert.Equal("Selected today", selectedCard.DueBadgeText);
    Assert.Equal("#EAF4FF", selectedCard.DueBadgeBackground);
}

static void TodayViewModelHandlesTaskCardActions()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem startTask = CreateTask("Start the sketch", dueDate: today);
    TaskItem doneTask = CreateTask("Finish the sketch", dueDate: today);
    TaskItem snoozeTask = CreateTask("Call the printer", dueDate: today);
    TaskItem noteTask = CreateTask("Choose calmer color set", dueDate: today);
    noteTask.UpdateNotes("Use the calmer color set.");
    List<(Guid Id, TaskItemStatus Status, DateOnly? DueDate, DateOnly? StartDate, string Notes)> savedTasks = [];
    TodayViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([startTask, doneTask, snoozeTask, noteTask]),
        (task, _) =>
        {
            savedTasks.Add((task.Id, task.Status, task.DueDate, task.StartDate, task.Notes));
            return Task.CompletedTask;
        },
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    TodayTaskCardViewModel startCard = viewModel.TaskCards.First(card => card.Id == startTask.Id);

    Assert.True(startCard.StartCommand.CanExecute(null), "Start should be available on Today cards.");
    Assert.True(startCard.DoneCommand.CanExecute(null), "Done should be available on Today cards.");
    Assert.True(startCard.SnoozeCommand.CanExecute(null), "Snooze should be available on Today cards.");
    Assert.True(startCard.NoteCommand.CanExecute(null), "Note should be available on Today cards.");

    viewModel.ExecuteTaskActionAsync(startCard, TodayTaskAction.Start).GetAwaiter().GetResult();

    Assert.Equal(TodayTaskAction.Start, viewModel.SelectedAction);
    Assert.Equal(startTask.Id, viewModel.SelectedTaskId.GetValueOrDefault());
    Assert.Equal("Start the sketch", viewModel.SelectedTaskTitle);
    Assert.Equal("Start", viewModel.ActionPanelTitle);
    Assert.Contains("started locally", viewModel.ActionPanelText);
    Assert.Equal("Start saved locally.", viewModel.StatusText);
    Assert.False(viewModel.IsNoteActionSelected, "Start should not show the note composer.");
    Assert.Equal(TaskItemStatus.InProgress, startTask.Status);
    Assert.Equal(TaskItemStatus.InProgress, savedTasks[^1].Status);
    Assert.Equal("In progress", viewModel.TaskCards.First(card => card.Id == startTask.Id).StatusText);

    TodayTaskCardViewModel doneCard = viewModel.TaskCards.First(card => card.Id == doneTask.Id);
    viewModel.ExecuteTaskActionAsync(doneCard, TodayTaskAction.Done).GetAwaiter().GetResult();
    Assert.Equal(TodayTaskAction.Done, viewModel.SelectedAction);
    Assert.Equal("Done", viewModel.ActionPanelTitle);
    Assert.Equal("Done saved locally.", viewModel.StatusText);
    Assert.Equal(TaskItemStatus.Done, doneTask.Status);
    Assert.Equal(TaskItemStatus.Done, savedTasks[^1].Status);
    Assert.False(viewModel.TaskCards.Any(card => card.Id == doneTask.Id), "Done task should leave Today cards.");

    TodayTaskCardViewModel snoozeCard = viewModel.TaskCards.First(card => card.Id == snoozeTask.Id);
    viewModel.ExecuteTaskActionAsync(snoozeCard, TodayTaskAction.Snooze).GetAwaiter().GetResult();
    Assert.Equal(TodayTaskAction.Snooze, viewModel.SelectedAction);
    Assert.Equal("Snooze", viewModel.ActionPanelTitle);
    Assert.Equal("#FFBE7A", viewModel.ActionPanelAccentColor);
    Assert.Equal("Snooze saved locally.", viewModel.StatusText);
    Assert.Equal(today.AddDays(1), snoozeTask.DueDate);
    Assert.Equal(today.AddDays(1), snoozeTask.StartDate);
    Assert.Equal(today.AddDays(1), savedTasks[^1].DueDate);
    Assert.False(viewModel.TaskCards.Any(card => card.Id == snoozeTask.Id), "Snoozed task should leave Today cards.");

    TodayTaskCardViewModel noteCard = viewModel.TaskCards.First(card => card.Id == noteTask.Id);
    viewModel.ExecuteTaskActionAsync(noteCard, TodayTaskAction.Note).GetAwaiter().GetResult();
    Assert.Equal(TodayTaskAction.Note, viewModel.SelectedAction);
    Assert.Equal("Note", viewModel.ActionPanelTitle);
    Assert.True(viewModel.IsNoteActionSelected, "Note should show the note composer.");
    Assert.Equal("Use the calmer color set.", viewModel.NoteDraft);
    Assert.Equal("Note ready.", viewModel.StatusText);
    Assert.True(viewModel.SaveNoteCommand.CanExecute(null), "Note should enable Save Note.");

    viewModel.NoteDraft = "Remember the calmer color set.";
    viewModel.SaveSelectedNoteAsync().GetAwaiter().GetResult();

    Assert.Equal("Remember the calmer color set.", viewModel.NoteDraft);
    Assert.Equal("Remember the calmer color set.", noteTask.Notes);
    Assert.Equal("Remember the calmer color set.", savedTasks[^1].Notes);
    Assert.Equal("Note saved locally.", viewModel.StatusText);
    Assert.Equal("Remember the calmer color set.", viewModel.TaskCards.First(card => card.Id == noteTask.Id).NotesPreview);
}

static void TodayViewModelShowsEmptyTodayState()
{
    DateOnly today = new(2026, 5, 30);
    TodayViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([CreateTask("Future task", dueDate: today.AddDays(1))]),
        () => today);

    viewModel.OnNavigatedToAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.False(viewModel.HasTasks, "Future-only task list should leave Today empty.");
    Assert.Equal("0 tasks", viewModel.TaskCountText);
    Assert.Equal("Today is clear.", viewModel.StatusText);
    Assert.Contains("No tasks due today", viewModel.EmptyStateText);
}

static void TodayViewModelReportsLoadFailures()
{
    DateOnly today = new(2026, 5, 30);
    TodayViewModel viewModel = new(
        _ => throw new InvalidOperationException("Task storage unavailable."),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasTasks, "Failed load should not leave stale tasks visible.");
    Assert.Equal("0 tasks", viewModel.TaskCountText);
    Assert.Equal("Today could not load.", viewModel.StatusText);
    Assert.Contains("Try Refresh", viewModel.EmptyStateText);
    Assert.False(viewModel.IsLoading, "Loading flag should clear after failure.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available after failure.");
}

static void DoneViewModelStartsReady()
{
    DoneViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Done, viewModel.Descriptor);
    Assert.Equal("Done", viewModel.Title);
    Assert.Equal("Record the win.", viewModel.Subtitle);
    Assert.Equal("\uE73E", viewModel.IconGlyph);
    Assert.Equal("#DADDE2", viewModel.AccentColor);
    Assert.Equal("Ready to choose a win.", viewModel.StatusText);
    Assert.Equal("No active tasks ready to finish.", viewModel.EmptyStateText);
    Assert.Equal("0 tasks", viewModel.TaskCountText);
    Assert.Equal("No task selected.", viewModel.SelectedTaskTitle);
    Assert.Equal("Choose a recent active task, then mark it complete.", viewModel.CompletionPanelText);
    Assert.Null(viewModel.LastSmallWinNoteId, "Done should start without a small win note.");
    Assert.Equal("No small win recorded yet.", viewModel.LastSmallWinText);
    Assert.False(viewModel.HasSuccessFeedback, "Done should start without success feedback visible.");
    Assert.Equal("Small Win Recorded", viewModel.SuccessFeedbackTitle);
    Assert.Equal("Complete a task to see the win here.", viewModel.SuccessFeedbackText);
    Assert.False(viewModel.HasTasks, "Done should start with no loaded tasks.");
    Assert.False(viewModel.IsLoading, "Done should not start in a loading state.");
    Assert.False(viewModel.IsCompleting, "Done should not start in a completing state.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available when Done is idle.");
}

static void DoneViewModelLoadsRecentActiveTasks()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem captured = CreateTask("Quick captured thought");
    TaskItem planned = CreateTask("Pay the bill", dueDate: today, priority: TaskPriority.High);
    TaskItem selectedYesterday = CreateTask("Review notes", dueDate: today.AddDays(5), startDate: today.AddDays(-1));
    TaskItem inProgress = CreateTask("Finish active task", inProgress: true);
    TaskItem blocked = CreateTask("Resolve blocked item");
    blocked.MarkBlocked();
    TaskItem oldOverdue = CreateTask("Old stale task", dueDate: today.AddDays(-15));
    TaskItem future = CreateTask("Future task", dueDate: today.AddDays(1));
    TaskItem alreadyDone = CreateTask("Already complete", done: true);
    DoneViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>(
            [captured, alreadyDone, planned, selectedYesterday, blocked, inProgress, oldOverdue, future]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.Equal(4, viewModel.Tasks.Count);
    Assert.Equal(4, viewModel.TaskCards.Count);
    Assert.Equal("Finish active task", viewModel.Tasks[0].Title);
    Assert.True(viewModel.Tasks[0].IsInProgress, "In-progress task should sort first.");
    Assert.Equal("Pay the bill", viewModel.Tasks[1].Title);
    Assert.True(viewModel.Tasks[1].IsDueToday, "Due-today task should be identified.");
    Assert.Equal("Review notes", viewModel.Tasks[2].Title);
    Assert.Equal("Resolve blocked item", viewModel.Tasks[3].Title);
    Assert.True(viewModel.Tasks[3].IsBlocked, "Blocked task should keep visible status.");
    Assert.False(viewModel.Tasks.Any(task => task.Title == "Quick captured thought"), "Loose captures should not load into recent active tasks.");
    Assert.False(viewModel.Tasks.Any(task => task.Title == "Old stale task"), "Older inactive tasks should not load into recent active tasks.");
    Assert.False(viewModel.Tasks.Any(task => task.Title == "Future task"), "Future tasks should not load into recent active tasks.");
    Assert.False(viewModel.Tasks.Any(task => task.Title == "Already complete"), "Done tasks should not load into Done candidates.");
    Assert.True(viewModel.HasTasks, "Loaded Done tasks should set HasTasks.");
    Assert.Equal("4 tasks", viewModel.TaskCountText);
    Assert.Equal("Choose one task to complete.", viewModel.StatusText);
    Assert.Equal("Pick one finished action and record the win.", viewModel.EmptyStateText);
    Assert.True(viewModel.Tasks[0].CompleteCommand.CanExecute(null), "Complete should be available on Done task cards.");
}

static void DoneViewModelCreatesTaskCardDisplayState()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem overdue = CreateTask("Call the pharmacy", dueDate: today.AddDays(-1), priority: TaskPriority.Critical);
    overdue.UpdateNotes("Ask about the refill, bring the glucose log, and keep the call under ten minutes.");

    DoneTaskCardViewModel overdueCard = DoneTaskCardViewModel.FromTask(overdue, today);

    Assert.Equal("Call the pharmacy", overdueCard.Title);
    Assert.True(overdueCard.HasNotes, "Done card should show notes when notes exist.");
    Assert.Contains("glucose log", overdueCard.NotesPreview);
    Assert.Equal("Overdue", overdueCard.DueBadgeText);
    Assert.Contains("Overdue", overdueCard.DueText);
    Assert.Equal("Critical", overdueCard.PriorityBadgeText);
    Assert.Equal("Planned", overdueCard.StatusBadgeText);
    Assert.Equal("#FFBE7A", overdueCard.CardAccentColor);
    Assert.Equal("\uE823", overdueCard.CardIconGlyph);
    Assert.Contains("Call the pharmacy", overdueCard.CardToolTip);

    TaskItem active = CreateTask("Finish the active item", inProgress: true);
    DoneTaskCardViewModel activeCard = DoneTaskCardViewModel.FromTask(active, today);

    Assert.Equal("In progress", activeCard.StatusBadgeText);
    Assert.Equal("#8DDAD5", activeCard.CardAccentColor);
    Assert.Equal("\uE768", activeCard.CardIconGlyph);
    Assert.True(activeCard.CompleteCommand.CanExecute(null), "Done card should expose a Complete command.");
}

static void DoneViewModelCompletesSelectedTask()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem completedTask = CreateTask("Finish active task", inProgress: true);
    TaskItem remainingTask = CreateTask("Pay the bill", dueDate: today);
    List<(Guid Id, TaskItemStatus Status, DateTimeOffset? CompletedAt)> savedTasks = [];
    List<NoteItem> smallWins = [];
    DoneViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([completedTask, remainingTask]),
        (task, _) =>
        {
            savedTasks.Add((task.Id, task.Status, task.CompletedAt));
            return Task.CompletedTask;
        },
        (task, _) =>
        {
            NoteItem smallWin = NoteItem.Create(NoteOwnerType.Task, task.Id, $"Small win: {task.Title}");
            smallWin.MarkReviewHighlight();
            smallWin.AddTag("Win");
            smallWin.AddTag("Small Win");
            smallWins.Add(smallWin);
            return Task.FromResult(smallWin);
        },
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    DoneTaskCardViewModel card = viewModel.TaskCards.First(task => task.Id == completedTask.Id);

    viewModel.CompleteSelectedTaskAsync(card).GetAwaiter().GetResult();

    Assert.Equal(completedTask.Id, viewModel.SelectedTaskId.GetValueOrDefault());
    Assert.Equal("Finish active task", viewModel.SelectedTaskTitle);
    Assert.Equal("Small win recorded locally.", viewModel.StatusText);
    Assert.Contains("recorded as a small win", viewModel.CompletionPanelText);
    Assert.Equal(TaskItemStatus.Done, completedTask.Status);
    Assert.True(completedTask.CompletedAt.HasValue, "Completing selected task should stamp a completed time.");
    Assert.Equal(1, savedTasks.Count);
    Assert.Equal(completedTask.Id, savedTasks[0].Id);
    Assert.Equal(TaskItemStatus.Done, savedTasks[0].Status);
    Assert.True(savedTasks[0].CompletedAt.HasValue, "Completed task should be saved with a completed time.");
    Assert.Equal(1, smallWins.Count);
    Assert.Equal(NoteOwnerType.Task, smallWins[0].OwnerType);
    Assert.Equal(completedTask.Id, smallWins[0].OwnerId.GetValueOrDefault());
    Assert.True(smallWins[0].IsReviewHighlight, "Small win should be visible to Review.");
    Assert.Contains("Win", smallWins[0].Tags);
    Assert.Contains("Small Win", smallWins[0].Tags);
    Assert.Equal(smallWins[0].Id, viewModel.LastSmallWinNoteId.GetValueOrDefault());
    Assert.Equal("Small win: Finish active task", viewModel.LastSmallWinText);
    Assert.False(viewModel.TaskCards.Any(task => task.Id == completedTask.Id), "Completed task should leave Done task cards.");
    Assert.Equal(1, viewModel.TaskCards.Count);
    Assert.Equal("1 task", viewModel.TaskCountText);
    Assert.True(viewModel.HasTasks, "Remaining task should keep Done non-empty.");
    Assert.False(viewModel.IsCompleting, "Completing flag should clear after save.");
}

static void DoneViewModelShowsBriefSuccessFeedback()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem completedTask = CreateTask("Finish active task", inProgress: true);
    DoneViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([completedTask]),
        (_, _) => Task.CompletedTask,
        (task, _) =>
        {
            NoteItem smallWin = NoteItem.Create(NoteOwnerType.Task, task.Id, $"Small win: {task.Title}");
            smallWin.MarkReviewHighlight();
            smallWin.AddTag("Win");
            smallWin.AddTag("Small Win");
            return Task.FromResult(smallWin);
        },
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.CompleteSelectedTaskAsync(viewModel.TaskCards[0]).GetAwaiter().GetResult();

    Assert.True(viewModel.HasSuccessFeedback, "Completing a task should show brief success feedback.");
    Assert.Equal("Small Win Recorded", viewModel.SuccessFeedbackTitle);
    Assert.Contains("Finish active task", viewModel.SuccessFeedbackText);
    Assert.Contains("Small win: Finish active task", viewModel.SuccessFeedbackText);

    viewModel.DismissSuccessFeedback();

    Assert.False(viewModel.HasSuccessFeedback, "Dismiss should hide brief success feedback.");
}

static void DoneViewModelShowsEmptyDoneState()
{
    DoneViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([CreateTask("Already done", done: true)]),
        () => new DateOnly(2026, 5, 30));

    viewModel.OnNavigatedToAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.False(viewModel.HasTasks, "Done-only task list should leave Done candidates empty.");
    Assert.Equal("0 tasks", viewModel.TaskCountText);
    Assert.Equal("No active tasks ready.", viewModel.StatusText);
    Assert.Contains("Recent active tasks", viewModel.EmptyStateText);
}

static void DoneViewModelReportsLoadFailures()
{
    DoneViewModel viewModel = new(
        _ => throw new InvalidOperationException("Task storage unavailable."),
        () => new DateOnly(2026, 5, 30));

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasTasks, "Failed load should not leave stale tasks visible.");
    Assert.Equal("0 tasks", viewModel.TaskCountText);
    Assert.Equal("Done could not load.", viewModel.StatusText);
    Assert.Contains("Try Refresh", viewModel.EmptyStateText);
    Assert.False(viewModel.IsLoading, "Loading flag should clear after failure.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available after failure.");
}

static void StartWorkViewModelStartsReady()
{
    StartWorkViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.StartWork, viewModel.Descriptor);
    Assert.Equal("Start", viewModel.Title);
    Assert.Equal("Begin a short focus session.", viewModel.Subtitle);
    Assert.Equal("\uE768", viewModel.IconGlyph);
    Assert.Equal("#8DDAD5", viewModel.AccentColor);
    Assert.Equal("Ready to choose focus.", viewModel.StatusText);
    Assert.Equal("No focus options loaded.", viewModel.EmptyStateText);
    Assert.Equal("0 options", viewModel.TaskCountText);
    Assert.Equal("No focus selected.", viewModel.SelectedTaskTitle);
    Assert.Equal("Choose Focus", viewModel.FocusPanelTitle);
    Assert.Contains("20 minute", viewModel.FocusPanelText);
    Assert.Equal("Choose one small action.", viewModel.FocusIntention);
    Assert.False(viewModel.HasSuggestedTask, "Start should begin without a best-next suggestion.");
    Assert.Equal("No suggestion yet.", viewModel.SuggestedTaskTitle);
    Assert.Equal("Best Next", viewModel.SuggestionPanelTitle);
    Assert.Equal("Waiting", viewModel.SuggestionBadgeText);
    Assert.Contains("Load focus options", viewModel.SuggestionPanelText);
    Assert.Equal("No focus options loaded.", viewModel.SuggestionReasonText);
    Assert.Equal(FocusSession.DefaultPlannedMinutes, viewModel.PlannedMinutes);
    Assert.Equal("20 minute focus", viewModel.PlannedMinutesText);
    Assert.Equal("20 minute session selected.", viewModel.SessionChoiceSummaryText);
    Assert.False(viewModel.IsTenMinuteSessionSelected, "Ten minute session should not start selected.");
    Assert.False(viewModel.IsFifteenMinuteSessionSelected, "Fifteen minute session should not start selected.");
    Assert.True(viewModel.IsTwentyMinuteSessionSelected, "Twenty minute session should start selected.");
    Assert.Null(viewModel.ActiveFocusSessionId, "Start should begin without a focus session.");
    Assert.Null(viewModel.ActiveFocusSessionTaskId, "Start should begin without a session task.");
    Assert.False(viewModel.HasFocusSession, "Start should not show a session before the user starts one.");
    Assert.False(viewModel.HasActiveFocusSession, "Start should not show an active session before the user starts one.");
    Assert.Equal("No active session.", viewModel.FocusSessionStatusText);
    Assert.Equal("Ready Timer", viewModel.FocusSessionPanelTitle);
    Assert.Equal("Ready", viewModel.FocusSessionBadgeText);
    Assert.Equal("20:00 planned", viewModel.FocusTimerText);
    Assert.Contains("Select one focus option", viewModel.FocusSessionPanelText);
    Assert.Equal("Session not saved yet.", viewModel.FocusSessionStorageText);
    Assert.False(viewModel.HasSavedFocusSession, "Start should not begin with a saved focus session.");
    Assert.Null(viewModel.LastSavedFocusSessionId, "Start should not begin with a saved focus session id.");
    Assert.False(viewModel.HasBreakSuggestion, "Start should not begin with a break suggestion.");
    Assert.Equal("Break After Focus", viewModel.BreakSuggestionTitle);
    Assert.Equal("Waiting", viewModel.BreakSuggestionBadgeText);
    Assert.Equal("Break pending", viewModel.BreakSuggestionDurationText);
    Assert.Contains("Complete a focus session", viewModel.BreakSuggestionText);
    Assert.Contains("Finish the current focus block", viewModel.BreakSuggestionActionText);
    Assert.False(viewModel.CanStartFocus, "Start Focus should wait for a selected task.");
    Assert.False(viewModel.StartFocusCommand.CanExecute(null), "Start Focus command should wait for a selected task.");
    Assert.False(viewModel.PauseFocusCommand.CanExecute(null), "Pause should wait for a running session.");
    Assert.False(viewModel.ResumeFocusCommand.CanExecute(null), "Resume should wait for a paused session.");
    Assert.False(viewModel.CompleteFocusCommand.CanExecute(null), "Complete should wait for an active session.");
    Assert.False(viewModel.BlockFocusCommand.CanExecute(null), "Blocked should wait for an active session.");
    Assert.True(viewModel.ChooseTenMinuteSessionCommand.CanExecute(null), "Ten minute choice should be available.");
    Assert.True(viewModel.ChooseFifteenMinuteSessionCommand.CanExecute(null), "Fifteen minute choice should be available.");
    Assert.True(viewModel.ChooseTwentyMinuteSessionCommand.CanExecute(null), "Twenty minute choice should be available.");
    Assert.False(viewModel.HasTaskOptions, "Start should begin with no loaded focus options.");
    Assert.False(viewModel.HasSelectedTask, "Start should begin without a selected focus task.");
    Assert.False(viewModel.IsLoading, "Start should not begin in a loading state.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available when Start is idle.");
    Assert.False(viewModel.UseSuggestionCommand.CanExecute(null), "Use Suggestion should wait for a suggestion.");
}

static void StartWorkViewModelAppliesSessionChoices()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Draft the first Start screen", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.ChooseTenMinuteSessionCommand.Execute(null);

    Assert.Equal(10, viewModel.PlannedMinutes);
    Assert.Equal("10 minute focus", viewModel.PlannedMinutesText);
    Assert.Equal("10 minute session selected.", viewModel.SessionChoiceSummaryText);
    Assert.True(viewModel.IsTenMinuteSessionSelected, "Ten minute session should be selected.");
    Assert.False(viewModel.IsFifteenMinuteSessionSelected, "Fifteen minute session should not be selected.");
    Assert.False(viewModel.IsTwentyMinuteSessionSelected, "Twenty minute session should not be selected.");
    Assert.Contains("10 minute focus session", viewModel.FocusPanelText);
    Assert.Equal("10 minute focus selected.", viewModel.StatusText);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    Assert.Contains("10 minute focus session", viewModel.FocusPanelText);

    viewModel.ChooseFifteenMinuteSessionCommand.Execute(null);

    Assert.Equal(15, viewModel.PlannedMinutes);
    Assert.Equal("15 minute focus", viewModel.PlannedMinutesText);
    Assert.Equal("15 minute session selected.", viewModel.SessionChoiceSummaryText);
    Assert.True(viewModel.IsFifteenMinuteSessionSelected, "Fifteen minute session should be selected.");
    Assert.False(viewModel.IsTenMinuteSessionSelected, "Ten minute session should no longer be selected.");
    Assert.False(viewModel.IsTwentyMinuteSessionSelected, "Twenty minute session should not be selected.");
    Assert.Contains("15 minute focus session", viewModel.FocusPanelText);
    Assert.Equal("15 minute focus selected.", viewModel.StatusText);

    viewModel.SetSessionLength(FocusSession.DefaultPlannedMinutes);

    Assert.Equal(20, viewModel.PlannedMinutes);
    Assert.Equal("20 minute focus", viewModel.PlannedMinutesText);
    Assert.True(viewModel.IsTwentyMinuteSessionSelected, "Twenty minute session should be selected.");
    Assert.Contains("20 minute focus session", viewModel.FocusPanelText);
    Assert.Equal("20 minute focus selected.", viewModel.StatusText);
}

static void StartWorkViewModelLoadsFocusTaskOptions()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem overdue = CreateTask("Call the pharmacy", dueDate: today.AddDays(-1));
    TaskItem dueToday = CreateTask("Pay the bill", dueDate: today, priority: TaskPriority.High);
    TaskItem selectedToday = CreateTask("Draft next plan", dueDate: today.AddDays(5), startDate: today);
    TaskItem inProgress = CreateTask("Finish active task", inProgress: true);
    TaskItem blocked = CreateTask("Resolve blocker", dueDate: today);
    blocked.MarkBlocked();
    TaskItem future = CreateTask("Future task", dueDate: today.AddDays(1));
    TaskItem looseCapture = CreateTask("Loose captured thought");
    TaskItem alreadyDone = CreateTask("Already complete", dueDate: today, done: true);
    DateOnly? requestedToday = null;
    StartWorkViewModel viewModel = new(
        (todayToLoad, _) =>
        {
            requestedToday = todayToLoad;
            return Task.FromResult<IReadOnlyList<TaskItem>>(
                [future, selectedToday, blocked, alreadyDone, dueToday, inProgress, looseCapture, overdue]);
        },
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.True(requestedToday.HasValue, "Start load should pass the current date to its loader.");
    Assert.Equal(today, requestedToday.GetValueOrDefault());
    Assert.Equal(4, viewModel.TaskOptions.Count);
    Assert.Equal(4, viewModel.Tasks.Count);
    Assert.Equal("Finish active task", viewModel.TaskOptions[0].Title);
    Assert.True(viewModel.TaskOptions[0].IsInProgress, "In-progress focus should sort first.");
    Assert.True(viewModel.TaskOptions[0].IsSuggestedAction, "In-progress focus should be suggested first.");
    Assert.Equal("Suggested", viewModel.TaskOptions[0].SuggestionBadgeText);
    Assert.Equal("Call the pharmacy", viewModel.TaskOptions[1].Title);
    Assert.True(viewModel.TaskOptions[1].IsOverdue, "Overdue focus should be marked overdue.");
    Assert.Equal("Pay the bill", viewModel.TaskOptions[2].Title);
    Assert.True(viewModel.TaskOptions[2].IsDueToday, "Due-today focus should be marked due today.");
    Assert.Equal("Draft next plan", viewModel.TaskOptions[3].Title);
    Assert.True(viewModel.TaskOptions[3].IsSelectedForToday, "Start-date task should be selected for today.");
    Assert.False(viewModel.TaskOptions.Any(task => task.Title == "Future task"), "Future task should not load into Start.");
    Assert.False(viewModel.TaskOptions.Any(task => task.Title == "Resolve blocker"), "Blocked task should not load into Start.");
    Assert.False(viewModel.TaskOptions.Any(task => task.Title == "Loose captured thought"), "Loose captures should wait for planning.");
    Assert.False(viewModel.TaskOptions.Any(task => task.Title == "Already complete"), "Done task should not load into Start.");
    Assert.True(viewModel.HasTaskOptions, "Loaded Start tasks should set HasTaskOptions.");
    Assert.True(viewModel.HasSuggestedTask, "Loaded focus options should produce a best-next suggestion.");
    Assert.Equal(inProgress.Id, viewModel.SuggestedTaskId.GetValueOrDefault());
    Assert.Equal("Finish active task", viewModel.SuggestedTaskTitle);
    Assert.Equal("Continue", viewModel.SuggestionBadgeText);
    Assert.Contains("Already in progress", viewModel.SuggestionReasonText);
    Assert.True(viewModel.UseSuggestionCommand.CanExecute(null), "Use Suggestion should enable after a suggestion loads.");
    Assert.Equal("4 options", viewModel.TaskCountText);
    Assert.Equal("Best next action suggested.", viewModel.StatusText);
    Assert.Equal("Pick one small action and start when ready.", viewModel.EmptyStateText);
}

static void StartWorkViewModelSuggestsBestNextAction()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem overdueTinyStep = CreateTask("Call the pharmacy", dueDate: today.AddDays(-1));
    overdueTinyStep.MarkTinyStep();
    overdueTinyStep.SetEstimate(15);
    overdueTinyStep.SetEnergyLevel("Low");
    TaskItem dueTodayLarge = CreateTask("Draft the larger plan", dueDate: today, priority: TaskPriority.Critical);
    dueTodayLarge.SetEstimate(90);
    TaskItem selectedToday = CreateTask("Sketch the next panel", startDate: today, priority: TaskPriority.High);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([dueTodayLarge, selectedToday, overdueTinyStep]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.HasSuggestedTask, "Start should suggest one best next action when options exist.");
    Assert.Equal(overdueTinyStep.Id, viewModel.SuggestedTaskId.GetValueOrDefault());
    Assert.Equal("Call the pharmacy", viewModel.SuggestedTaskTitle);
    Assert.Equal("Best Next", viewModel.SuggestionPanelTitle);
    Assert.Equal("Suggested: Call the pharmacy", viewModel.SuggestionPanelText);
    Assert.Equal("Overdue", viewModel.SuggestionBadgeText);
    Assert.Contains("Overdue and ready now", viewModel.SuggestionReasonText);
    Assert.True(viewModel.SuggestedTaskScore > 0, "Suggested task should expose a positive score.");

    StartWorkTaskOptionViewModel suggestedOption = viewModel.TaskOptions.First(task => task.Id == overdueTinyStep.Id);
    Assert.True(suggestedOption.IsSuggestedAction, "Suggested option should be marked for the view.");
    Assert.Equal("Suggested", suggestedOption.SuggestionBadgeText);
    Assert.Equal("#65BFB8", suggestedOption.CardBorderColor);
}

static void StartWorkViewModelUsesSuggestedAction()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem dueToday = CreateTask("Pay the bill", dueDate: today, priority: TaskPriority.High);
    TaskItem selectedToday = CreateTask("Sketch the next panel", startDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([selectedToday, dueToday]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    Assert.True(viewModel.UseSuggestionCommand.CanExecute(null), "Use Suggestion should be enabled after load.");

    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();

    Assert.Equal(dueToday.Id, viewModel.SelectedTaskId.GetValueOrDefault());
    Assert.Equal("Pay the bill", viewModel.SelectedTaskTitle);
    Assert.True(viewModel.HasSelectedTask, "Using the suggestion should select a focus task.");
    Assert.Equal("Pay the bill", viewModel.FocusIntention);
    Assert.Equal("Focus Selected", viewModel.FocusPanelTitle);
    Assert.Contains("suggested next action", viewModel.FocusPanelText);
    Assert.Equal("Suggested focus selected.", viewModel.StatusText);
}

static void StartWorkViewModelSelectsFocusTask()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Draft the first Start screen", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    StartWorkTaskOptionViewModel taskOption = viewModel.TaskOptions[0];
    Assert.True(taskOption.SelectCommand.CanExecute(null), "Focus option should expose a select command.");

    viewModel.SelectTask(taskOption);

    Assert.Equal(focusTask.Id, viewModel.SelectedTaskId.GetValueOrDefault());
    Assert.Equal("Draft the first Start screen", viewModel.SelectedTaskTitle);
    Assert.True(viewModel.HasSelectedTask, "Selecting a focus option should set HasSelectedTask.");
    Assert.Equal("Draft the first Start screen", viewModel.FocusIntention);
    Assert.Equal("Focus Selected", viewModel.FocusPanelTitle);
    Assert.Contains("20 minute focus session", viewModel.FocusPanelText);
    Assert.Equal("Suggested focus selected.", viewModel.StatusText);
}

static void StartWorkViewModelStartsFocusSession()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Draft the first Start screen", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    viewModel.SetSessionLength(15);
    viewModel.StartFocusAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.ActiveFocusSessionId.HasValue, "Starting focus should create a session id.");
    Assert.Equal(focusTask.Id, viewModel.ActiveFocusSessionTaskId.GetValueOrDefault());
    Assert.True(viewModel.ActiveFocusSessionStatus == FocusSessionStatus.InProgress, "Started focus session should be in progress.");
    Assert.Equal(15, viewModel.ActiveFocusSessionPlannedMinutes);
    Assert.True(viewModel.HasFocusSession, "Started focus session should be visible.");
    Assert.True(viewModel.HasActiveFocusSession, "Started focus session should be active.");
    Assert.True(viewModel.IsFocusSessionInProgress, "Started focus session should expose running state.");
    Assert.Equal("In progress", viewModel.FocusSessionStatusText);
    Assert.Equal("Focus Running", viewModel.FocusSessionPanelTitle);
    Assert.Equal("Running", viewModel.FocusSessionBadgeText);
    Assert.Equal("15:00 focus block", viewModel.FocusTimerText);
    Assert.Contains("only focus", viewModel.FocusSessionPanelText);
    Assert.Contains("Keep this one action", viewModel.FocusSessionWinText);
    Assert.Equal(TaskItemStatus.InProgress, focusTask.Status);
    Assert.False(viewModel.CanStartFocus, "Start should disable while a session is active.");
    Assert.True(viewModel.CanPauseFocus, "Pause should enable while a session is running.");
    Assert.False(viewModel.CanResumeFocus, "Resume should wait for a paused session.");
    Assert.True(viewModel.CanCompleteFocus, "Complete should be available during active focus.");
    Assert.True(viewModel.CanBlockFocus, "Blocked should be available during active focus.");
    Assert.False(viewModel.StartFocusCommand.CanExecute(null), "Start command should disable while running.");
    Assert.True(viewModel.PauseFocusCommand.CanExecute(null), "Pause command should enable while running.");
}

static void StartWorkViewModelPausesAndResumesFocusSession()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Draft the pause state", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    viewModel.StartFocusAsync().GetAwaiter().GetResult();
    viewModel.PauseFocusAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.ActiveFocusSessionStatus == FocusSessionStatus.Paused, "Pause should put the session in paused state.");
    Assert.True(viewModel.IsFocusSessionPaused, "Pause should expose paused state.");
    Assert.Equal("Paused", viewModel.FocusSessionStatusText);
    Assert.Equal("Focus Paused", viewModel.FocusSessionPanelTitle);
    Assert.Equal("20:00 paused", viewModel.FocusTimerText);
    Assert.False(viewModel.CanPauseFocus, "Pause should disable while paused.");
    Assert.True(viewModel.CanResumeFocus, "Resume should enable while paused.");
    Assert.True(viewModel.ResumeFocusCommand.CanExecute(null), "Resume command should enable while paused.");

    viewModel.ResumeFocusAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.ActiveFocusSessionStatus == FocusSessionStatus.InProgress, "Resume should return the session to running.");
    Assert.True(viewModel.IsFocusSessionInProgress, "Resume should expose running state.");
    Assert.Equal("In progress", viewModel.FocusSessionStatusText);
    Assert.Equal("Focus Running", viewModel.FocusSessionPanelTitle);
    Assert.True(viewModel.CanPauseFocus, "Pause should enable after resume.");
    Assert.False(viewModel.CanResumeFocus, "Resume should disable after resume.");
}

static void StartWorkViewModelCompletesFocusSession()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Draft the completion state", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    viewModel.StartFocusAsync().GetAwaiter().GetResult();
    viewModel.CompleteFocusAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.ActiveFocusSessionStatus == FocusSessionStatus.Completed, "Complete should close the focus session as completed.");
    Assert.True(viewModel.HasEndedFocusSession, "Completed session should expose ended state.");
    Assert.True(viewModel.IsFocusSessionCompleted, "Completed session should expose completed state.");
    Assert.Equal("Completed", viewModel.FocusSessionStatusText);
    Assert.Equal("Focus Complete", viewModel.FocusSessionPanelTitle);
    Assert.Equal("Done", viewModel.FocusSessionBadgeText);
    Assert.Contains("recorded", viewModel.FocusTimerText);
    Assert.Contains("Completed 20 minute focus", viewModel.FocusSessionWinText);
    Assert.True(viewModel.HasBreakSuggestion, "Completing focus should suggest a break.");
    Assert.Equal(5, viewModel.BreakSuggestionMinutes);
    Assert.Equal("5 minute reset", viewModel.BreakSuggestionDurationText);
    Assert.Equal("Take a Reset", viewModel.BreakSuggestionTitle);
    Assert.Equal("Break", viewModel.BreakSuggestionBadgeText);
    Assert.Contains("away from the screen", viewModel.BreakSuggestionText);
    Assert.Equal(TaskItemStatus.InProgress, focusTask.Status);
    Assert.True(viewModel.CanStartFocus, "A completed focus session should allow a new session.");
    Assert.False(viewModel.CanPauseFocus, "Pause should disable after completion.");
    Assert.False(viewModel.CanResumeFocus, "Resume should disable after completion.");
    Assert.False(viewModel.CanCompleteFocus, "Complete should disable after completion.");
    Assert.False(viewModel.CanBlockFocus, "Blocked should disable after completion.");
}

static void StartWorkViewModelBlocksFocusSessionWithoutCompletingTask()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Call the supplier", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    viewModel.StartFocusAsync().GetAwaiter().GetResult();
    viewModel.BlockedReasonDraft = "Waiting on a return call";
    viewModel.BlockFocusAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.ActiveFocusSessionStatus == FocusSessionStatus.Blocked, "Blocked should close the focus session as blocked.");
    Assert.True(viewModel.HasEndedFocusSession, "Blocked session should expose ended state.");
    Assert.True(viewModel.IsFocusSessionBlocked, "Blocked session should expose blocked state.");
    Assert.Equal("Blocked", viewModel.FocusSessionStatusText);
    Assert.Equal("Focus Blocked", viewModel.FocusSessionPanelTitle);
    Assert.Equal("Waiting on a return call", viewModel.FocusSessionBlockedReason);
    Assert.Contains("Blocked: Waiting on a return call", viewModel.FocusSessionWinText);
    Assert.False(viewModel.HasBreakSuggestion, "Blocked focus should not suggest the completion break.");
    Assert.Equal("Waiting", viewModel.BreakSuggestionBadgeText);
    Assert.Contains("Blocked sessions wait", viewModel.BreakSuggestionActionText);
    Assert.Equal(TaskItemStatus.InProgress, focusTask.Status);
    Assert.False(focusTask.Status == TaskItemStatus.Done, "Blocked session must not mark the task complete.");
    Assert.True(viewModel.CanStartFocus, "A blocked focus session should allow a new session.");
    Assert.False(viewModel.CanCompleteFocus, "Complete should disable after blocked.");
    Assert.False(viewModel.CanBlockFocus, "Blocked should disable after blocked.");
}

static void StartWorkViewModelSavesFocusSessionStateChanges()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Draft the local save path", dueDate: today);
    List<(Guid Id, FocusSessionStatus Status, int PlannedMinutes, Guid? TaskId)> savedSessions = [];
    List<(Guid Id, TaskItemStatus Status)> savedTasks = [];
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        (session, _) =>
        {
            savedSessions.Add((session.Id, session.Status, session.PlannedMinutes, session.TaskId));
            return Task.CompletedTask;
        },
        (task, _) =>
        {
            savedTasks.Add((task.Id, task.Status));
            return Task.CompletedTask;
        },
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    viewModel.SetSessionLength(10);
    viewModel.StartFocusAsync().GetAwaiter().GetResult();

    Assert.Equal(1, savedSessions.Count);
    Assert.Equal(FocusSessionStatus.InProgress, savedSessions[^1].Status);
    Assert.Equal(10, savedSessions[^1].PlannedMinutes);
    Assert.Equal(focusTask.Id, savedSessions[^1].TaskId.GetValueOrDefault());
    Assert.Equal(1, savedTasks.Count);
    Assert.Equal(TaskItemStatus.InProgress, savedTasks[^1].Status);
    Assert.True(viewModel.HasSavedFocusSession, "Start should expose the saved session id.");
    Assert.Equal(savedSessions[^1].Id, viewModel.LastSavedFocusSessionId.GetValueOrDefault());
    Assert.Equal("Saved locally: running focus session.", viewModel.FocusSessionStorageText);
    Assert.Equal("Focus session started and saved locally.", viewModel.StatusText);

    viewModel.PauseFocusAsync().GetAwaiter().GetResult();
    Assert.Equal(2, savedSessions.Count);
    Assert.Equal(FocusSessionStatus.Paused, savedSessions[^1].Status);
    Assert.Equal("Saved locally: paused focus session.", viewModel.FocusSessionStorageText);

    viewModel.ResumeFocusAsync().GetAwaiter().GetResult();
    Assert.Equal(3, savedSessions.Count);
    Assert.Equal(FocusSessionStatus.InProgress, savedSessions[^1].Status);
    Assert.Equal("Saved locally: running focus session.", viewModel.FocusSessionStorageText);

    viewModel.CompleteFocusAsync().GetAwaiter().GetResult();
    Assert.Equal(4, savedSessions.Count);
    Assert.Equal(FocusSessionStatus.Completed, savedSessions[^1].Status);
    Assert.Equal("Saved locally: completed focus session.", viewModel.FocusSessionStorageText);
    Assert.Equal("Focus session completed and saved locally.", viewModel.StatusText);
}

static void StartWorkViewModelSuggestsBreakAfterCompletion()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem focusTask = CreateTask("Finish a short focus", dueDate: today);
    StartWorkViewModel viewModel = new(
        (_, _) => Task.FromResult<IReadOnlyList<TaskItem>>([focusTask]),
        () => today);

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();
    viewModel.UseSuggestedTaskAsync().GetAwaiter().GetResult();
    viewModel.SetSessionLength(10);
    viewModel.StartFocusAsync().GetAwaiter().GetResult();
    Assert.False(viewModel.HasBreakSuggestion, "Starting focus should keep break suggestion pending.");

    viewModel.CompleteFocusAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.HasBreakSuggestion, "Completing focus should create a break suggestion.");
    Assert.Equal(3, viewModel.BreakSuggestionMinutes);
    Assert.Equal("3 minute reset", viewModel.BreakSuggestionDurationText);
    Assert.Equal("Take a Reset", viewModel.BreakSuggestionTitle);
    Assert.Equal("Break", viewModel.BreakSuggestionBadgeText);
    Assert.Contains("You finished 10 minutes", viewModel.BreakSuggestionText);
    Assert.Contains("drink water", viewModel.BreakSuggestionActionText);

    viewModel.StartFocusAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasBreakSuggestion, "Starting the next focus should clear the last break suggestion.");
    Assert.Equal("Break pending", viewModel.BreakSuggestionDurationText);
    Assert.Equal("Waiting", viewModel.BreakSuggestionBadgeText);
}

static void StartWorkViewModelCreatesTaskOptionDisplayState()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem overdue = CreateTask("Call the pharmacy", dueDate: today.AddDays(-1), priority: TaskPriority.Critical);
    overdue.UpdateNotes("Ask about the refill, bring the glucose log, and keep the call under ten minutes.");
    overdue.SetEstimate(15);
    overdue.SetEnergyLevel("Low");
    overdue.MarkTinyStep();

    StartWorkTaskOptionViewModel overdueOption = StartWorkTaskOptionViewModel.FromTask(overdue, today);

    Assert.Equal("Call the pharmacy", overdueOption.Title);
    Assert.True(overdueOption.HasNotes, "Start option should show notes when notes exist.");
    Assert.Contains("glucose log", overdueOption.NotesPreview);
    Assert.Equal("Overdue", overdueOption.DueBadgeText);
    Assert.Contains("Overdue", overdueOption.DueText);
    Assert.Equal("Critical", overdueOption.PriorityBadgeText);
    Assert.Equal("Planned", overdueOption.StatusBadgeText);
    Assert.Equal("15 min", overdueOption.EstimateText);
    Assert.Equal("Low", overdueOption.EnergyLevel);
    Assert.True(overdueOption.IsTinyStep, "Tiny-step task should expose tiny-step display state.");
    Assert.False(overdueOption.IsSuggestedAction, "Standalone option should not mark itself suggested.");
    Assert.Equal(string.Empty, overdueOption.SuggestionBadgeText);
    Assert.Equal("Tiny step", overdueOption.FocusBadgeText);
    Assert.Contains("Overdue", overdueOption.FocusReasonText);
    Assert.Equal("#FFBE7A", overdueOption.CardAccentColor);
    Assert.Equal("\uE823", overdueOption.CardIconGlyph);
    Assert.Contains("Call the pharmacy", overdueOption.CardToolTip);

    TaskItem active = CreateTask("Keep going on the active item", inProgress: true);
    StartWorkTaskOptionViewModel activeOption = StartWorkTaskOptionViewModel.FromTask(active, today);

    Assert.Equal("Keep going", activeOption.FocusBadgeText);
    Assert.Equal("Already in progress", activeOption.FocusReasonText);
    Assert.Equal("#8DDAD5", activeOption.CardAccentColor);
    Assert.Equal("\uE768", activeOption.CardIconGlyph);
}

static void StartWorkViewModelShowsEmptyFocusState()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem blocked = CreateTask("Blocked task", dueDate: today);
    blocked.MarkBlocked();
    StartWorkViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>(
            [CreateTask("Future task", dueDate: today.AddDays(1)), blocked]),
        () => today);

    viewModel.OnNavigatedToAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.False(viewModel.HasTaskOptions, "Future and blocked tasks should leave Start empty.");
    Assert.Equal("0 options", viewModel.TaskCountText);
    Assert.Equal("No focus options ready.", viewModel.StatusText);
    Assert.Contains("Capture or plan", viewModel.EmptyStateText);
    Assert.False(viewModel.HasSelectedTask, "Empty Start should not select a task.");
    Assert.False(viewModel.HasSuggestedTask, "Empty Start should not suggest a task.");
    Assert.Equal("No suggestion yet.", viewModel.SuggestedTaskTitle);
    Assert.Equal("Waiting", viewModel.SuggestionBadgeText);
    Assert.False(viewModel.UseSuggestionCommand.CanExecute(null), "Use Suggestion should disable when empty.");
}

static void StartWorkViewModelReportsLoadFailures()
{
    StartWorkViewModel viewModel = new(
        _ => throw new InvalidOperationException("Task storage unavailable."),
        () => new DateOnly(2026, 5, 30));

    viewModel.LoadTasksAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasTaskOptions, "Failed load should not leave stale focus options visible.");
    Assert.Equal("0 options", viewModel.TaskCountText);
    Assert.Equal("Start could not load.", viewModel.StatusText);
    Assert.Contains("Try Refresh", viewModel.EmptyStateText);
    Assert.False(viewModel.HasSelectedTask, "Failed load should clear any selected focus task.");
    Assert.False(viewModel.HasSuggestedTask, "Failed load should clear the best-next suggestion.");
    Assert.Equal("No suggestion yet.", viewModel.SuggestedTaskTitle);
    Assert.False(viewModel.UseSuggestionCommand.CanExecute(null), "Use Suggestion should disable after failure.");
    Assert.False(viewModel.IsLoading, "Loading flag should clear after failure.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available after failure.");
}

static void PlanViewModelStartsReady()
{
    PlanViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Plan, viewModel.Descriptor);
    Assert.Equal("Plan", viewModel.Title);
    Assert.Equal("Make the next step smaller.", viewModel.Subtitle);
    Assert.Equal("\uE9D5", viewModel.IconGlyph);
    Assert.Equal("#FFE08A", viewModel.AccentColor);
    Assert.Equal("Ready to plan.", viewModel.StatusText);
    Assert.Equal("Plan Small", viewModel.PlanPanelTitle);
    Assert.Contains("one realistic next action", viewModel.PlanPanelText);
    Assert.Equal("Unplanned inbox not loaded yet.", viewModel.InboxStatusText);
    Assert.Equal("No inbox item selected.", viewModel.SelectedInboxItemText);
    Assert.Equal("No planning changes yet.", viewModel.PlanningStatusText);
    Assert.Equal("No minimum win selected.", viewModel.MinimumWinText);
    Assert.Equal("Plan is local-first and not saved yet.", viewModel.SaveStatusText);
    Assert.Equal("Load unplanned inbox next.", viewModel.EmptyStateText);
    Assert.Equal("0 unplanned", viewModel.InboxCountText);
    Assert.False(viewModel.IsLoading, "Plan should start idle.");
    Assert.False(viewModel.HasInboxItems, "Plan should wait for inbox loading.");
    Assert.False(viewModel.HasSelectedInboxItem, "Plan should start without a selected inbox item.");
    Assert.False(viewModel.HasPlanningControls, "Plan controls should wait for a selected inbox item.");
    Assert.False(viewModel.HasPlanningChanges, "Plan should start without planning changes.");
    Assert.False(viewModel.CanSavePlan, "Plan should not save before planning changes.");
    Assert.False(viewModel.IsSavingPlan, "Plan should start without an active save.");
    Assert.Null(viewModel.LastSavedTaskId, "Plan should start without a saved task id.");
    Assert.False(viewModel.HasSavedPlan, "Plan should start without saved planning changes.");
    Assert.Equal(0, viewModel.SavedTinyStepIds.Count);
    Assert.Equal("0 tiny steps saved", viewModel.SavedTinyStepCountText);
    Assert.Null(viewModel.SelectedPriority, "Plan should start without a selected priority.");
    Assert.Equal("No priority selected.", viewModel.PriorityText);
    Assert.False(viewModel.IsLowPrioritySelected, "Low priority should start unselected.");
    Assert.False(viewModel.IsNormalPrioritySelected, "Normal priority should start unselected.");
    Assert.False(viewModel.IsHighPrioritySelected, "High priority should start unselected.");
    Assert.False(viewModel.IsCriticalPrioritySelected, "Critical priority should start unselected.");
    Assert.Null(viewModel.SelectedDueDate, "Plan should start without a selected date.");
    Assert.Equal("No plan date selected.", viewModel.DateHintText);
    Assert.Equal(string.Empty, viewModel.ProjectName);
    Assert.Equal("No project selected.", viewModel.ProjectText);
    Assert.False(viewModel.HasProjectName, "Plan should start without a project name.");
    Assert.Equal(string.Empty, viewModel.MinimumWinDraft);
    Assert.False(viewModel.HasMinimumWin, "Plan should start without a minimum win.");
    Assert.Equal(string.Empty, viewModel.TinyStepDraft);
    Assert.Equal("No tiny steps created.", viewModel.TinyStepsText);
    Assert.Equal("Split a selected item into tiny steps.", viewModel.TinyStepStatusText);
    Assert.Equal("0 tiny steps", viewModel.TinyStepCountText);
    Assert.False(viewModel.HasTinySteps, "Plan should start without tiny steps.");
    Assert.False(viewModel.CanSplitIntoTinySteps, "Split should wait for a selected inbox item.");
    Assert.False(viewModel.CanAddTinyStep, "Add tiny step should wait for text and selection.");
    Assert.False(viewModel.CanClearTinySteps, "Clear tiny steps should wait for drafted steps.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available when Plan is idle.");
    Assert.False(viewModel.ChooseLowPriorityCommand.CanExecute(null), "Priority controls should wait for selection.");
    Assert.False(viewModel.TodayDateCommand.CanExecute(null), "Date controls should wait for selection.");
    Assert.False(viewModel.SplitIntoTinyStepsCommand.CanExecute(null), "Split command should wait for selection.");
    Assert.False(viewModel.AddTinyStepCommand.CanExecute(null), "Add command should wait for text and selection.");
    Assert.False(viewModel.ClearTinyStepsCommand.CanExecute(null), "Clear command should wait for steps.");
    Assert.False(viewModel.SavePlanCommand.CanExecute(null), "Save should wait for a ready plan draft.");
}

static void PlanViewModelLoadsUnplannedInbox()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem looseCapture = CreateTask("Loose capture");
    looseCapture.UpdateNotes("A loose thought that needs a next action.");
    TaskItem highCapture = CreateTask("High priority capture", priority: TaskPriority.High);
    TaskItem plannedTask = CreateTask("Already planned", dueDate: today);
    TaskItem inProgressTask = CreateTask("Already started", inProgress: true);
    TaskItem doneTask = CreateTask("Already done", done: true);
    PlanViewModel viewModel = new(_ => Task.FromResult<IReadOnlyList<TaskItem>>(
        [plannedTask, doneTask, looseCapture, highCapture, inProgressTask]));

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();

    Assert.Equal(2, viewModel.InboxItems.Count);
    Assert.Equal(2, viewModel.Tasks.Count);
    Assert.Equal("High priority capture", viewModel.InboxItems[0].Title);
    Assert.Equal("Loose capture", viewModel.InboxItems[1].Title);
    Assert.True(viewModel.HasInboxItems, "Loaded unplanned captures should be visible.");
    Assert.Equal("2 unplanned", viewModel.InboxCountText);
    Assert.Equal("2 unplanned items ready.", viewModel.InboxStatusText);
    Assert.Equal("Inbox ready.", viewModel.StatusText);
    Assert.Contains("Choose one loose capture", viewModel.EmptyStateText);
    Assert.Equal("Choose an inbox item to start planning.", viewModel.PlanningStatusText);
    Assert.False(viewModel.HasSelectedInboxItem, "Loading inbox should not auto-select an item.");

    viewModel.SelectInboxItem(viewModel.InboxItems[1]);

    Assert.True(viewModel.HasSelectedInboxItem, "Clicking an inbox item should select it.");
    Assert.Equal(looseCapture.Id, viewModel.SelectedInboxItemId.GetValueOrDefault());
    Assert.Equal("Selected: Loose capture", viewModel.SelectedInboxItemText);
    Assert.Equal("Ready to plan Loose capture.", viewModel.PlanningStatusText);
    Assert.Equal("Minimum win pending for Loose capture.", viewModel.MinimumWinText);
    Assert.Equal("Planning changes not saved yet.", viewModel.SaveStatusText);
    Assert.Equal("Inbox item selected.", viewModel.StatusText);
    Assert.Equal(TaskPriority.Normal, viewModel.SelectedPriority.GetValueOrDefault());
    Assert.Equal("Normal priority", viewModel.PriorityText);
    Assert.True(viewModel.IsNormalPrioritySelected, "Selected inbox item should apply its current priority.");
    Assert.True(viewModel.HasPlanningControls, "Selecting an inbox item should unlock Plan controls.");
    Assert.True(viewModel.ChooseHighPriorityCommand.CanExecute(null), "Priority choices should unlock after selection.");
    Assert.True(viewModel.TodayDateCommand.CanExecute(null), "Date choices should unlock after selection.");
    Assert.False(viewModel.HasPlanningChanges, "Selecting an inbox item alone should not mark planning changed.");
    Assert.False(viewModel.CanSavePlan, "Minimum win should be required before plan draft is save-ready.");
    Assert.True(viewModel.CanSplitIntoTinySteps, "Split should unlock after selection.");
    Assert.True(viewModel.SplitIntoTinyStepsCommand.CanExecute(null), "Split command should unlock after selection.");
    Assert.False(viewModel.CanAddTinyStep, "Add should wait for custom tiny step text.");
    Assert.False(viewModel.CanClearTinySteps, "Clear should wait for drafted tiny steps.");
}

static void PlanViewModelAppliesPlanningControls()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem task = CreateTask("Build plan controls");
    PlanViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([task]),
        () => today);

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();
    viewModel.SelectInboxItem(viewModel.InboxItems[0]);

    viewModel.ChooseHighPriorityCommand.Execute(null);

    Assert.Equal(TaskPriority.High, viewModel.SelectedPriority.GetValueOrDefault());
    Assert.Equal("High priority", viewModel.PriorityText);
    Assert.True(viewModel.IsHighPrioritySelected, "High priority button should become selected.");
    Assert.True(viewModel.HasPlanningChanges, "Priority choice should mark the draft changed.");
    Assert.False(viewModel.CanSavePlan, "Minimum win should still be required.");
    Assert.Equal("Add a minimum win before saving.", viewModel.SaveStatusText);

    viewModel.TodayDateCommand.Execute(null);

    Assert.Equal(today, viewModel.SelectedDueDate.GetValueOrDefault());
    Assert.Equal("Today: May 30", viewModel.DateHintText);
    Assert.Contains("Today: May 30", viewModel.PlanningStatusText);

    viewModel.TomorrowDateCommand.Execute(null);

    Assert.Equal(today.AddDays(1), viewModel.SelectedDueDate.GetValueOrDefault());
    Assert.Equal("Tomorrow: May 31", viewModel.DateHintText);

    viewModel.ThisWeekDateCommand.Execute(null);

    Assert.Equal(today.AddDays(7), viewModel.SelectedDueDate.GetValueOrDefault());
    Assert.Equal("This week: Jun 6", viewModel.DateHintText);

    viewModel.ClearDateCommand.Execute(null);

    Assert.Null(viewModel.SelectedDueDate, "No Date should clear the selected plan date.");
    Assert.Equal("No plan date selected.", viewModel.DateHintText);

    viewModel.ProjectName = "  Success Planner  ";

    Assert.True(viewModel.HasProjectName, "Project text should mark a project name present.");
    Assert.Equal("Project: Success Planner", viewModel.ProjectText);
    Assert.Contains("Project: Success Planner", viewModel.PlanningStatusText);

    viewModel.MinimumWinDraft = "Pick one realistic next action";

    Assert.True(viewModel.HasMinimumWin, "Minimum win text should mark a minimum win present.");
    Assert.Equal("Minimum win: Pick one realistic next action", viewModel.MinimumWinText);
    Assert.True(viewModel.CanSavePlan, "Priority, project, and minimum win draft should be save-ready.");
    Assert.True(viewModel.SavePlanCommand.CanExecute(null), "Save command should unlock when draft is ready.");
    Assert.Equal("Draft ready for local save.", viewModel.SaveStatusText);
    Assert.Equal("Minimum win updated.", viewModel.StatusText);
}

static void PlanViewModelSplitsSelectedItemIntoTinySteps()
{
    TaskItem original = CreateTask("Build the planning screen");
    PlanViewModel viewModel = new(_ => Task.FromResult<IReadOnlyList<TaskItem>>([original]));

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();
    viewModel.SelectInboxItem(viewModel.InboxItems[0]);

    viewModel.SplitIntoTinyStepsCommand.Execute(null);

    Assert.True(viewModel.HasTinySteps, "Split should create visible tiny steps.");
    Assert.Equal(3, viewModel.TinySteps.Count);
    Assert.Equal("3 tiny steps", viewModel.TinyStepCountText);
    Assert.Equal("3 tiny steps drafted.", viewModel.TinyStepsText);
    Assert.Equal("Set up Build the planning screen", viewModel.TinySteps[0].Title);
    Assert.Equal("Do 10 minutes of Build the planning screen", viewModel.TinySteps[1].Title);
    Assert.Equal("Write the next note for Build the planning screen", viewModel.TinySteps[2].Title);
    Assert.True(viewModel.TinySteps.All(step => step.IsTinyStep), "Every split result should be a tiny step draft.");
    Assert.Equal("Step 1", viewModel.TinySteps[0].BadgeText);
    Assert.Equal("Tiny steps created.", viewModel.StatusText);
    Assert.Contains("3 tiny steps", viewModel.PlanningStatusText);
    Assert.True(viewModel.HasPlanningChanges, "Split should mark the draft changed.");
    Assert.False(viewModel.CanSavePlan, "Minimum win should still be required before saving.");
    Assert.Equal(original.Id, viewModel.SelectedInboxItemId.GetValueOrDefault());
    Assert.True(viewModel.InboxItems.Any(item => item.Id == original.Id), "Original inbox item should remain visible after split.");
    Assert.True(viewModel.ClearTinyStepsCommand.CanExecute(null), "Clear should unlock once steps exist.");

    viewModel.SplitIntoTinyStepsCommand.Execute(null);

    Assert.Equal(3, viewModel.TinySteps.Count);
    Assert.Equal("Tiny steps already ready.", viewModel.StatusText);

    viewModel.TinyStepDraft = "  Send one note  ";

    Assert.True(viewModel.CanAddTinyStep, "Custom tiny step text should unlock Add.");
    Assert.True(viewModel.AddTinyStepCommand.CanExecute(null), "Add command should be available with text.");

    viewModel.AddTinyStepCommand.Execute(null);

    Assert.Equal(4, viewModel.TinySteps.Count);
    Assert.Equal("Send one note", viewModel.TinySteps[3].Title);
    Assert.Equal("Step 4", viewModel.TinySteps[3].BadgeText);
    Assert.Equal(string.Empty, viewModel.TinyStepDraft);
    Assert.Equal("Tiny step added.", viewModel.StatusText);

    viewModel.TinySteps[1].RemoveCommand.Execute(null);

    Assert.Equal(3, viewModel.TinySteps.Count);
    Assert.Equal("Step 1", viewModel.TinySteps[0].BadgeText);
    Assert.Equal("Step 2", viewModel.TinySteps[1].BadgeText);
    Assert.Equal("Step 3", viewModel.TinySteps[2].BadgeText);
    Assert.Equal("Tiny step removed.", viewModel.StatusText);

    viewModel.ClearTinyStepsCommand.Execute(null);

    Assert.False(viewModel.HasTinySteps, "Clear should remove tiny step drafts.");
    Assert.Equal(0, viewModel.TinySteps.Count);
    Assert.Equal("0 tiny steps", viewModel.TinyStepCountText);
    Assert.Equal("No tiny steps created.", viewModel.TinyStepsText);
    Assert.Equal("Tiny steps cleared.", viewModel.StatusText);
    Assert.True(viewModel.InboxItems.Any(item => item.Id == original.Id), "Clearing split drafts should not remove the original inbox item.");
}

static void PlanViewModelSavesPlanningChangesLocally()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem original = CreateTask("Build plan save");
    List<TaskItem> savedTasks = [];
    PlanViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([original]),
        (task, _) =>
        {
            savedTasks.Add(task);
            return Task.CompletedTask;
        },
        () => today);

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();
    viewModel.SelectInboxItem(viewModel.InboxItems[0]);
    viewModel.ChooseHighPriorityCommand.Execute(null);
    viewModel.TodayDateCommand.Execute(null);
    viewModel.ProjectName = "Success Planner";
    viewModel.MinimumWinDraft = "Save one planned action";
    viewModel.SplitIntoTinyStepsCommand.Execute(null);

    Assert.True(viewModel.CanSavePlan, "Plan draft should be save-ready.");
    Assert.True(viewModel.SavePlanCommand.CanExecute(null), "Save command should be available.");

    viewModel.SavePlanAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal(4, savedTasks.Count);
    TaskItem plannedTask = savedTasks[0];
    Assert.Equal(original.Id, plannedTask.Id);
    Assert.Equal(TaskItemStatus.Planned, plannedTask.Status);
    Assert.Equal(TaskPriority.High, plannedTask.Priority);
    Assert.Equal(today, plannedTask.DueDate);
    Assert.Equal(today, plannedTask.StartDate);
    Assert.Contains("Minimum Win: Save one planned action", plannedTask.Notes);
    Assert.Contains("Project: Success Planner", plannedTask.Notes);
    Assert.Contains("Tiny Steps:", plannedTask.Notes);
    Assert.Contains("Plan", plannedTask.Tags);
    Assert.Contains("Minimum Win", plannedTask.Tags);

    IReadOnlyList<TaskItem> tinySteps = savedTasks.Skip(1).ToList();
    Assert.Equal(3, tinySteps.Count);
    Assert.True(tinySteps.All(task => task.IsTinyStep), "Saved tiny steps should be real task records.");
    Assert.True(tinySteps.All(task => task.Status == TaskItemStatus.Planned), "Saved tiny steps should be planned.");
    Assert.True(tinySteps.All(task => task.DueDate == today), "Saved tiny steps should inherit the plan date.");
    Assert.True(tinySteps.All(task => task.Priority == TaskPriority.High), "Saved tiny steps should inherit priority.");
    Assert.True(tinySteps.All(task => task.Notes.Contains("Split from: Build plan save", StringComparison.Ordinal)), "Saved tiny steps should link back to the original task.");
    Assert.True(tinySteps.Select(task => task.Id).Distinct().Count() == 3, "Each tiny step should have its own id.");

    Assert.True(viewModel.HasSavedPlan, "Plan should expose a saved state.");
    Assert.Equal(original.Id, viewModel.LastSavedTaskId.GetValueOrDefault());
    Assert.Equal(3, viewModel.SavedTinyStepIds.Count);
    Assert.Equal("3 tiny steps saved", viewModel.SavedTinyStepCountText);
    Assert.False(viewModel.InboxItems.Any(item => item.Id == original.Id), "Saved planned task should leave the unplanned inbox.");
    Assert.Equal("0 unplanned", viewModel.InboxCountText);
    Assert.Equal("Plan saved locally.", viewModel.StatusText);
    Assert.Contains("Saved locally: Build plan save plus 3 tiny steps.", viewModel.SaveStatusText);
    Assert.Contains("Minimum win saved", viewModel.MinimumWinText);
    Assert.False(viewModel.CanSavePlan, "Saved plan should not remain save-ready.");
    Assert.False(viewModel.SavePlanCommand.CanExecute(null), "Save command should disable after save.");
}

static void PlanViewModelReportsSaveFailures()
{
    TaskItem original = CreateTask("Save should fail");
    PlanViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([original]),
        (_, _) => throw new InvalidOperationException("Local database unavailable."));

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();
    viewModel.SelectInboxItem(viewModel.InboxItems[0]);
    viewModel.MinimumWinDraft = "Know that failed saves are visible";

    viewModel.SavePlanAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal("Plan save failed.", viewModel.StatusText);
    Assert.Equal("Planning changes were not saved locally.", viewModel.SaveStatusText);
    Assert.False(viewModel.HasSavedPlan, "Failed save should not expose a saved task id.");
    Assert.True(viewModel.InboxItems.Any(item => item.Id == original.Id), "Failed save should leave the inbox item visible.");
    Assert.True(viewModel.CanSavePlan, "Failed save should leave the corrected draft ready to retry.");
}

static void PlanViewModelCreatesInboxCardDisplayState()
{
    TaskItem task = CreateTask("Sketch the first tiny step", priority: TaskPriority.Critical);
    task.UpdateNotes("Keep it small enough to finish in one short focus block.");

    PlanInboxTaskViewModel card = PlanInboxTaskViewModel.FromTask(task);

    Assert.Equal(task.Id, card.Id);
    Assert.Equal("Sketch the first tiny step", card.Title);
    Assert.True(card.HasNotes, "Inbox card should show notes when notes exist.");
    Assert.Contains("short focus block", card.NotesPreview);
    Assert.Equal(TaskItemStatus.Captured, card.Status);
    Assert.Equal("Critical", card.PriorityBadgeText);
    Assert.Equal("Unplanned", card.StatusBadgeText);
    Assert.Equal("\uE9D5", card.CardIconGlyph);
    Assert.Contains("Sketch the first tiny step", card.CardToolTip);
}

static void PlanViewModelCreatesTinyStepDisplayState()
{
    PlanTinyStepViewModel step = new(2, "  Write the next note  ");

    Assert.Equal(2, step.SequenceNumber);
    Assert.Equal("Write the next note", step.Title);
    Assert.True(step.IsTinyStep, "Tiny step display state should identify tiny steps.");
    Assert.Equal("Step 2", step.BadgeText);
    Assert.Equal("\uE73E", step.CardIconGlyph);
    Assert.Contains("Write the next note", step.CardToolTip);

    step.SetSequenceNumber(3);

    Assert.Equal("Step 3", step.BadgeText);
}

static void PlanViewModelShowsEmptyUnplannedInbox()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem plannedTask = CreateTask("Already planned", dueDate: today);
    TaskItem doneTask = CreateTask("Already done", done: true);
    PlanViewModel viewModel = new(_ => Task.FromResult<IReadOnlyList<TaskItem>>([plannedTask, doneTask]));

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasInboxItems, "Planned and done tasks should leave Plan inbox empty.");
    Assert.Equal(0, viewModel.InboxItems.Count);
    Assert.Equal("0 unplanned", viewModel.InboxCountText);
    Assert.Equal("No unplanned inbox items.", viewModel.InboxStatusText);
    Assert.Equal("Inbox is clear.", viewModel.StatusText);
    Assert.Contains("Capture a loose thought", viewModel.EmptyStateText);
    Assert.Equal("No unplanned items to plan.", viewModel.PlanningStatusText);
    Assert.False(viewModel.HasSelectedInboxItem, "Empty inbox should not select an item.");
}

static void PlanViewModelReportsLoadFailures()
{
    PlanViewModel viewModel = new(_ => throw new InvalidOperationException("Task storage unavailable."));

    viewModel.LoadInboxAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasInboxItems, "Failed load should not leave stale inbox items visible.");
    Assert.Equal("0 unplanned", viewModel.InboxCountText);
    Assert.Equal("Plan could not load.", viewModel.StatusText);
    Assert.Equal("Unplanned inbox could not load.", viewModel.InboxStatusText);
    Assert.Contains("Try Refresh", viewModel.EmptyStateText);
    Assert.False(viewModel.HasSelectedInboxItem, "Failed load should clear selected inbox item.");
    Assert.False(viewModel.IsLoading, "Loading flag should clear after failure.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available after failure.");
}

static void ReviewViewModelStartsReady()
{
    ReviewViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Review, viewModel.Descriptor);
    Assert.Equal("Review", viewModel.Title);
    Assert.Equal("Notice progress and choose what matters next.", viewModel.Subtitle);
    Assert.Equal("\uE9D2", viewModel.IconGlyph);
    Assert.Equal("#C8B6FF", viewModel.AccentColor);
    Assert.Equal("Ready to review.", viewModel.StatusText);
    Assert.Equal("Review Gently", viewModel.ReviewPanelTitle);
    Assert.Contains("small wins", viewModel.ReviewPanelText);
    Assert.Equal("Week summary not loaded yet.", viewModel.WeekSummaryText);
    Assert.Equal("Small wins not loaded yet.", viewModel.SmallWinsText);
    Assert.Equal("Stuck items not loaded yet.", viewModel.StuckItemsText);
    Assert.Equal("Needs-decision items not loaded yet.", viewModel.NeedsDecisionText);
    Assert.Equal("No next focus selected.", viewModel.NextFocusText);
    Assert.Equal("Review is local-first and not saved yet.", viewModel.SaveReviewStatusText);
    Assert.Equal("Review will show progress after local activity is loaded.", viewModel.EmptyStateText);
    Assert.Equal("0 review items", viewModel.ReviewCountText);
    Assert.False(viewModel.IsLoadingReview, "Review should start idle.");
    Assert.Equal(0, viewModel.SmallWins.Count);
    Assert.Equal(0, viewModel.StuckItems.Count);
    Assert.Equal(0, viewModel.NeedsDecisionItems.Count);
    Assert.False(viewModel.HasReviewData, "Review should wait for loaded summary data.");
    Assert.False(viewModel.HasSmallWins, "Small wins should wait for Review loading.");
    Assert.False(viewModel.HasStuckItems, "Stuck items should wait for Review loading.");
    Assert.False(viewModel.HasNeedsDecisionItems, "Needs-decision items should wait for Review loading.");
    Assert.False(viewModel.HasNextFocus, "Next focus should wait for user selection.");
    Assert.Null(viewModel.SelectedNextFocusKind, "Next focus kind should start empty.");
    Assert.Null(viewModel.SelectedNextFocusId, "Next focus id should start empty.");
    Assert.Equal(string.Empty, viewModel.SelectedNextFocusTitle);
    Assert.Equal(string.Empty, viewModel.SelectedNextFocusSourceText);
    Assert.Null(viewModel.LastSavedNextFocusId, "No review focus should be saved yet.");
    Assert.False(viewModel.HasSavedNextFocus, "Review focus should start unsaved.");
    Assert.False(viewModel.CanSaveReview, "Review should not save before a next focus is selected.");
    Assert.False(viewModel.IsSavingReview, "Review should not start in a saving state.");
    Assert.False(viewModel.SaveReviewCommand.CanExecute(null), "Save Review should wait for next focus selection.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available when Review is idle.");
}

static void ReviewViewModelLoadsSmallWins()
{
    DateTimeOffset oldTime = new(2026, 5, 29, 10, 0, 0, TimeSpan.Zero);
    DateTimeOffset recentTime = new(2026, 5, 30, 14, 0, 0, TimeSpan.Zero);
    NoteItem oldWin = CreateReviewHighlight("Small win: Sketch the idea", oldTime);
    NoteItem recentWin = CreateReviewHighlight("Small win: Finish active task", recentTime);
    NoteItem ignoredNote = NoteItem.Rehydrate(
        Guid.NewGuid(),
        NoteOwnerType.Task,
        Guid.NewGuid(),
        "Not marked for review",
        recentTime.AddMinutes(1),
        recentTime.AddMinutes(1));
    ReviewViewModel viewModel = new(_ => Task.FromResult<IReadOnlyList<NoteItem>>([oldWin, ignoredNote, recentWin]));

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.HasReviewData, "Review should show data after loading small wins.");
    Assert.True(viewModel.HasSmallWins, "Review should expose loaded small wins.");
    Assert.Equal(2, viewModel.SmallWins.Count);
    Assert.Equal(recentWin.Id, viewModel.SmallWins[0].Id);
    Assert.Equal("Small win: Finish active task", viewModel.SmallWins[0].Text);
    Assert.Equal(oldWin.Id, viewModel.SmallWins[1].Id);
    Assert.Equal("2 review items", viewModel.ReviewCountText);
    Assert.Equal("2 small wins this review.", viewModel.WeekSummaryText);
    Assert.Equal("2 small wins ready.", viewModel.SmallWinsText);
    Assert.Equal("Small wins ready.", viewModel.StatusText);
    Assert.Contains("local completions", viewModel.EmptyStateText);
    Assert.False(viewModel.IsLoadingReview, "Loading flag should clear after small wins load.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should return after loading.");
}

static void ReviewViewModelLoadsFocusAndMovementSuccessItems()
{
    DateTimeOffset startedAt = new(2026, 5, 30, 14, 0, 0, TimeSpan.Zero);
    FocusSession completedFocus = FocusSession.Rehydrate(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Draft the Review feed",
        20,
        FocusSessionStatus.Completed,
        startedAt,
        completedAt: startedAt.AddMinutes(20),
        endedAt: startedAt.AddMinutes(20),
        actualFocusMinutes: 20,
        winNote: "Completed 20 minute focus: Draft the Review feed",
        tags: ["Win"]);
    FocusSession blockedFocus = FocusSession.Rehydrate(
        Guid.NewGuid(),
        Guid.NewGuid(),
        "Blocked focus",
        20,
        FocusSessionStatus.Blocked,
        startedAt.AddHours(1),
        endedAt: startedAt.AddHours(1).AddMinutes(4),
        actualFocusMinutes: 4,
        blockedReason: "Need a decision.",
        tags: ["Blocked"]);
    MovementSession plannedMovement = MovementSession.Rehydrate(
        Guid.NewGuid(),
        MovementActivityType.Walk,
        "Walk",
        20,
        MovementSessionStatus.Planned,
        startedAt.AddHours(2),
        scheduledFor: startedAt.AddHours(3));
    MovementSession activeMovement = MovementSession.Rehydrate(
        Guid.NewGuid(),
        MovementActivityType.Workout,
        "Workout",
        20,
        MovementSessionStatus.Active,
        startedAt.AddHours(4),
        startedAt: startedAt.AddHours(4).AddMinutes(5));
    MovementSession completedMovement = MovementSession.Rehydrate(
        Guid.NewGuid(),
        MovementActivityType.Stretch,
        "Stretch",
        20,
        MovementSessionStatus.Completed,
        startedAt.AddHours(5),
        actualMinutes: 18,
        startedAt: startedAt.AddHours(5).AddMinutes(5),
        completedAt: startedAt.AddHours(5).AddMinutes(23),
        endedAt: startedAt.AddHours(5).AddMinutes(23),
        winNote: "Movement completed: stretch break",
        tags: ["Win"]);
    MovementSession skippedMovement = MovementSession.Rehydrate(
        Guid.NewGuid(),
        MovementActivityType.Walk,
        "Skipped walk",
        20,
        MovementSessionStatus.Skipped,
        startedAt.AddHours(6),
        endedAt: startedAt.AddHours(6).AddMinutes(2));
    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([]),
        _ => Task.FromResult<IReadOnlyList<FocusSession>>([blockedFocus, completedFocus]),
        _ => Task.FromResult<IReadOnlyList<MovementSession>>([skippedMovement, plannedMovement, activeMovement, completedMovement]),
        (_, _) => Task.CompletedTask);

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.HasReviewData, "Review should show data after loading focus and movement wins.");
    Assert.True(viewModel.HasSmallWins, "Focus and movement successes should appear in the small wins lane.");
    Assert.Equal(4, viewModel.SmallWins.Count);
    Assert.True(
        viewModel.SmallWins.Any(card => card.OwnerType == NoteOwnerType.FocusSession
            && card.SourceText == "Focus win"
            && card.Text == "Completed 20 minute focus: Draft the Review feed"),
        "Completed focus session should appear as a focus win.");
    Assert.True(
        viewModel.SmallWins.Any(card => card.OwnerType == NoteOwnerType.MovementSession
            && card.Text == "Movement planned: Walk"),
        "Planned movement should appear as a movement success.");
    Assert.True(
        viewModel.SmallWins.Any(card => card.OwnerType == NoteOwnerType.MovementSession
            && card.Text == "Movement started: Workout"),
        "Started movement should appear as a movement success.");
    Assert.True(
        viewModel.SmallWins.Any(card => card.OwnerType == NoteOwnerType.MovementSession
            && card.Text == "Movement completed: stretch break"),
        "Completed movement should appear as a movement success.");
    Assert.False(
        viewModel.SmallWins.Any(card => card.Id == blockedFocus.Id || card.Id == skippedMovement.Id),
        "Blocked focus and skipped movement should not appear as successes.");
    Assert.Equal("4 review items", viewModel.ReviewCountText);
    Assert.Equal("4 small wins this review.", viewModel.WeekSummaryText);
    Assert.Equal("4 small wins ready.", viewModel.SmallWinsText);
    Assert.Equal("Small wins ready.", viewModel.StatusText);
}

static void ReviewViewModelLoadsStuckItems()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem blocked = CreateTask("Call the supplier", dueDate: today.AddDays(-1), priority: TaskPriority.High);
    blocked.UpdateNotes("Waiting on a return call.");
    blocked.MarkBlocked();
    TaskItem repeatedSnooze = CreateTask("Review insurance paperwork", dueDate: today.AddDays(2));
    repeatedSnooze.AddTag("Repeated Snooze");
    TaskItem ordinaryTask = CreateTask("Ordinary planned task", dueDate: today);
    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([ordinaryTask, repeatedSnooze, blocked]));

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.HasReviewData, "Review should show data after loading stuck items.");
    Assert.True(viewModel.HasStuckItems, "Review should expose loaded stuck items.");
    Assert.False(viewModel.HasSmallWins, "This test should isolate stuck items from wins.");
    Assert.Equal(2, viewModel.StuckItems.Count);
    Assert.Equal(blocked.Id, viewModel.StuckItems[0].Id);
    Assert.Equal("Blocked", viewModel.StuckItems[0].StatusText);
    Assert.Equal(repeatedSnooze.Id, viewModel.StuckItems[1].Id);
    Assert.Equal("Repeated Snooze", viewModel.StuckItems[1].StatusText);
    Assert.Equal("2 review items", viewModel.ReviewCountText);
    Assert.Equal("2 stuck items this review.", viewModel.WeekSummaryText);
    Assert.Equal("2 stuck items ready.", viewModel.StuckItemsText);
    Assert.Equal("Stuck items ready.", viewModel.StatusText);
    Assert.Contains("local task status", viewModel.EmptyStateText);
    Assert.False(viewModel.IsLoadingReview, "Loading flag should clear after stuck items load.");
}

static void ReviewViewModelLoadsNeedsDecisionItems()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem highDecision = CreateTask("Choose the project scope", dueDate: today.AddDays(1), priority: TaskPriority.High);
    highDecision.UpdateNotes("Pick the smallest shippable shape.");
    highDecision.AddTag("Needs Decision");
    TaskItem normalDecision = CreateTask("Decide notebook layout", dueDate: today.AddDays(2));
    normalDecision.AddTag("Decision Needed");
    TaskItem doneDecision = CreateTask("Already decided", dueDate: today, done: true);
    doneDecision.AddTag("Needs Decision");
    TaskItem ordinaryTask = CreateTask("Ordinary planned task", dueDate: today);
    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([ordinaryTask, normalDecision, doneDecision, highDecision]));

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();

    Assert.True(viewModel.HasReviewData, "Review should show data after loading needs-decision items.");
    Assert.True(viewModel.HasNeedsDecisionItems, "Review should expose loaded needs-decision items.");
    Assert.False(viewModel.HasSmallWins, "This test should isolate needs-decision items from wins.");
    Assert.False(viewModel.HasStuckItems, "This test should isolate needs-decision items from stuck items.");
    Assert.Equal(2, viewModel.NeedsDecisionItems.Count);
    Assert.Equal(highDecision.Id, viewModel.NeedsDecisionItems[0].Id);
    Assert.Equal("Needs Decision", viewModel.NeedsDecisionItems[0].BadgeText);
    Assert.Equal(normalDecision.Id, viewModel.NeedsDecisionItems[1].Id);
    Assert.Equal("2 review items", viewModel.ReviewCountText);
    Assert.Equal("2 needs-decision items this review.", viewModel.WeekSummaryText);
    Assert.Equal("2 needs-decision items ready.", viewModel.NeedsDecisionText);
    Assert.Equal("Needs-decision items ready.", viewModel.StatusText);
    Assert.Contains("local task tags", viewModel.EmptyStateText);
    Assert.False(viewModel.IsLoadingReview, "Loading flag should clear after needs-decision items load.");
}

static void ReviewViewModelLetsUserChooseNextFocus()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem decision = CreateTask("Choose the project scope", dueDate: today.AddDays(1), priority: TaskPriority.High);
    decision.UpdateNotes("Pick the smallest shippable shape.");
    decision.AddTag("Needs Decision");
    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([decision]),
        (_, _) => Task.CompletedTask);

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();
    ReviewNeedsDecisionItemViewModel card = viewModel.NeedsDecisionItems[0];
    Assert.True(card.ChooseNextFocusCommand.CanExecute(null), "Review cards should expose a choose-next-focus command.");

    card.ChooseNextFocusCommand.Execute(null);

    Assert.True(viewModel.HasNextFocus, "Choosing a review card should set the next focus.");
    Assert.Equal(ReviewNextFocusKind.NeedsDecision, viewModel.SelectedNextFocusKind.GetValueOrDefault());
    Assert.Equal(decision.Id, viewModel.SelectedNextFocusId.GetValueOrDefault());
    Assert.Equal("Choose the project scope", viewModel.SelectedNextFocusTitle);
    Assert.Equal("Needs Decision", viewModel.SelectedNextFocusSourceText);
    Assert.Contains("Choose the project scope", viewModel.NextFocusText);
    Assert.Equal("Next focus ready to save locally.", viewModel.SaveReviewStatusText);
    Assert.Equal("Next focus selected.", viewModel.StatusText);
    Assert.False(viewModel.HasSavedNextFocus, "Changing next focus should clear saved state.");
    Assert.Null(viewModel.LastSavedNextFocusId, "Changing next focus should clear the saved id.");
    Assert.True(viewModel.CanSaveReview, "Choosing a next focus should unlock save.");
    Assert.True(viewModel.SaveReviewCommand.CanExecute(null), "Choosing a next focus should enable Save Review.");
}

static void ReviewViewModelSavesNextFocus()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem stuckTask = CreateTask("Call the supplier", dueDate: today.AddDays(-1), priority: TaskPriority.High);
    stuckTask.UpdateNotes("Waiting on a return call.");
    stuckTask.MarkBlocked();
    List<ReviewNextFocusSelection> savedSelections = [];
    ReviewViewModel viewModel = new(
        _ => Task.FromResult<IReadOnlyList<NoteItem>>([]),
        _ => Task.FromResult<IReadOnlyList<TaskItem>>([stuckTask]),
        (selection, _) =>
        {
            savedSelections.Add(selection);
            return Task.CompletedTask;
        });

    viewModel.SaveReviewAsync(CancellationToken.None).GetAwaiter().GetResult();
    Assert.Equal(0, savedSelections.Count);
    Assert.Equal("Choose one review item before saving.", viewModel.SaveReviewStatusText);
    Assert.Equal("Choose a next focus first.", viewModel.StatusText);

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();
    viewModel.StuckItems[0].ChooseNextFocusCommand.Execute(null);
    viewModel.SaveReviewAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal(1, savedSelections.Count);
    Assert.Equal(ReviewNextFocusKind.StuckItem, savedSelections[0].Kind);
    Assert.Equal(stuckTask.Id, savedSelections[0].ItemId);
    Assert.Equal("Call the supplier", savedSelections[0].Title);
    Assert.Equal("Blocked", savedSelections[0].SourceText);
    Assert.True(viewModel.HasSavedNextFocus, "Successful save should expose saved focus state.");
    Assert.Equal(stuckTask.Id, viewModel.LastSavedNextFocusId.GetValueOrDefault());
    Assert.Equal("Saved locally: Call the supplier", viewModel.SaveReviewStatusText);
    Assert.Equal("Next focus saved.", viewModel.StatusText);
    Assert.False(viewModel.IsSavingReview, "Saving flag should clear after save.");
    Assert.False(viewModel.CanSaveReview, "Saved focus should disable another save until the focus changes.");
    Assert.False(viewModel.SaveReviewCommand.CanExecute(null), "Saved focus should disable Save Review until the focus changes.");
}

static void ReviewViewModelShowsEmptySmallWinsState()
{
    ReviewViewModel viewModel = new(_ => Task.FromResult<IReadOnlyList<NoteItem>>([]));

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasReviewData, "Empty load should not report review data.");
    Assert.False(viewModel.HasSmallWins, "Empty load should not report small wins.");
    Assert.Equal(0, viewModel.SmallWins.Count);
    Assert.Equal(0, viewModel.StuckItems.Count);
    Assert.Equal(0, viewModel.NeedsDecisionItems.Count);
    Assert.Equal("0 review items", viewModel.ReviewCountText);
    Assert.Equal("No local review items loaded yet.", viewModel.WeekSummaryText);
    Assert.Equal("No small wins yet.", viewModel.SmallWinsText);
    Assert.Equal("No stuck items yet.", viewModel.StuckItemsText);
    Assert.Equal("No needs-decision items yet.", viewModel.NeedsDecisionText);
    Assert.Equal("No review items yet.", viewModel.StatusText);
    Assert.Contains("Complete one small task", viewModel.EmptyStateText);
}

static void ReviewViewModelReportsLoadFailures()
{
    ReviewViewModel viewModel = new(_ => throw new InvalidOperationException("Review storage unavailable."));

    viewModel.LoadReviewAsync().GetAwaiter().GetResult();

    Assert.False(viewModel.HasReviewData, "Failed load should not leave review data visible.");
    Assert.False(viewModel.HasSmallWins, "Failed load should clear small wins.");
    Assert.False(viewModel.HasStuckItems, "Failed load should clear stuck items.");
    Assert.False(viewModel.HasNeedsDecisionItems, "Failed load should clear needs-decision items.");
    Assert.Equal(0, viewModel.SmallWins.Count);
    Assert.Equal(0, viewModel.StuckItems.Count);
    Assert.Equal(0, viewModel.NeedsDecisionItems.Count);
    Assert.Equal("0 review items", viewModel.ReviewCountText);
    Assert.Equal("Review could not load.", viewModel.StatusText);
    Assert.Equal("Review highlights could not load.", viewModel.WeekSummaryText);
    Assert.Equal("Small wins could not load.", viewModel.SmallWinsText);
    Assert.Equal("Stuck items could not load.", viewModel.StuckItemsText);
    Assert.Equal("Needs-decision items could not load.", viewModel.NeedsDecisionText);
    Assert.Contains("Try Review again", viewModel.EmptyStateText);
    Assert.False(viewModel.IsLoadingReview, "Loading flag should clear after failure.");
    Assert.True(viewModel.RefreshCommand.CanExecute(null), "Refresh should be available after failure.");
}

static void ReviewViewModelCreatesSmallWinDisplayState()
{
    DateTimeOffset createdAt = new(2026, 5, 30, 14, 0, 0, TimeSpan.Zero);
    NoteItem note = CreateReviewHighlight("Small win: Finish active task", createdAt);

    ReviewSmallWinViewModel card = ReviewSmallWinViewModel.FromNote(note);

    Assert.Equal(note.Id, card.Id);
    Assert.Equal(NoteOwnerType.Task, card.OwnerType);
    Assert.Equal(note.OwnerId.GetValueOrDefault(), card.OwnerId.GetValueOrDefault());
    Assert.Equal("Small win: Finish active task", card.Text);
    Assert.Equal(createdAt, card.CreatedAt);
    Assert.Equal("Task win", card.SourceText);
    Assert.True(card.HasSource, "Small win card should expose its source.");
    Assert.Equal("Small Win", card.BadgeText);
    Assert.Equal("\uE73E", card.CardIconGlyph);
    Assert.Contains("Finish active task", card.CardToolTip);
}

static void ReviewViewModelCreatesStuckItemDisplayState()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem blocked = CreateTask("Call the supplier", dueDate: today.AddDays(-1), priority: TaskPriority.Critical);
    blocked.UpdateNotes("Waiting on a return call before the next action.");
    blocked.MarkBlocked();

    ReviewStuckItemViewModel card = ReviewStuckItemViewModel.FromTask(blocked);

    Assert.Equal(blocked.Id, card.Id);
    Assert.Equal("Call the supplier", card.Title);
    Assert.Equal(TaskItemStatus.Blocked, card.Status);
    Assert.Equal(TaskPriority.Critical, card.Priority);
    Assert.Equal("Blocked", card.StatusText);
    Assert.Equal("Critical priority", card.PriorityText);
    Assert.Equal("Due May 29", card.DateText);
    Assert.True(card.HasNotes, "Stuck item card should expose notes when notes exist.");
    Assert.Contains("return call", card.NotesPreview);
    Assert.Equal("Blocked", card.BadgeText);
    Assert.Equal("\uE7BA", card.CardIconGlyph);
    Assert.Contains("Call the supplier", card.CardToolTip);
}

static void ReviewViewModelCreatesNeedsDecisionDisplayState()
{
    DateOnly today = new(2026, 5, 30);
    TaskItem decision = CreateTask("Choose the project scope", dueDate: today.AddDays(1), priority: TaskPriority.High);
    decision.UpdateNotes("Pick the smallest shippable shape.");
    decision.AddTag("Needs Decision");

    ReviewNeedsDecisionItemViewModel card = ReviewNeedsDecisionItemViewModel.FromTask(decision);

    Assert.Equal(decision.Id, card.Id);
    Assert.Equal("Choose the project scope", card.Title);
    Assert.Equal(TaskPriority.High, card.Priority);
    Assert.Equal("High priority", card.PriorityText);
    Assert.Equal("Due May 31", card.DateText);
    Assert.True(card.HasNotes, "Needs-decision card should expose notes when notes exist.");
    Assert.Contains("smallest shippable", card.NotesPreview);
    Assert.Equal("Needs Decision", card.BadgeText);
    Assert.Equal("\uE9CE", card.CardIconGlyph);
    Assert.Contains("Choose the project scope", card.CardToolTip);
}

static void FindViewModelStartsReady()
{
    FindViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Find, viewModel.Descriptor);
    Assert.Equal("Find", viewModel.Title);
    Assert.Equal("Search local tasks and notes.", viewModel.Subtitle);
    Assert.Equal("\uE721", viewModel.IconGlyph);
    Assert.Equal("#FFFFFF", viewModel.AccentColor);
    Assert.Equal(string.Empty, viewModel.SearchText);
    Assert.Equal("Ready to find.", viewModel.StatusText);
    Assert.Equal("Find Local Data", viewModel.SearchPanelTitle);
    Assert.Contains("tasks, projects, notes", viewModel.SearchPanelText);
    Assert.Equal("Type a word or phrase to search local data.", viewModel.EmptyStateText);
    Assert.Equal("0 results", viewModel.ResultsCountText);
    Assert.Equal(0, viewModel.Results.Count);
    Assert.False(viewModel.HasOpenedItem, "Find should start without an opened item.");
    Assert.Null(viewModel.OpenedResult, "Find should not have an opened result at startup.");
    Assert.Equal("No Item Open", viewModel.OpenedItemPanelTitle);
    Assert.Contains("opened here", viewModel.OpenedItemPanelText);
    Assert.False(viewModel.HasQuery, "Find should start without a query.");
    Assert.False(viewModel.HasResults, "Find should start without results.");
    Assert.False(viewModel.CanSearch, "Find should wait for a query before search is enabled.");
    Assert.False(viewModel.IsSearching, "Find should start idle.");
    Assert.False(viewModel.SearchCommand.CanExecute(null), "Search command should wait for a query.");
    Assert.False(viewModel.ClearSearchCommand.CanExecute(null), "Clear command should wait for query or results.");
}

static void FindViewModelUpdatesQueryState()
{
    FindViewModel viewModel = new();

    viewModel.SearchText = "  pharmacy  ";

    Assert.True(viewModel.HasQuery, "Nonblank search text should count as a query.");
    Assert.True(viewModel.CanSearch, "Nonblank search text should enable search.");
    Assert.True(viewModel.SearchCommand.CanExecute(null), "Search command should enable with a query.");
    Assert.True(viewModel.ClearSearchCommand.CanExecute(null), "Clear command should enable with a query.");
    Assert.Equal("Ready to search locally.", viewModel.StatusText);
    Assert.Equal("Ready To Search", viewModel.SearchPanelTitle);
    Assert.Equal("Find local matches for \"pharmacy\".", viewModel.SearchPanelText);
    Assert.Contains("without Microsoft sync", viewModel.EmptyStateText);
}

static void FindViewModelClearsSearchState()
{
    FindViewModel viewModel = new((_, _) => Task.FromResult<IReadOnlyList<LocalSearchResult>>(
        [CreateSearchResult(LocalSearchResultKind.Task, "Call the pharmacy", "Task")]))
    {
        SearchText = "pharmacy"
    };
    viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();
    Assert.True(viewModel.HasResults, "Search should create results before clear.");

    viewModel.ClearSearchAsync().GetAwaiter().GetResult();

    Assert.Equal(string.Empty, viewModel.SearchText);
    Assert.False(viewModel.HasQuery, "Clear should remove the query.");
    Assert.False(viewModel.CanSearch, "Clear should disable search.");
    Assert.False(viewModel.SearchCommand.CanExecute(null), "Search command should disable after clear.");
    Assert.False(viewModel.ClearSearchCommand.CanExecute(null), "Clear command should disable after clear.");
    Assert.Equal("Search cleared.", viewModel.StatusText);
    Assert.Equal("Find Local Data", viewModel.SearchPanelTitle);
    Assert.Equal("Type a word or phrase to search local data.", viewModel.EmptyStateText);
    Assert.Equal("0 results", viewModel.ResultsCountText);
    Assert.Equal(0, viewModel.Results.Count);
    Assert.False(viewModel.HasOpenedItem, "Clear should close the opened local item.");
    Assert.Null(viewModel.OpenedResult, "Clear should remove the opened result.");
}

static void FindViewModelSearchesLocalResults()
{
    string? requestedQuery = null;
    FindViewModel viewModel = new((query, cancellationToken) =>
    {
        cancellationToken.ThrowIfCancellationRequested();
        requestedQuery = query;
        return Task.FromResult<IReadOnlyList<LocalSearchResult>>(
        [
            CreateSearchResult(LocalSearchResultKind.Task, "Call the pharmacy", "Task"),
            CreateSearchResult(LocalSearchResultKind.Note, "Pharmacy note", "Note")
        ]);
    })
    {
        SearchText = "  pharmacy  "
    };

    viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal("pharmacy", requestedQuery);
    Assert.True(viewModel.HasResults, "Search should expose results.");
    Assert.Equal(2, viewModel.Results.Count);
    Assert.Equal("2 results", viewModel.ResultsCountText);
    Assert.Equal("Search complete.", viewModel.StatusText);
    Assert.Equal("Local Matches", viewModel.SearchPanelTitle);
    Assert.Equal("Found local matches for \"pharmacy\".", viewModel.SearchPanelText);
    Assert.Equal("Local matches found.", viewModel.EmptyStateText);
    Assert.Equal(LocalSearchResultKind.Task, viewModel.Results[0].Kind);
    Assert.Equal("Call the pharmacy", viewModel.Results[0].Title);
    Assert.Equal("Task", viewModel.Results[0].BadgeText);
    Assert.True(viewModel.Results[0].OpenCommand.CanExecute(null), "Each result should expose an open command.");
    Assert.True(viewModel.ClearSearchCommand.CanExecute(null), "Results should keep clear enabled.");
    Assert.False(viewModel.IsSearching, "Search should clear the loading flag.");
}

static void FindViewModelOpensSelectedLocalItem()
{
    FindViewModel viewModel = new((_, _) => Task.FromResult<IReadOnlyList<LocalSearchResult>>(
    [
        CreateSearchResult(LocalSearchResultKind.Task, "Call the pharmacy", "Task"),
        CreateSearchResult(LocalSearchResultKind.Project, "Kitchen reset", "Project")
    ]))
    {
        SearchText = "local"
    };
    viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

    FindResultViewModel taskResult = viewModel.Results[0];
    FindResultViewModel projectResult = viewModel.Results[1];

    viewModel.OpenLocalItemAsync(taskResult).GetAwaiter().GetResult();

    Assert.True(viewModel.HasOpenedItem, "Open should expose the selected local item.");
    Assert.True(ReferenceEquals(taskResult, viewModel.OpenedResult), "Open should keep the selected result instance.");
    Assert.True(taskResult.IsOpened, "Opened result should be marked for the view.");
    Assert.Equal("#FFFFFF", taskResult.CardBackgroundColor);
    Assert.Equal("Opened", taskResult.OpenButtonText);
    Assert.Equal("Opened Item", viewModel.OpenedItemPanelTitle);
    Assert.Equal("Task: Call the pharmacy", viewModel.OpenedItemPanelText);
    Assert.Equal("Local item opened.", viewModel.StatusText);
    Assert.False(string.IsNullOrWhiteSpace(taskResult.LocalIdText), "Opened item should expose a local id.");

    viewModel.OpenLocalItemAsync(projectResult).GetAwaiter().GetResult();

    Assert.True(ReferenceEquals(projectResult, viewModel.OpenedResult), "Opening another result should replace the opened item.");
    Assert.False(taskResult.IsOpened, "Opening a different item should clear the previous opened marker.");
    Assert.True(projectResult.IsOpened, "New opened item should be marked for the view.");
    Assert.Equal("Project: Kitchen reset", viewModel.OpenedItemPanelText);
}

static void FindViewModelHandlesNoResults()
{
    FindViewModel viewModel = new((_, _) => Task.FromResult<IReadOnlyList<LocalSearchResult>>([]))
    {
        SearchText = "missing"
    };

    viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.False(viewModel.HasResults, "Empty search result should not report matches.");
    Assert.Equal(0, viewModel.Results.Count);
    Assert.Equal("0 results", viewModel.ResultsCountText);
    Assert.Equal("No local matches.", viewModel.StatusText);
    Assert.Equal("No Matches", viewModel.SearchPanelTitle);
    Assert.Equal("No local matches for \"missing\" yet.", viewModel.SearchPanelText);
    Assert.Equal("No local matches for \"missing\".", viewModel.EmptyStateText);
}

static void FindViewModelReportsSearchFailures()
{
    FindViewModel viewModel = new((_, _) => throw new InvalidOperationException("Search unavailable."))
    {
        SearchText = "notes"
    };

    viewModel.SearchAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.False(viewModel.HasResults, "Failed search should not leave stale results.");
    Assert.Equal("0 results", viewModel.ResultsCountText);
    Assert.Equal("Find could not search.", viewModel.StatusText);
    Assert.Equal("Try the local search again.", viewModel.EmptyStateText);
}

static LocalSearchResult CreateSearchResult(
    LocalSearchResultKind kind,
    string title,
    string sourceText)
{
    return new LocalSearchResult(
        kind,
        Guid.NewGuid(),
        title,
        $"{sourceText} detail",
        sourceText,
        new DateTimeOffset(2026, 6, 1, 10, 0, 0, TimeSpan.Zero),
        LocalItemType: sourceText,
        LocalItemId: Guid.NewGuid());
}

static void MoveViewModelStartsReady()
{
    MoveViewModel viewModel = new();

    Assert.Equal(ScreenCatalog.Move, viewModel.Descriptor);
    Assert.Equal("Move", viewModel.Title);
    Assert.Equal("Walk, stretch, or work out.", viewModel.Subtitle);
    Assert.Equal("\uE805", viewModel.IconGlyph);
    Assert.Equal("#FFBE7A", viewModel.AccentColor);
    Assert.Equal("Ready to plan movement.", viewModel.StatusText);
    Assert.Equal("Choose Movement", viewModel.MovementPanelTitle);
    Assert.Contains("Pick a small physical activity", viewModel.MovementPanelText);
    Assert.Equal("No movement selected.", viewModel.SelectedActivityText);
    Assert.Equal("Not scheduled yet.", viewModel.TimingText);
    Assert.Equal("No mind occupier selected.", viewModel.MindOccupierText);
    Assert.Equal("Solo movement.", viewModel.SpouseText);
    Assert.Equal("No movement plan created yet.", viewModel.MovementDraftStatusText);
    Assert.Equal(MovementSession.DefaultPlannedMinutes, viewModel.PlannedMinutes);
    Assert.Equal("20 minute movement", viewModel.PlannedMinutesText);
    Assert.Null(viewModel.SelectedActivityType, "Move should wait for a movement activity choice.");
    Assert.False(viewModel.HasSelectedActivity, "Move should start without a selected activity.");
    Assert.False(viewModel.HasMovementDraft, "Move should start without a movement draft.");
    Assert.Null(viewModel.SelectedTimingChoice, "Move should wait for a timing choice.");
    Assert.Null(viewModel.SelectedScheduledFor, "Move should start without a scheduled movement time.");
    Assert.Null(viewModel.SelectedMindOccupierChoice, "Move should wait for a mind occupier choice.");
    Assert.Null(viewModel.SelectedSpouseChoice, "Move should wait for a spouse option choice.");
    Assert.Null(viewModel.LastSavedMovementSessionId, "Move should start without a saved movement session.");
    Assert.False(viewModel.HasSelectedTiming, "Move should start without selected timing.");
    Assert.False(viewModel.HasSelectedMindOccupier, "Move should start without a mind occupier.");
    Assert.False(viewModel.HasSelectedSpouseOption, "Move should start without a spouse option.");
    Assert.False(viewModel.HasSavedMovementSession, "Move should start without saved movement state.");
    Assert.False(viewModel.IsSavingMovement, "Move should not start in a saving state.");
    Assert.False(viewModel.CanChooseTiming, "Timing choices should wait for a movement activity.");
    Assert.False(viewModel.CanChooseMindOccupier, "Mind occupier choices should wait for timing.");
    Assert.False(viewModel.CanChooseSpouseOption, "Spouse option should wait for mind occupier.");
    Assert.False(viewModel.CanSaveMovement, "Save should wait for every movement choice.");
    Assert.False(viewModel.IsWalkSelected, "Walk should start unselected.");
    Assert.False(viewModel.IsWorkoutSelected, "Workout should start unselected.");
    Assert.False(viewModel.IsStretchSelected, "Stretch should start unselected.");
    Assert.False(viewModel.IsNowSelected, "Now should start unselected.");
    Assert.False(viewModel.IsScheduleSelected, "Schedule should start unselected.");
    Assert.False(viewModel.IsMusicSelected, "Music should start unselected.");
    Assert.False(viewModel.IsPodcastSelected, "Podcast should start unselected.");
    Assert.False(viewModel.IsAudiobookSelected, "Audiobook should start unselected.");
    Assert.False(viewModel.IsSoloSelected, "Solo should start unselected.");
    Assert.False(viewModel.IsWithSpouseSelected, "With Spouse should start unselected.");
    Assert.Equal("Choose", viewModel.WalkChoiceStatusText);
    Assert.Equal("Choose", viewModel.WorkoutChoiceStatusText);
    Assert.Equal("Choose", viewModel.StretchChoiceStatusText);
    Assert.Equal("Pick activity", viewModel.NowChoiceStatusText);
    Assert.Equal("Pick activity", viewModel.ScheduleChoiceStatusText);
    Assert.Equal("Pick timing", viewModel.MusicChoiceStatusText);
    Assert.Equal("Pick timing", viewModel.PodcastChoiceStatusText);
    Assert.Equal("Pick timing", viewModel.AudiobookChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.SoloChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.WithSpouseChoiceStatusText);
    Assert.True(viewModel.ChooseWalkCommand.CanExecute(null), "Walk choice should be available.");
    Assert.True(viewModel.ChooseWorkoutCommand.CanExecute(null), "Workout choice should be available.");
    Assert.True(viewModel.ChooseStretchCommand.CanExecute(null), "Stretch choice should be available.");
    Assert.False(viewModel.ChooseNowCommand.CanExecute(null), "Now should wait for a selected activity.");
    Assert.False(viewModel.ChooseScheduleCommand.CanExecute(null), "Schedule should wait for a selected activity.");
    Assert.False(viewModel.ChooseMusicCommand.CanExecute(null), "Music should wait for timing.");
    Assert.False(viewModel.ChoosePodcastCommand.CanExecute(null), "Podcast should wait for timing.");
    Assert.False(viewModel.ChooseAudiobookCommand.CanExecute(null), "Audiobook should wait for timing.");
    Assert.False(viewModel.ChooseSoloCommand.CanExecute(null), "Solo should wait for mind occupier.");
    Assert.False(viewModel.ChooseWithSpouseCommand.CanExecute(null), "With Spouse should wait for mind occupier.");
    Assert.False(viewModel.SaveMovementCommand.CanExecute(null), "Save should wait for all movement choices.");
    Assert.Equal("Choose one small movement activity.", viewModel.EmptyStateText);
    Assert.Equal("Movement is local-first and not saved yet.", viewModel.SaveStatusText);

    viewModel.OnNavigatedToAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal("Ready to plan movement.", viewModel.StatusText);
}

static void MoveViewModelAppliesMovementActivityChoices()
{
    MoveViewModel viewModel = new();

    viewModel.ChooseWalkCommand.Execute(null);

    AssertMovementChoice(
        viewModel,
        MovementActivityType.Walk,
        "Walk",
        walkSelected: true,
        workoutSelected: false,
        stretchSelected: false);

    viewModel.ChooseWorkoutCommand.Execute(null);

    AssertMovementChoice(
        viewModel,
        MovementActivityType.Workout,
        "Workout",
        walkSelected: false,
        workoutSelected: true,
        stretchSelected: false);

    viewModel.ChooseStretchCommand.Execute(null);

    AssertMovementChoice(
        viewModel,
        MovementActivityType.Stretch,
        "Stretch",
        walkSelected: false,
        workoutSelected: false,
        stretchSelected: true);
}

static void MoveViewModelAppliesNowAndScheduleChoices()
{
    DateTimeOffset now = new(2026, 5, 31, 14, 15, 0, TimeSpan.FromHours(-5));
    MoveViewModel viewModel = new(() => now);

    Assert.False(viewModel.ChooseNowCommand.CanExecute(null), "Now should wait for an activity.");
    Assert.False(viewModel.ChooseScheduleCommand.CanExecute(null), "Schedule should wait for an activity.");

    viewModel.ChooseWalkCommand.Execute(null);

    Assert.True(viewModel.ChooseNowCommand.CanExecute(null), "Now should unlock after activity selection.");
    Assert.True(viewModel.ChooseScheduleCommand.CanExecute(null), "Schedule should unlock after activity selection.");
    Assert.Equal("Choose", viewModel.NowChoiceStatusText);
    Assert.Equal("Choose", viewModel.ScheduleChoiceStatusText);

    viewModel.ChooseNowCommand.Execute(null);

    Assert.Equal(MovementTimingChoice.Now, viewModel.SelectedTimingChoice);
    Assert.Equal(now, viewModel.SelectedScheduledFor);
    Assert.True(viewModel.HasSelectedTiming, "Now should count as selected movement timing.");
    Assert.True(viewModel.IsNowSelected, "Now button should become selected.");
    Assert.False(viewModel.IsScheduleSelected, "Schedule should clear when Now is selected.");
    Assert.Equal("Now selected.", viewModel.TimingText);
    Assert.Equal("Walk Now", viewModel.MovementPanelTitle);
    Assert.Equal("Walk is ready to start now for 20 minutes.", viewModel.MovementPanelText);
    Assert.Equal("Walk now draft ready.", viewModel.MovementDraftStatusText);
    Assert.Equal("Walk set for now.", viewModel.StatusText);
    Assert.Equal("Selected", viewModel.NowChoiceStatusText);
    Assert.Equal("Choose", viewModel.ScheduleChoiceStatusText);
    Assert.Equal("Choose", viewModel.MusicChoiceStatusText);
    Assert.Equal("Choose", viewModel.PodcastChoiceStatusText);
    Assert.Equal("Choose", viewModel.AudiobookChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.SoloChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.WithSpouseChoiceStatusText);
    Assert.Equal("Choose a mind occupier next.", viewModel.EmptyStateText);

    viewModel.ChooseScheduleCommand.Execute(null);

    DateTimeOffset scheduledFor = now.AddHours(1);
    Assert.Equal(MovementTimingChoice.Schedule, viewModel.SelectedTimingChoice);
    Assert.Equal(scheduledFor, viewModel.SelectedScheduledFor);
    Assert.False(viewModel.IsNowSelected, "Now should clear when Schedule is selected.");
    Assert.True(viewModel.IsScheduleSelected, "Schedule button should become selected.");
    Assert.Equal("Scheduled for May 31, 3:15 PM.", viewModel.TimingText);
    Assert.Equal("Walk Scheduled", viewModel.MovementPanelTitle);
    Assert.Equal("Walk is scheduled for May 31, 3:15 PM.", viewModel.MovementPanelText);
    Assert.Equal("Walk scheduled draft ready.", viewModel.MovementDraftStatusText);
    Assert.Equal("Walk scheduled.", viewModel.StatusText);
    Assert.Equal("Choose", viewModel.NowChoiceStatusText);
    Assert.Equal("Selected", viewModel.ScheduleChoiceStatusText);
    Assert.Equal("Choose", viewModel.MusicChoiceStatusText);
    Assert.Equal("Choose", viewModel.PodcastChoiceStatusText);
    Assert.Equal("Choose", viewModel.AudiobookChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.SoloChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.WithSpouseChoiceStatusText);
    Assert.Equal("Choose a mind occupier next.", viewModel.EmptyStateText);

    viewModel.ChooseStretchCommand.Execute(null);

    Assert.Equal(MovementActivityType.Stretch, viewModel.SelectedActivityType);
    Assert.Equal(MovementTimingChoice.Schedule, viewModel.SelectedTimingChoice);
    Assert.Equal("Stretch Scheduled", viewModel.MovementPanelTitle);
    Assert.Equal("Stretch is scheduled for May 31, 3:15 PM.", viewModel.MovementPanelText);
    Assert.Equal("Stretch scheduled draft ready.", viewModel.MovementDraftStatusText);
}

static void MoveViewModelAppliesMindOccupierChoices()
{
    DateTimeOffset now = new(2026, 5, 31, 14, 15, 0, TimeSpan.FromHours(-5));
    MoveViewModel viewModel = new(() => now);

    Assert.False(viewModel.ChooseMusicCommand.CanExecute(null), "Music should wait for timing.");
    Assert.False(viewModel.ChoosePodcastCommand.CanExecute(null), "Podcast should wait for timing.");
    Assert.False(viewModel.ChooseAudiobookCommand.CanExecute(null), "Audiobook should wait for timing.");

    viewModel.ChooseWalkCommand.Execute(null);

    Assert.False(viewModel.ChooseMusicCommand.CanExecute(null), "Music should still wait for timing.");

    viewModel.ChooseNowCommand.Execute(null);

    Assert.True(viewModel.ChooseMusicCommand.CanExecute(null), "Music should unlock after timing.");
    Assert.True(viewModel.ChoosePodcastCommand.CanExecute(null), "Podcast should unlock after timing.");
    Assert.True(viewModel.ChooseAudiobookCommand.CanExecute(null), "Audiobook should unlock after timing.");

    viewModel.ChooseMusicCommand.Execute(null);

    AssertMindOccupierChoice(
        viewModel,
        MovementMindOccupierChoice.Music,
        "Music",
        "Walk Now",
        "Walk is ready to start now with music.",
        "Walk now with Music ready.",
        musicSelected: true,
        podcastSelected: false,
        audiobookSelected: false);

    viewModel.ChoosePodcastCommand.Execute(null);

    AssertMindOccupierChoice(
        viewModel,
        MovementMindOccupierChoice.Podcast,
        "Podcast",
        "Walk Now",
        "Walk is ready to start now with podcast.",
        "Walk now with Podcast ready.",
        musicSelected: false,
        podcastSelected: true,
        audiobookSelected: false);

    viewModel.ChooseScheduleCommand.Execute(null);

    Assert.Equal("Walk Scheduled", viewModel.MovementPanelTitle);
    Assert.Equal("Walk is scheduled for May 31, 3:15 PM with podcast.", viewModel.MovementPanelText);
    Assert.Equal("Walk scheduled with Podcast ready.", viewModel.MovementDraftStatusText);

    viewModel.ChooseAudiobookCommand.Execute(null);

    AssertMindOccupierChoice(
        viewModel,
        MovementMindOccupierChoice.Audiobook,
        "Audiobook",
        "Walk Scheduled",
        "Walk is scheduled for May 31, 3:15 PM with audiobook.",
        "Walk scheduled with Audiobook ready.",
        musicSelected: false,
        podcastSelected: false,
        audiobookSelected: true);
}

static void MoveViewModelAppliesSpouseOptionChoices()
{
    DateTimeOffset now = new(2026, 5, 31, 14, 15, 0, TimeSpan.FromHours(-5));
    MoveViewModel viewModel = new(() => now);

    Assert.False(viewModel.ChooseSoloCommand.CanExecute(null), "Solo should wait for mind occupier.");
    Assert.False(viewModel.ChooseWithSpouseCommand.CanExecute(null), "With Spouse should wait for mind occupier.");

    viewModel.ChooseWalkCommand.Execute(null);
    viewModel.ChooseScheduleCommand.Execute(null);

    Assert.False(viewModel.ChooseSoloCommand.CanExecute(null), "Solo should still wait for mind occupier.");

    viewModel.ChoosePodcastCommand.Execute(null);

    Assert.True(viewModel.ChooseSoloCommand.CanExecute(null), "Solo should unlock after mind occupier.");
    Assert.True(viewModel.ChooseWithSpouseCommand.CanExecute(null), "With Spouse should unlock after mind occupier.");
    Assert.Equal("Choose", viewModel.SoloChoiceStatusText);
    Assert.Equal("Choose", viewModel.WithSpouseChoiceStatusText);

    viewModel.ChooseSoloCommand.Execute(null);

    AssertSpouseOptionChoice(
        viewModel,
        MovementSpouseChoice.Solo,
        "Solo movement selected.",
        "Solo selected.",
        "Walk is scheduled for May 31, 3:15 PM with podcast as a solo movement.",
        "Walk scheduled with Podcast solo ready.",
        soloSelected: true,
        withSpouseSelected: false);

    viewModel.ChooseWithSpouseCommand.Execute(null);

    AssertSpouseOptionChoice(
        viewModel,
        MovementSpouseChoice.WithSpouse,
        "With spouse selected.",
        "With spouse selected.",
        "Walk is scheduled for May 31, 3:15 PM with podcast and spouse support.",
        "Walk scheduled with Podcast and spouse ready.",
        soloSelected: false,
        withSpouseSelected: true);

    viewModel.ChooseNowCommand.Execute(null);

    Assert.Equal(MovementTimingChoice.Now, viewModel.SelectedTimingChoice);
    Assert.Equal("Walk Now", viewModel.MovementPanelTitle);
    Assert.Equal("Walk is ready to start now with podcast and spouse support.", viewModel.MovementPanelText);
    Assert.Equal("Walk now with Podcast and spouse ready.", viewModel.MovementDraftStatusText);
}

static void MoveViewModelSavesMovementActivityLocally()
{
    DateTimeOffset now = new(2026, 5, 31, 14, 15, 0, TimeSpan.FromHours(-5));
    List<MovementSession> savedSessions = [];
    MoveViewModel viewModel = new(
        (session, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            savedSessions.Add(session);
            return Task.CompletedTask;
        },
        () => now);

    viewModel.SaveMovementAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal(0, savedSessions.Count);
    Assert.Equal("Complete movement choices before saving.", viewModel.SaveStatusText);
    Assert.Equal("Complete movement choices before saving.", viewModel.StatusText);

    viewModel.ChooseWorkoutCommand.Execute(null);
    viewModel.ChooseNowCommand.Execute(null);
    viewModel.ChooseMusicCommand.Execute(null);
    viewModel.ChooseWithSpouseCommand.Execute(null);

    Assert.True(viewModel.CanSaveMovement, "Completed movement choices should unlock save.");
    Assert.True(viewModel.SaveMovementCommand.CanExecute(null), "Completed movement choices should enable save command.");
    Assert.Equal("Ready to save movement locally.", viewModel.SaveStatusText);

    viewModel.SaveMovementAsync(CancellationToken.None).GetAwaiter().GetResult();

    Assert.Equal(1, savedSessions.Count);
    MovementSession savedSession = savedSessions[0];
    Assert.Equal(savedSession.Id, viewModel.LastSavedMovementSessionId.GetValueOrDefault());
    Assert.True(viewModel.HasSavedMovementSession, "Saved movement id should be visible.");
    Assert.Equal(MovementActivityType.Workout, savedSession.ActivityType);
    Assert.Equal("Workout", savedSession.ActivityName);
    Assert.Equal(MovementSessionStatus.Active, savedSession.Status);
    Assert.Equal(MovementSession.DefaultPlannedMinutes, savedSession.PlannedMinutes);
    Assert.Equal(now, savedSession.ScheduledFor);
    Assert.True(savedSession.StartedAt.HasValue, "Now movement should be active after save.");
    Assert.Equal("Music", savedSession.MindOccupier);
    Assert.True(savedSession.IsWithSpouse, "With Spouse choice should persist.");
    Assert.Equal("Mind: Music; Support: With spouse.", savedSession.Notes);
    Assert.Contains("With spouse", savedSession.Tags);
    Assert.Equal("Saved locally: active movement session.", viewModel.SaveStatusText);
    Assert.Equal("Movement started and saved locally.", viewModel.StatusText);

    viewModel.ChooseStretchCommand.Execute(null);

    Assert.False(viewModel.HasSavedMovementSession, "Changing movement draft should clear prior save id.");
    Assert.Equal("Movement draft changed. Save again locally.", viewModel.SaveStatusText);
}

static void AssertSpouseOptionChoice(
    MoveViewModel viewModel,
    MovementSpouseChoice expectedSpouseChoice,
    string expectedSpouseText,
    string expectedStatusText,
    string expectedPanelText,
    string expectedDraftStatus,
    bool soloSelected,
    bool withSpouseSelected)
{
    Assert.Equal(expectedSpouseChoice, viewModel.SelectedSpouseChoice);
    Assert.True(viewModel.HasSelectedSpouseOption, "Spouse option should be visible after selection.");
    Assert.Equal(expectedSpouseText, viewModel.SpouseText);
    Assert.Equal(expectedStatusText, viewModel.StatusText);
    Assert.Equal("Walk Scheduled", viewModel.MovementPanelTitle);
    Assert.Equal(expectedPanelText, viewModel.MovementPanelText);
    Assert.Equal(expectedDraftStatus, viewModel.MovementDraftStatusText);
    Assert.Equal("Ready to save movement activity.", viewModel.EmptyStateText);
    Assert.True(viewModel.CanSaveMovement, "Spouse option should unlock movement save.");
    Assert.True(viewModel.SaveMovementCommand.CanExecute(null), "Spouse option should enable movement save command.");
    Assert.Equal("Ready to save movement locally.", viewModel.SaveStatusText);
    Assert.Equal(soloSelected, viewModel.IsSoloSelected);
    Assert.Equal(withSpouseSelected, viewModel.IsWithSpouseSelected);
    Assert.Equal(soloSelected ? "Selected" : "Choose", viewModel.SoloChoiceStatusText);
    Assert.Equal(withSpouseSelected ? "Selected" : "Choose", viewModel.WithSpouseChoiceStatusText);
}

static void AssertMindOccupierChoice(
    MoveViewModel viewModel,
    MovementMindOccupierChoice expectedMindOccupierChoice,
    string mindOccupierName,
    string expectedPanelTitle,
    string expectedPanelText,
    string expectedDraftStatus,
    bool musicSelected,
    bool podcastSelected,
    bool audiobookSelected)
{
    Assert.Equal(expectedMindOccupierChoice, viewModel.SelectedMindOccupierChoice);
    Assert.True(viewModel.HasSelectedMindOccupier, "Mind occupier should be visible after selection.");
    Assert.Equal($"{mindOccupierName} selected.", viewModel.MindOccupierText);
    Assert.Equal(expectedPanelTitle, viewModel.MovementPanelTitle);
    Assert.Equal(expectedPanelText, viewModel.MovementPanelText);
    Assert.Equal(expectedDraftStatus, viewModel.MovementDraftStatusText);
    Assert.Equal($"{mindOccupierName} selected.", viewModel.StatusText);
    Assert.Equal("Choose spouse option next.", viewModel.EmptyStateText);
    Assert.Null(viewModel.SelectedSpouseChoice, "Mind occupier alone should not choose a spouse option.");
    Assert.False(viewModel.HasSelectedSpouseOption, "Mind occupier alone should not complete spouse option.");
    Assert.True(viewModel.CanChooseSpouseOption, "Mind occupier should unlock spouse option.");
    Assert.Equal(musicSelected, viewModel.IsMusicSelected);
    Assert.Equal(podcastSelected, viewModel.IsPodcastSelected);
    Assert.Equal(audiobookSelected, viewModel.IsAudiobookSelected);
    Assert.Equal(musicSelected ? "Selected" : "Choose", viewModel.MusicChoiceStatusText);
    Assert.Equal(podcastSelected ? "Selected" : "Choose", viewModel.PodcastChoiceStatusText);
    Assert.Equal(audiobookSelected ? "Selected" : "Choose", viewModel.AudiobookChoiceStatusText);
    Assert.Equal("Choose", viewModel.SoloChoiceStatusText);
    Assert.Equal("Choose", viewModel.WithSpouseChoiceStatusText);
}

static void AssertMovementChoice(
    MoveViewModel viewModel,
    MovementActivityType expectedActivityType,
    string activityName,
    bool walkSelected,
    bool workoutSelected,
    bool stretchSelected)
{
    Assert.Equal(expectedActivityType, viewModel.SelectedActivityType);
    Assert.True(viewModel.HasSelectedActivity, "Selected movement activity should be visible.");
    Assert.True(viewModel.HasMovementDraft, "Selected movement activity should create a movement draft.");
    Assert.Null(viewModel.SelectedTimingChoice, "Selecting only an activity should not choose timing.");
    Assert.Null(viewModel.SelectedScheduledFor, "Selecting only an activity should not set a scheduled time.");
    Assert.Null(viewModel.SelectedMindOccupierChoice, "Selecting only an activity should not choose a mind occupier.");
    Assert.Null(viewModel.SelectedSpouseChoice, "Selecting only an activity should not choose a spouse option.");
    Assert.False(viewModel.HasSelectedTiming, "Selecting only an activity should not complete timing.");
    Assert.False(viewModel.HasSelectedMindOccupier, "Selecting only an activity should not complete mind occupier.");
    Assert.False(viewModel.HasSelectedSpouseOption, "Selecting only an activity should not complete spouse option.");
    Assert.True(viewModel.CanChooseTiming, "Activity selection should unlock timing.");
    Assert.False(viewModel.CanChooseMindOccupier, "Activity selection should not unlock mind occupier yet.");
    Assert.False(viewModel.CanChooseSpouseOption, "Activity selection should not unlock spouse option yet.");
    Assert.Equal($"{activityName} selected.", viewModel.SelectedActivityText);
    Assert.Equal($"{activityName} Ready", viewModel.MovementPanelTitle);
    Assert.Equal($"{activityName} is ready for a 20 minute movement plan.", viewModel.MovementPanelText);
    Assert.Equal($"{activityName} draft ready.", viewModel.MovementDraftStatusText);
    Assert.Equal($"{activityName} selected.", viewModel.StatusText);
    Assert.Equal("Choose Now or Schedule next.", viewModel.EmptyStateText);
    Assert.Equal(walkSelected, viewModel.IsWalkSelected);
    Assert.Equal(workoutSelected, viewModel.IsWorkoutSelected);
    Assert.Equal(stretchSelected, viewModel.IsStretchSelected);
    Assert.Equal(walkSelected ? "Selected" : "Choose", viewModel.WalkChoiceStatusText);
    Assert.Equal(workoutSelected ? "Selected" : "Choose", viewModel.WorkoutChoiceStatusText);
    Assert.Equal(stretchSelected ? "Selected" : "Choose", viewModel.StretchChoiceStatusText);
    Assert.Equal("Choose", viewModel.NowChoiceStatusText);
    Assert.Equal("Choose", viewModel.ScheduleChoiceStatusText);
    Assert.Equal("Pick timing", viewModel.MusicChoiceStatusText);
    Assert.Equal("Pick timing", viewModel.PodcastChoiceStatusText);
    Assert.Equal("Pick timing", viewModel.AudiobookChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.SoloChoiceStatusText);
    Assert.Equal("Pick mind", viewModel.WithSpouseChoiceStatusText);
}

static TaskItem CreateTask(
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

static NoteItem CreateReviewHighlight(string text, DateTimeOffset createdAt)
{
    return NoteItem.Rehydrate(
        Guid.NewGuid(),
        NoteOwnerType.Task,
        Guid.NewGuid(),
        text,
        createdAt,
        createdAt,
        isReviewHighlight: true,
        tags: ["Review", "Win", "Small Win"]);
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

internal sealed class TestMicrosoftProjectFilePicker : IMicrosoftProjectFilePicker
{
    private readonly string? _selectedPath;

    public TestMicrosoftProjectFilePicker(string? selectedPath)
    {
        _selectedPath = selectedPath;
    }

    public int CallCount { get; private set; }

    public string LastCurrentFilePath { get; private set; } = string.Empty;

    public Task<string?> PickProjectFileAsync(
        string currentFilePath = "",
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        LastCurrentFilePath = currentFilePath;
        return Task.FromResult(_selectedPath);
    }
}

internal static class TestRunner
{
    public static void RunAll(params (string Name, Action Test)[] tests)
    {
        int passed = 0;

        foreach ((string name, Action test) in tests)
        {
            try
            {
                test();
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

        Console.WriteLine($"{passed} view model tests passed.");
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
        if (!value.Contains(expectedSubstring, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected '{value}' to contain '{expectedSubstring}'.");
        }
    }
}
