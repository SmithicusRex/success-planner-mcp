using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Services;

TestRunner.RunAll(
    ("TaskItem creation and status transitions", TaskItemCreationAndStatusTransitions),
    ("ProjectItem creation and status transitions", ProjectItemCreationAndStatusTransitions),
    ("MilestoneItem creation and status transitions", MilestoneItemCreationAndStatusTransitions),
    ("NoteItem creation and status transitions", NoteItemCreationAndStatusTransitions),
    ("FocusSession creation and status transitions", FocusSessionCreationAndStatusTransitions),
    ("SuccessGoal creation and status transitions", SuccessGoalCreationAndStatusTransitions),
    ("MovementSession creation and status transitions", MovementSessionCreationAndStatusTransitions),
    ("Microsoft To Do connection status model", MicrosoftToDoConnectionStatusModel),
    ("Microsoft Planner connection status model", MicrosoftPlannerConnectionStatusModel),
    ("Phone companion sync contract model", PhoneCompanionSyncContractModel),
    ("SourceLink creation and sync transitions", SourceLinkCreationAndSyncTransitions),
    ("SyncQueueItem creation and sync transitions", SyncQueueItemCreationAndSyncTransitions));

static void TaskItemCreationAndStatusTransitions()
{
    TaskItem task = TaskItem.Capture("  Write outline  ");

    Assert.Equal("Write outline", task.Title);
    Assert.Equal(TaskItemStatus.Captured, task.Status);
    Assert.Equal(TaskPriority.Normal, task.Priority);

    task.Schedule(DateOnly.FromDateTime(DateTime.Today));
    Assert.Equal(TaskItemStatus.Planned, task.Status);

    task.Start();
    Assert.Equal(TaskItemStatus.InProgress, task.Status);

    task.Complete(new DateTimeOffset(2026, 5, 29, 10, 0, 0, TimeSpan.Zero));
    Assert.Equal(TaskItemStatus.Done, task.Status);
    Assert.True(task.CompletedAt.HasValue, "Task completion time should be recorded.");

    task.Reopen();
    Assert.Equal(TaskItemStatus.Planned, task.Status);
    Assert.False(task.CompletedAt.HasValue, "Reopened task should clear completion time.");
}

static void ProjectItemCreationAndStatusTransitions()
{
    Guid taskId = Guid.NewGuid();
    Guid milestoneId = Guid.NewGuid();
    ProjectItem project = ProjectItem.Create("  Build planner  ");

    Assert.Equal("Build planner", project.Name);
    Assert.Equal(ProjectStatus.Active, project.Status);
    Assert.Equal(TaskPriority.Normal, project.Priority);

    project.AddTask(taskId);
    project.AddTask(taskId);
    project.AddMilestone(milestoneId);
    Assert.Equal(1, project.TaskIds.Count);
    Assert.Equal(1, project.MilestoneIds.Count);

    project.Pause();
    Assert.Equal(ProjectStatus.Paused, project.Status);

    project.Resume();
    Assert.Equal(ProjectStatus.Active, project.Status);

    project.MarkBlocked();
    Assert.Equal(ProjectStatus.Blocked, project.Status);

    project.Complete(new DateTimeOffset(2026, 5, 29, 11, 0, 0, TimeSpan.Zero));
    Assert.Equal(ProjectStatus.Completed, project.Status);

    project.Reopen();
    Assert.Equal(ProjectStatus.Active, project.Status);
}

static void MilestoneItemCreationAndStatusTransitions()
{
    Guid projectId = Guid.NewGuid();
    Guid taskId = Guid.NewGuid();
    MilestoneItem milestone = MilestoneItem.Create(projectId, "  First usable build  ");

    Assert.Equal("First usable build", milestone.Name);
    Assert.Equal(projectId, milestone.ProjectId);
    Assert.Equal(MilestoneStatus.Upcoming, milestone.Status);

    milestone.AddTask(taskId);
    milestone.AddTask(taskId);
    Assert.Equal(1, milestone.TaskIds.Count);

    milestone.MarkAtRisk();
    Assert.Equal(MilestoneStatus.AtRisk, milestone.Status);

    milestone.MarkBlocked();
    Assert.Equal(MilestoneStatus.Blocked, milestone.Status);

    milestone.Resume();
    Assert.Equal(MilestoneStatus.Upcoming, milestone.Status);

    milestone.Complete(new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));
    Assert.Equal(MilestoneStatus.Completed, milestone.Status);

    milestone.Reopen();
    Assert.Equal(MilestoneStatus.Upcoming, milestone.Status);

    milestone.Archive();
    Assert.Equal(MilestoneStatus.Archived, milestone.Status);
}

