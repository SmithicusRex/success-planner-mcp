using System.Text.Json;

namespace SuccessPlanner.App.Infrastructure;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly AppPaths _paths;

    public SettingsService(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task<AppSettings> LoadOrCreateAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_paths.SettingsPath))
        {
            AppSettings defaults = new();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        await using FileStream stream = File.OpenRead(_paths.SettingsPath);
        AppSettings? settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken);
        return settings ?? new AppSettings();
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_paths.AppDataDirectory);

        await using FileStream stream = File.Create(_paths.SettingsPath);
        await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
    }
}
