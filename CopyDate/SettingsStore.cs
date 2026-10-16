using System.Text.Json;

namespace CopyDate;

internal static class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public static string FilePath
    {
        get
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CopyDate");
            Directory.CreateDirectory(directory);
            return Path.Combine(directory, "settings.json");
        }
    }

    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(FilePath);
            AppSettings? loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
            return Normalize(loaded ?? new AppSettings());
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        {
            return new AppSettings();
        }
    }

    public static void Save(AppSettings settings)
    {
        AppSettings normalized = Normalize(settings);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(normalized, JsonOptions));
    }

    public static AppSettings Clone(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        return Normalize(JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings());
    }

    private static AppSettings Normalize(AppSettings settings)
    {
        settings.EnabledBuiltInIds ??= [.. DateFormatCatalog.DefaultEnabledIds];
        settings.CustomFormats ??= [];
        settings.Language = string.IsNullOrWhiteSpace(settings.Language)
            ? Localization.FollowSystem
            : settings.Language;
        settings.TrayIconStyle = TrayIconThemes.Resolve(settings.TrayIconStyle).Id;

        foreach (CustomFormat format in settings.CustomFormats)
        {
            if (string.IsNullOrWhiteSpace(format.Id))
            {
                format.Id = Guid.NewGuid().ToString("N");
            }
        }

        if (string.IsNullOrWhiteSpace(settings.DefaultFormatId)
            || DateFormatCatalog.Resolve(settings, settings.DefaultFormatId) is null)
        {
            settings.DefaultFormatId = DateFormatCatalog.IsoDate;
        }

        return settings;
    }
}
