namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectDesktopDetectionResult
{
    private MicrosoftProjectDesktopDetectionResult(
        string executablePath,
        IReadOnlyList<string> searchedPaths)
    {
        ExecutablePath = executablePath.Trim();
        SearchedPaths = searchedPaths;
    }

    public const string ExecutableName = "WINPROJ.EXE";

    public string DisplayName => "Microsoft Project Desktop";

    public bool IsDetected => !string.IsNullOrWhiteSpace(ExecutablePath);

    public string ExecutablePath { get; }

    public IReadOnlyList<string> SearchedPaths { get; }

    public string StatusText => IsDetected
        ? "Project detected"
        : "Project not found";

    public string DetailText => IsDetected
        ? $"Found {ExecutableName} at {ExecutablePath}."
        : $"Could not find {ExecutableName} in the common Microsoft Office install paths.";

    public static MicrosoftProjectDesktopDetectionResult Detected(
        string executablePath,
        IReadOnlyList<string> searchedPaths)
    {
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new ArgumentException("Executable path cannot be blank.", nameof(executablePath));
        }

        return new MicrosoftProjectDesktopDetectionResult(
            executablePath,
            searchedPaths);
    }

    public static MicrosoftProjectDesktopDetectionResult NotFound(
        IReadOnlyList<string> searchedPaths)
    {
        return new MicrosoftProjectDesktopDetectionResult(string.Empty, searchedPaths);
    }
}
