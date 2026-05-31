using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.ViewModels;

TestRunner.RunAll(
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
    ("StartWorkViewModel reports load failures", StartWorkViewModelReportsLoadFailures));

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
