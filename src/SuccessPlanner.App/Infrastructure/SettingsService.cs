using System.Text.Json;
using System.Text.Json.Serialization;

namespace SuccessPlanner.App.Infrastructure;

public sealed class SettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly AppPaths _paths;

    public SettingsService(AppPaths paths)
    {
        _paths = paths;
    }

    public async Task<AppSettings> LoadOrCreateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_paths.AppDataDirectory);

        if (!File.Exists(_paths.SettingsPath))
        {
            AppSettings defaults = AppSettings.CreateDefault();
            await SaveAsync(defaults, cancellationToken);
            return defaults;
        }

        try
        {
            AppSettings settings = await LoadAsync(_paths.SettingsPath, cancellationToken);
            Normalize(settings);
            Validate(settings);
            return settings;
        }
        catch (JsonException)
        {
            return await RecoverFromInvalidSettingsAsync(cancellationToken);
        }
        catch (SettingsValidationException)
        {
            return await RecoverFromInvalidSettingsAsync(cancellationToken);
        }
        catch (IOException)
        {
            if (File.Exists(BackupPath))
            {
                AppSettings settings = await LoadAsync(BackupPath, cancellationToken);
                Normalize(settings);
                Validate(settings);
                await SaveAsync(settings, cancellationToken);
                return settings;
            }

            throw;
        }
    }

    public async Task SaveAsync(AppSettings settings, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_paths.AppDataDirectory);
        Normalize(settings);
        Validate(settings);

        string tempPath = $"{_paths.SettingsPath}.tmp";

        await using (FileStream stream = File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, settings, JsonOptions, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        if (File.Exists(_paths.SettingsPath))
        {
            File.Replace(tempPath, _paths.SettingsPath, BackupPath, ignoreMetadataErrors: true);
        }
        else
        {
            File.Move(tempPath, _paths.SettingsPath);
        }
    }

    private string BackupPath => $"{_paths.SettingsPath}.bak";

    private static async Task<AppSettings> LoadAsync(string path, CancellationToken cancellationToken)
    {
        await using FileStream stream = File.OpenRead(path);
        AppSettings? settings = await JsonSerializer.DeserializeAsync<AppSettings>(stream, JsonOptions, cancellationToken);
        return settings ?? throw new SettingsValidationException("Settings file is empty.");
    }

    private async Task<AppSettings> RecoverFromInvalidSettingsAsync(CancellationToken cancellationToken)
    {
        string invalidPath = $"{_paths.SettingsPath}.invalid-{DateTimeOffset.Now:yyyyMMddHHmmss}";

        if (File.Exists(_paths.SettingsPath))
        {
            File.Move(_paths.SettingsPath, invalidPath);
        }

        AppSettings defaults = AppSettings.CreateDefault();
        await SaveAsync(defaults, cancellationToken);
        return defaults;
    }

    private static void Normalize(AppSettings settings)
    {
        settings.SchemaVersion = AppSettings.CurrentSchemaVersion;
        settings.ProfileName = NormalizeText(settings.ProfileName, "Personal Success Planner");
        settings.DefaultFocusMinutes = Math.Clamp(settings.DefaultFocusMinutes, 5, 60);
        settings.Display ??= new DisplaySettings();
        settings.Connections ??= new ConnectionSettings();
        settings.DestinationRules ??= [];

        settings.Display.ThemeName = NormalizeText(settings.Display.ThemeName, "Light");
        settings.Display.AccentColor = NormalizeText(settings.Display.AccentColor, "#2F6FED");

        foreach (DestinationRuleSettings rule in settings.DestinationRules)
        {
            rule.Name = NormalizeText(rule.Name, "Untitled rule");
            rule.Condition = NormalizeText(rule.Condition, "Always");
            rule.DestinationSystem = NormalizeText(rule.DestinationSystem, "Local");
            rule.DestinationName = NormalizeText(rule.DestinationName, "Inbox");
        }
    }

    private static string NormalizeText(string? value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }

    private static void Validate(AppSettings settings)
    {
        if (settings.ProfileName.Length > 80)
        {
            throw new SettingsValidationException("Profile name must be 80 characters or fewer.");
        }

        if (!settings.Display.AccentColor.StartsWith('#') || settings.Display.AccentColor.Length != 7)
        {
            throw new SettingsValidationException("Accent color must be a hex color like #2F6FED.");
        }

        foreach (DestinationRuleSettings rule in settings.DestinationRules)
        {
            if (rule.Name.Length > 80)
            {
                throw new SettingsValidationException("Destination rule names must be 80 characters or fewer.");
            }
        }
    }
}

public sealed class SettingsValidationException : Exception
{
    public SettingsValidationException(string message)
        : base(message)
    {
    }
}