static void NoteItemCreationAndStatusTransitions()
{
    Guid taskId = Guid.NewGuid();
    NoteItem note = NoteItem.Capture("  Remember the win  ");

    Assert.Equal(NoteOwnerType.Inbox, note.OwnerType);
    Assert.False(note.OwnerId.HasValue, "Inbox notes should not require an owner id.");
    Assert.Equal("Remember the win", note.Text);

    note.AppendText("Add one small next action.");
    Assert.Contains("Add one small next action.", note.Text);

    note.MoveTo(NoteOwnerType.Task, taskId);
    Assert.Equal(NoteOwnerType.Task, note.OwnerType);
    Assert.Equal(taskId, note.OwnerId);

    note.Pin();
    Assert.True(note.IsPinned, "Pinned note should record pin state.");

    note.MarkReviewHighlight();
    Assert.True(note.IsReviewHighlight, "Review highlight should be set.");
    Assert.Contains("Review", note.Tags);
}

static void FocusSessionCreationAndStatusTransitions()
{
    Guid taskId = Guid.NewGuid();
    FocusSession session = FocusSession.StartForTask(taskId, "  Draft the screen  ");

    Assert.Equal(taskId, session.TaskId);
    Assert.Equal("Draft the screen", session.Intention);
    Assert.Equal(FocusSession.DefaultPlannedMinutes, session.PlannedMinutes);
    Assert.Equal(FocusSessionStatus.InProgress, session.Status);

    session.Pause(session.StartedAt.AddMinutes(5));
    Assert.Equal(FocusSessionStatus.Paused, session.Status);

    session.Resume(session.StartedAt.AddMinutes(7));
    Assert.Equal(FocusSessionStatus.InProgress, session.Status);
    Assert.Equal(2, session.TotalPausedMinutes);

    session.Complete("Small win recorded.", session.StartedAt.AddMinutes(20));
    Assert.Equal(FocusSessionStatus.Completed, session.Status);
    Assert.Equal(18, session.ActualFocusMinutes);
    Assert.Equal("Small win recorded.", session.WinNote);
    Assert.Contains("Win", session.Tags);
}

static void SuccessGoalCreationAndStatusTransitions()
{
    Guid taskId = Guid.NewGuid();
    SuccessGoal goal = SuccessGoal.Create("  Finish planning slice  ", "  One tiny working slice  ");

    Assert.Equal("Finish planning slice", goal.Title);
    Assert.Equal("One tiny working slice", goal.MinimumWin);
    Assert.Equal(SuccessGoalStatus.Draft, goal.Status);

    goal.SetStretchGoal("A polished full workflow.");
    Assert.True(goal.HasStretchGoal, "Stretch goal should be detected.");

    goal.AddTask(taskId);
    goal.AddTask(taskId);
    Assert.Equal(1, goal.TaskIds.Count);

    goal.Schedule(DateOnly.FromDateTime(DateTime.Today), DateOnly.FromDateTime(DateTime.Today.AddDays(7)));
    Assert.Equal(SuccessGoalStatus.Active, goal.Status);

    goal.MarkNeedsDecision();
    Assert.Equal(SuccessGoalStatus.NeedsDecision, goal.Status);
    Assert.Contains("Decision", goal.Tags);

    goal.Complete("Completed the minimum win.", new DateTimeOffset(2026, 5, 29, 13, 0, 0, TimeSpan.Zero));
    Assert.Equal(SuccessGoalStatus.Completed, goal.Status);
    Assert.Contains("Win", goal.Tags);

    goal.Reopen();
    Assert.Equal(SuccessGoalStatus.Active, goal.Status);
}

