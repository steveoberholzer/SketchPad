using System.IO;
using System.Text.Json;

namespace Sketchpad.App.AI;

internal class SettingsData
{
    public string AnthropicApiKey { get; set; } = string.Empty;
}

public static class AiSettings
{
    private static readonly string _path =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "Sketchpad", "settings.json");

    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    public static string AnthropicApiKey
    {
        get => Load().AnthropicApiKey;
        set
        {
            var data = Load();
            data.AnthropicApiKey = value;
            Save(data);
        }
    }

    private static SettingsData Load()
    {
        try
        {
            if (File.Exists(_path))
                return JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(_path), _json) ?? new();
        }
        catch { }
        return new();
    }

    private static void Save(SettingsData data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(data, _json));
    }
}
