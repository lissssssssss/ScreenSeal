using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ScreenSeal.Models;

namespace ScreenSeal.Services;

public sealed class ConfigService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly string _configPath;

    public AppConfig Current { get; private set; } = new();

    public ConfigService()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ScreenSeal");
        Directory.CreateDirectory(dir);
        _configPath = Path.Combine(dir, "config.json");
        Load();
    }

    public void Load()
    {
        if (!File.Exists(_configPath))
        {
            Current = new AppConfig();
            Save();
            return;
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            Current = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions) ?? new AppConfig();
        }
        catch
        {
            Current = new AppConfig();
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Current, JsonOptions);
        File.WriteAllText(_configPath, json);
    }

    public void Update(Action<AppConfig> mutate)
    {
        mutate(Current);
        Save();
    }
}