static void MovementSessionCreationAndStatusTransitions()
{
    MovementSession session = MovementSession.Schedule(
        MovementActivityType.Walk,
        new DateTimeOffset(2026, 5, 29, 14, 0, 0, TimeSpan.Zero));

    Assert.Equal(MovementActivityType.Walk, session.ActivityType);
    Assert.Equal(MovementSessionStatus.Planned, session.Status);
    Assert.Equal(MovementSession.DefaultPlannedMinutes, session.PlannedMinutes);
    Assert.Contains("Move", session.Tags);

    session.SetMindOccupier("Podcast");
    session.MarkWithSpouse();
    Assert.Equal("Podcast", session.MindOccupier);
    Assert.True(session.IsWithSpouse, "Spouse option should be recorded.");

    session.Start(session.CreatedAt.AddMinutes(1));
    Assert.Equal(MovementSessionStatus.Active, session.Status);

    session.Complete("Walk completed.", 19, session.CreatedAt.AddMinutes(20));
    Assert.Equal(MovementSessionStatus.Completed, session.Status);
    Assert.Equal(19, session.ActualMinutes);
    Assert.Equal("Walk completed.", session.WinNote);
    Assert.Contains("Win", session.Tags);
}

static void MicrosoftToDoConnectionStatusModel()
{
    MicrosoftToDoConnectionStatus disabled = MicrosoftToDoConnectionStatus.Disabled();

    Assert.Equal(SourceSystem.MicrosoftToDo, disabled.SourceSystem);
    Assert.Equal("Microsoft To Do", disabled.DisplayName);
    Assert.Equal(MicrosoftToDoConnectionState.Disabled, disabled.State);
    Assert.Equal("To Do is off", disabled.StatusText);
    Assert.Equal("Microsoft To Do is turned off in Settings.", disabled.DetailText);
    Assert.False(disabled.IsEnabled, "Disabled To Do connection should not be enabled.");
    Assert.False(disabled.CanTestConnection, "Disabled To Do connection should not allow testing.");
    Assert.False(disabled.CanSync, "Disabled To Do connection should not sync.");

    DateTimeOffset checkedAt = new(2026, 6, 1, 18, 0, 0, TimeSpan.Zero);
    MicrosoftToDoConnectionStatus connected = MicrosoftToDoConnectionStatus.Connected(
        "  smith@example.com  ",
        checkedAt);

    Assert.Equal(MicrosoftToDoConnectionState.Connected, connected.State);
    Assert.Equal("smith@example.com", connected.AccountDisplayName);
    Assert.Equal(checkedAt, connected.LastCheckedAt);
    Assert.Equal("To Do connected", connected.StatusText);
    Assert.Equal("Connected as smith@example.com.", connected.DetailText);
    Assert.True(connected.IsConnected, "Connected To Do status should be connected.");
    Assert.True(connected.CanSync, "Connected To Do status should allow sync.");
    Assert.True(connected.CanTestConnection, "Connected To Do status should allow retesting.");
    Assert.False(connected.NeedsAttention, "Connected To Do status should not need attention.");

    MicrosoftToDoConnectionStatus needsSignIn = MicrosoftToDoConnectionStatus.NeedsSignIn(
        "  Sign in again to refresh To Do access.  ",
        checkedAt);

    Assert.Equal(MicrosoftToDoConnectionState.NeedsSignIn, needsSignIn.State);
    Assert.Equal("Sign in needed", needsSignIn.StatusText);
    Assert.Equal("Sign in again to refresh To Do access.", needsSignIn.DetailText);
    Assert.True(needsSignIn.CanStartSignIn, "Needs sign-in status should offer sign-in.");
    Assert.True(needsSignIn.NeedsAttention, "Needs sign-in status should need attention.");
    Assert.False(needsSignIn.CanSync, "Needs sign-in status should not sync.");

    MicrosoftToDoConnectionStatus failed = MicrosoftToDoConnectionStatus.Failed(
        "  Network unavailable.  ",
        checkedAt);

    Assert.Equal(MicrosoftToDoConnectionState.Failed, failed.State);
    Assert.Equal("Connection failed", failed.StatusText);
    Assert.Equal("Network unavailable.", failed.DetailText);
    Assert.True(failed.NeedsAttention, "Failed To Do connection should need attention.");
    Assert.True(failed.CanStartSignIn, "Failed To Do connection should allow a fresh sign-in path.");

    MicrosoftToDoConnectionStatus testing = MicrosoftToDoConnectionStatus.Testing(checkedAt);

    Assert.Equal(MicrosoftToDoConnectionState.Testing, testing.State);
    Assert.Equal("Checking To Do", testing.StatusText);
    Assert.False(testing.CanTestConnection, "Testing state should not start another test.");
    Assert.False(testing.CanSync, "Testing state should not sync yet.");
}

