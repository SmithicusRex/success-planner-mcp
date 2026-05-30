using SuccessPlanner.App.Domain;

TestRunner.RunAll(
    ("TaskItem creation and status transitions", TaskItemCreationAndStatusTransitions),
    ("ProjectItem creation and status transitions", ProjectItemCreationAndStatusTransitions),
    ("MilestoneItem creation and status transitions", MilestoneItemCreationAndStatusTransitions),
    ("NoteItem creation and status transitions", NoteItemCreationAndStatusTransitions),
    ("FocusSession creation and status transitions", FocusSessionCreationAndStatusTransitions),
    ("SuccessGoal creation and status transitions", SuccessGoalCreationAndStatusTransitions),
    ("MovementSession creation and status transitions", MovementSessionCreationAndStatusTransitions),
    ("SourceLink creation and sync transitions", SourceLinkCreationAndSyncTransitions));

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
}
