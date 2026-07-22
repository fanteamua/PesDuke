using System.IO;
using Newtonsoft.Json;
using PesDuke.Models;

namespace PesDuke.Services;

public class SettingsService
{
    private readonly string _settingsFilePath;
    private UserSettings _settings;

    public SettingsService(string? settingsFilePath = null)
    {
        _settingsFilePath = settingsFilePath ?? Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "pesduke_settings.json");
        _settings = LoadSettings();
    }

    public UserSettings Settings => _settings;

    public void SaveSettings(UserSettings settings)
    {
        _settings = settings;
        try
        {
            File.WriteAllText(_settingsFilePath, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
        catch { /* ponytail: swallow save errors */ }
    }

    private UserSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
                return JsonConvert.DeserializeObject<UserSettings>(File.ReadAllText(_settingsFilePath)) ?? new UserSettings();
        }
        catch { /* use defaults */ }
        return new UserSettings();
    }
}