static void MicrosoftPlannerConnectionStatusModel()
{
    MicrosoftPlannerConnectionStatus disabled = MicrosoftPlannerConnectionStatus.Disabled();

    Assert.Equal(SourceSystem.MicrosoftPlanner, disabled.SourceSystem);
    Assert.Equal("Microsoft Planner", disabled.DisplayName);
    Assert.Equal(MicrosoftPlannerConnectionState.Disabled, disabled.State);
    Assert.Equal("Planner is off", disabled.StatusText);
    Assert.Equal("Microsoft Planner is turned off in Settings.", disabled.DetailText);
    Assert.False(disabled.IsEnabled, "Disabled Planner connection should not be enabled.");
    Assert.False(disabled.CanTestAvailability, "Disabled Planner connection should not allow testing.");
    Assert.False(disabled.CanReadPlannerTasks, "Disabled Planner connection should not read tasks.");

    DateTimeOffset checkedAt = new(2026, 6, 6, 10, 0, 0, TimeSpan.Zero);
    MicrosoftPlannerConnectionStatus available = MicrosoftPlannerConnectionStatus.Available(
        "  smith@example.com  ",
        checkedAt);

    Assert.Equal(MicrosoftPlannerConnectionState.Available, available.State);
    Assert.Equal("smith@example.com", available.AccountDisplayName);
    Assert.Equal(checkedAt, available.LastCheckedAt);
    Assert.Equal("Planner available", available.StatusText);
    Assert.Equal("Planner is available for smith@example.com.", available.DetailText);
    Assert.True(available.IsAvailable, "Available Planner status should report available.");
    Assert.True(available.CanReadPlannerTasks, "Available Planner status should allow reading tasks.");
    Assert.True(available.CanTestAvailability, "Available Planner status should allow retesting.");
    Assert.False(available.NeedsAttention, "Available Planner status should not need attention.");

    MicrosoftPlannerConnectionStatus unavailable = MicrosoftPlannerConnectionStatus.Unavailable(
        "  Planner is not included with this account.  ",
        checkedAt);

    Assert.Equal(MicrosoftPlannerConnectionState.Unavailable, unavailable.State);
    Assert.Equal("Planner unavailable", unavailable.StatusText);
    Assert.Equal("Planner is not included with this account.", unavailable.DetailText);
    Assert.True(unavailable.NeedsAttention, "Unavailable Planner status should need attention.");
    Assert.False(unavailable.CanReadPlannerTasks, "Unavailable Planner status should not read tasks.");
    Assert.False(unavailable.CanStartSignIn, "Unavailable Planner access should not imply sign-in can fix licensing.");

    MicrosoftPlannerConnectionStatus needsSignIn = MicrosoftPlannerConnectionStatus.NeedsSignIn(
        "  Sign in again to check Planner.  ",
        checkedAt);

    Assert.Equal(MicrosoftPlannerConnectionState.NeedsSignIn, needsSignIn.State);
    Assert.Equal("Sign in needed", needsSignIn.StatusText);
    Assert.Equal("Sign in again to check Planner.", needsSignIn.DetailText);
    Assert.True(needsSignIn.CanStartSignIn, "Needs sign-in status should offer sign-in.");
    Assert.True(needsSignIn.NeedsAttention, "Needs sign-in status should need attention.");

    MicrosoftPlannerConnectionStatus failed = MicrosoftPlannerConnectionStatus.Failed(
        "  Graph request timed out.  ",
        checkedAt);

    Assert.Equal(MicrosoftPlannerConnectionState.Failed, failed.State);
    Assert.Equal("Planner check failed", failed.StatusText);
    Assert.Equal("Graph request timed out.", failed.DetailText);
    Assert.True(failed.NeedsAttention, "Failed Planner check should need attention.");
    Assert.True(failed.CanStartSignIn, "Failed Planner check should allow a fresh sign-in path.");

    MicrosoftPlannerConnectionStatus testing = MicrosoftPlannerConnectionStatus.Testing(checkedAt);

    Assert.Equal(MicrosoftPlannerConnectionState.Testing, testing.State);
    Assert.Equal("Checking Planner", testing.StatusText);
    Assert.False(testing.CanTestAvailability, "Testing state should not start another check.");
    Assert.False(testing.CanReadPlannerTasks, "Testing state should not read Planner tasks yet.");
}

