using Microsoft.Data.Sqlite;
using SuccessPlanner.App.Infrastructure;

TestRunner.RunAll(
    ("DatabaseService creates a real SQLite database", DatabaseServiceCreatesSqliteDatabase),
    ("DatabaseService replaces the legacy bootstrap marker", DatabaseServiceReplacesLegacyMarker),
    ("DatabaseService records repeatable migrations", DatabaseServiceRecordsRepeatableMigrations));

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
    await database.MigrateAsync(CancellationToken.None);
    await database.MigrateAsync(CancellationToken.None);
    await database.CloseAsync(CancellationToken.None);

    Assert.Equal(1L, await ReadScalarAsync(paths.DatabasePath, "SELECT COUNT(*) FROM schema_migrations;"));
    Assert.Equal(1L, await ReadScalarAsync(paths.DatabasePath, "SELECT version FROM schema_migrations;"));
    Assert.Equal(
        "Create local store metadata",
        await ReadScalarAsync(paths.DatabasePath, "SELECT name FROM schema_migrations WHERE version = 1;"));
    Assert.Equal(
        "Success Planner MCP SQLite local store",
        await ReadScalarAsync(paths.DatabasePath, "SELECT value FROM local_store_metadata WHERE key = 'store_kind';"));
}

static async Task<object?> ReadScalarAsync(string databasePath, string commandText)
{
    await using SqliteConnection connection = new($"Data Source={databasePath};Pooling=False");
    await connection.OpenAsync();

    await using SqliteCommand command = connection.CreateCommand();
    command.CommandText = commandText;
    return await command.ExecuteScalarAsync();
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
}
