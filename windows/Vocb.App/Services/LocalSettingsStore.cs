using System.Text.Json;

namespace Vocb.App.Services;

/// <summary>Non-secret local preferences persisted between launches.</summary>
public sealed class LocalSettings
{
    public bool ReminderEnabled { get; set; }
    public int ReminderHour { get; set; } = 19;   // 7:00 PM default, matching the Apple client
    public int ReminderMinute { get; set; }
    public bool HasSeenFirstRunHint { get; set; }
}

/// <summary>
/// Tiny JSON-backed store for <see cref="LocalSettings"/> under %LocalAppData%\Vocb.
/// Uses a plain file (not the packaged-app ApplicationData API) so it works for the
/// unpackaged EXE build. Reads/writes are best-effort and fall back to defaults.
/// </summary>
public sealed class LocalSettingsStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly string _path;

    public LocalSettingsStore(string? path = null)
    {
        if (path is not null)
        {
            _path = path;
        }
        else
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vocb");
            Directory.CreateDirectory(dir);
            _path = Path.Combine(dir, "settings.json");
        }
    }

    public LocalSettings Load()
    {
        try
        {
            if (File.Exists(_path))
                return JsonSerializer.Deserialize<LocalSettings>(File.ReadAllText(_path)) ?? new LocalSettings();
        }
        catch { /* fall through to defaults */ }
        return new LocalSettings();
    }

    public void Save(LocalSettings settings)
    {
        try { File.WriteAllText(_path, JsonSerializer.Serialize(settings, Options)); }
        catch { /* best effort */ }
    }
}