static void PhoneCompanionSyncContractModel()
{
    DateTimeOffset capturedAt = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
    PhoneCompanionQuickCaptureItem capture = new(
        "  phone-001  ",
        "  Call pharmacy  ",
        capturedAt,
        "  Ask about refill timing.  ",
        new DateOnly(2026, 6, 8),
        PhoneCompanionCaptureDestination.LocalInbox,
        ["Health", "health", "Errand"]);

    Assert.Equal(PhoneCompanionSyncContract.CurrentVersion, capture.ContractVersion);
    Assert.Equal("phone-001", capture.ClientCaptureId);
    Assert.Equal("Call pharmacy", capture.Title);
    Assert.Equal("Ask about refill timing.", capture.Notes);
    Assert.Equal(capturedAt, capture.CapturedAt);
    Assert.Equal(new DateOnly(2026, 6, 8), capture.DueDate.GetValueOrDefault());
    Assert.Equal(PhoneCompanionCaptureDestination.LocalInbox, capture.Destination);
    Assert.True(capture.HasNotes, "Capture should report notes when notes are supplied.");
    Assert.True(capture.HasDueDate, "Capture should report a due date when one is supplied.");
    Assert.True(capture.HasTags, "Capture should report tags when tags are supplied.");
    Assert.Equal(2, capture.Tags.Count);
    Assert.Contains("Health", capture.Tags);
    Assert.Contains("Errand", capture.Tags);

    PhoneCompanionSyncBatch batch = new(
        " batch-001 ",
        " device-001 ",
        " Smith phone ",
        capturedAt,
        [capture]);

    Assert.Equal("batch-001", batch.BatchId);
    Assert.Equal("device-001", batch.DeviceId);
    Assert.Equal("Smith phone", batch.DeviceName);
    Assert.Equal(1, batch.CaptureCount);
    Assert.True(batch.HasCaptures, "Batch should report captures when captures are supplied.");

    Guid localTaskId = Guid.NewGuid();
    PhoneCompanionCaptureImportOutcome imported =
        PhoneCompanionCaptureImportOutcome.Imported(capture.ClientCaptureId, localTaskId);
    PhoneCompanionCaptureImportOutcome skipped =
        PhoneCompanionCaptureImportOutcome.Skipped("phone-002", "Already imported.");
    PhoneCompanionSyncResult accepted = PhoneCompanionSyncResult.Accepted([imported, skipped]);

    Assert.Equal(PhoneCompanionCaptureImportState.Imported, imported.State);
    Assert.Equal(localTaskId, imported.LocalTaskId.GetValueOrDefault());
    Assert.True(imported.WasImported, "Imported outcome should report success.");
    Assert.Equal(PhoneCompanionSyncResultState.Accepted, accepted.State);
    Assert.Equal("Phone captures imported", accepted.StatusText);
    Assert.Contains("Imported 1 capture", accepted.DetailText);
    Assert.Contains("1 capture already existed", accepted.DetailText);
    Assert.Equal(1, accepted.ImportedCount);
    Assert.Equal(1, accepted.SkippedCount);
    Assert.False(accepted.NeedsAttention, "Accepted result should not need attention.");

    PhoneCompanionSyncResult rejected =
        PhoneCompanionSyncResult.Rejected("Phone companion path is not configured.");
    Assert.Equal("Phone sync unavailable", rejected.StatusText);
    Assert.True(rejected.NeedsAttention, "Rejected result should need attention.");
    Assert.False(rejected.WasSuccessful, "Rejected result should not report success.");

    Assert.Throws<ArgumentException>(() => new PhoneCompanionQuickCaptureItem(
        "phone-003",
        "   ",
        capturedAt));
    Assert.Throws<ArgumentOutOfRangeException>(() => new PhoneCompanionQuickCaptureItem(
        "phone-004",
        "Title",
        capturedAt,
        contractVersion: PhoneCompanionSyncContract.CurrentVersion + 1));
    Assert.Throws<ArgumentException>(() => new PhoneCompanionSyncBatch(
        "batch-002",
        "device-001",
        "Smith phone",
        capturedAt,
        [capture, new PhoneCompanionQuickCaptureItem("PHONE-001", "Duplicate", capturedAt)]));
}

