namespace SuccessPlanner.App.Services;

public sealed class MicrosoftProjectDesktopDetector
{
    private static readonly string[] OfficeFolderNames =
    [
        "Office16",
        "Office15",
        "Office14",
        "Office12"
    ];

    private readonly Func<IReadOnlyList<string>> _programFilesRootsProvider;
    private readonly Func<IReadOnlyList<string>> _pathDirectoriesProvider;
    private readonly Func<string, bool> _fileExists;

    public MicrosoftProjectDesktopDetector()
        : this(GetDefaultProgramFilesRoots, GetDefaultPathDirectories, File.Exists)
    {
    }

    public MicrosoftProjectDesktopDetector(
        IEnumerable<string> programFilesRoots,
        IEnumerable<string>? pathDirectories = null)
        : this(
            () => NormalizePaths(programFilesRoots),
            () => NormalizePaths(pathDirectories ?? []),
            File.Exists)
    {
    }

    internal MicrosoftProjectDesktopDetector(
        Func<IReadOnlyList<string>> programFilesRootsProvider,
        Func<IReadOnlyList<string>> pathDirectoriesProvider,
        Func<string, bool> fileExists)
    {
        _programFilesRootsProvider = programFilesRootsProvider
            ?? throw new ArgumentNullException(nameof(programFilesRootsProvider));
        _pathDirectoriesProvider = pathDirectoriesProvider
            ?? throw new ArgumentNullException(nameof(pathDirectoriesProvider));
        _fileExists = fileExists ?? throw new ArgumentNullException(nameof(fileExists));
    }

    public Task<MicrosoftProjectDesktopDetectionResult> DetectAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<string> candidates = BuildCandidatePaths(
            _programFilesRootsProvider(),
            _pathDirectoriesProvider());

        foreach (string candidate in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_fileExists(candidate))
            {
                return Task.FromResult(MicrosoftProjectDesktopDetectionResult.Detected(
                    candidate,
                    candidates));
            }
        }

        return Task.FromResult(MicrosoftProjectDesktopDetectionResult.NotFound(candidates));
    }

    public static IReadOnlyList<string> BuildCandidatePaths(
        IEnumerable<string> programFilesRoots,
        IEnumerable<string> pathDirectories)
    {
        List<string> candidates = [];

        foreach (string root in NormalizePaths(programFilesRoots))
        {
            foreach (string officeFolderName in OfficeFolderNames)
            {
                candidates.Add(Path.Combine(
                    root,
                    "Microsoft Office",
                    "root",
                    officeFolderName,
                    MicrosoftProjectDesktopDetectionResult.ExecutableName));
                candidates.Add(Path.Combine(
                    root,
                    "Microsoft Office",
                    officeFolderName,
                    MicrosoftProjectDesktopDetectionResult.ExecutableName));
            }
        }

        foreach (string pathDirectory in NormalizePaths(pathDirectories))
        {
            candidates.Add(Path.Combine(
                pathDirectory,
                MicrosoftProjectDesktopDetectionResult.ExecutableName));
        }

        return NormalizePaths(candidates);
    }

    private static IReadOnlyList<string> GetDefaultProgramFilesRoots()
    {
        string?[] roots =
        [
            Environment.GetEnvironmentVariable("ProgramFiles"),
            Environment.GetEnvironmentVariable("ProgramFiles(x86)"),
            Environment.GetEnvironmentVariable("ProgramW6432")
        ];

        return NormalizePaths(roots);
    }

    private static IReadOnlyList<string> GetDefaultPathDirectories()
    {
        string? path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return [];
        }

        return NormalizePaths(path.Split(Path.PathSeparator));
    }

    private static IReadOnlyList<string> NormalizePaths(IEnumerable<string?> paths)
    {
        List<string> normalized = [];

        foreach (string? path in paths)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                continue;
            }

            string trimmed = path.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(trimmed);
            }
            catch (ArgumentException)
            {
                continue;
            }
            catch (NotSupportedException)
            {
                continue;
            }
            catch (PathTooLongException)
            {
                continue;
            }

            if (!normalized.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                normalized.Add(fullPath);
            }
        }

        return normalized;
    }
}
