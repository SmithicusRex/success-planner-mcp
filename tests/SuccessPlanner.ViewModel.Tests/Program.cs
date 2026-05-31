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
    ("TodayViewModel shows an empty today state", TodayViewModelShowsEmptyTodayState),
    ("TodayViewModel reports load failures", TodayViewModelReportsLoadFailures));

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
