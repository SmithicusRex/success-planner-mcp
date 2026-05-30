using SuccessPlanner.App.Domain;
using SuccessPlanner.App.Screens;
using SuccessPlanner.App.ViewModels;

TestRunner.RunAll(
    ("CaptureViewModel starts in a simple ready state", CaptureViewModelStartsReady),
    ("CaptureViewModel validates an empty title", CaptureViewModelValidatesEmptyTitle),
    ("CaptureViewModel creates a captured task draft", CaptureViewModelCreatesCapturedTaskDraft),
    ("CaptureViewModel resets the capture form", CaptureViewModelResetsCaptureForm),
    ("CaptureViewModel raises property change notifications", CaptureViewModelRaisesPropertyChangeNotifications));

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
    Assert.False(viewModel.CanCreateTask, "Blank capture title should not be ready.");
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

static void CaptureViewModelResetsCaptureForm()
{
    CaptureViewModel viewModel = new()
    {
        TaskTitle = "Plan the tiny step",
        Notes = "Notes"
    };
    viewModel.TryCreateCapturedTask(out _);

    viewModel.ResetCaptureForm();

    Assert.Equal(string.Empty, viewModel.TaskTitle);
    Assert.Equal(string.Empty, viewModel.Notes);
    Assert.Equal(string.Empty, viewModel.ValidationMessage);
    Assert.Equal("Ready to capture.", viewModel.StatusText);
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
}