static void SourceLinkCreationAndSyncTransitions()
{
    Guid taskId = Guid.NewGuid();
    SourceLink link = SourceLink.Create(
        SourceLinkItemType.Task,
        taskId,
        SourceSystem.MicrosoftToDo,
        "todo-task-123");

    Assert.Equal(SourceLinkItemType.Task, link.LocalItemType);
    Assert.Equal(taskId, link.LocalItemId);
    Assert.Equal(SourceSystem.MicrosoftToDo, link.SourceSystem);
    Assert.Equal(SyncState.Pending, link.SyncState);

    link.MarkSyncing(new DateTimeOffset(2026, 5, 29, 15, 0, 0, TimeSpan.Zero));
    Assert.Equal(SyncState.Syncing, link.SyncState);
    Assert.True(link.LastAttemptedAt.HasValue, "Sync attempt should be recorded.");

    link.MarkFailed("Network unavailable.", new DateTimeOffset(2026, 5, 29, 15, 5, 0, TimeSpan.Zero));
    Assert.Equal(SyncState.Failed, link.SyncState);
    Assert.Equal(1, link.RetryCount);
    Assert.Equal("Network unavailable.", link.FailureMessage);

    link.MarkSynced("etag-1", new DateTimeOffset(2026, 5, 29, 15, 10, 0, TimeSpan.Zero));
    Assert.Equal(SyncState.Synced, link.SyncState);
    Assert.Equal(0, link.RetryCount);
    Assert.Equal("etag-1", link.SourceVersion);
    Assert.Equal(string.Empty, link.FailureMessage);

    link.DisableSync("User turned off this source.");
    Assert.Equal(SyncState.Disabled, link.SyncState);

    link.EnableSync();
    Assert.Equal(SyncState.Pending, link.SyncState);
}

static void SyncQueueItemCreationAndSyncTransitions()
{
    Guid taskId = Guid.NewGuid();
    DateTimeOffset createdAt = new(2026, 6, 1, 9, 0, 0, TimeSpan.Zero);
    SyncQueueItem item = SyncQueueItem.Create(
        SourceLinkItemType.Task,
        taskId,
        SourceSystem.MicrosoftToDo,
        SyncQueueActionType.Update,
        """{"title":"Call the pharmacy"}""",
        createdAt: createdAt);

    Assert.Equal(SourceLinkItemType.Task, item.LocalItemType);
    Assert.Equal(taskId, item.LocalItemId);
    Assert.Equal(SourceSystem.MicrosoftToDo, item.SourceSystem);
    Assert.Equal(SyncQueueActionType.Update, item.ActionType);
    Assert.Equal("""{"title":"Call the pharmacy"}""", item.PayloadJson);
    Assert.Equal(SyncState.Pending, item.SyncState);
    Assert.True(item.IsReady(createdAt), "New sync queue item should be ready immediately.");

    DateTimeOffset syncingAt = createdAt.AddMinutes(1);
    item.MarkSyncing(syncingAt);
    Assert.Equal(SyncState.Syncing, item.SyncState);
    Assert.Equal(syncingAt, item.LastAttemptedAt);
    Assert.False(item.IsReady(syncingAt), "Syncing item should not be picked again.");

    DateTimeOffset retryAt = createdAt.AddMinutes(20);
    item.MarkFailed("Network unavailable.", retryAt, createdAt.AddMinutes(2));
    Assert.Equal(SyncState.Failed, item.SyncState);
    Assert.Equal(1, item.RetryCount);
    Assert.Equal(retryAt, item.NextAttemptAt);
    Assert.Equal("Network unavailable.", item.FailureMessage);
    Assert.False(item.IsReady(createdAt.AddMinutes(10)), "Future retry should wait.");
    Assert.True(item.IsReady(retryAt), "Retry should become ready when next attempt time arrives.");

    item.MarkSynced(createdAt.AddMinutes(25));
    Assert.Equal(SyncState.Synced, item.SyncState);
    Assert.Equal(0, item.RetryCount);
    Assert.Equal(string.Empty, item.FailureMessage);
    Assert.False(item.IsReady(createdAt.AddMinutes(30)), "Synced item should not be picked.");
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

        Console.WriteLine($"{passed} domain unit tests passed.");
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

    public static void Contains<T>(T expected, IEnumerable<T> values)
    {
        if (!values.Contains(expected))
        {
            throw new InvalidOperationException($"Expected collection to contain '{expected}'.");
        }
    }

    public static void Contains(string expected, string actual)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Expected '{actual}' to contain '{expected}'.");
        }
    }

    public static void Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }

        throw new InvalidOperationException($"Expected exception of type '{typeof(TException).Name}'.");
    }
}
