using System;
using System.IO;
using System.Text.Json;

namespace RainbusTools.Models.Managers;

public class PersistentDataManager
{
    public SettingsData Settings { get; set; }
    private readonly string _filePath;
    
    public PersistentDataManager()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RainbusTools");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");

        Settings = new SettingsData();
        Read();
    }

    public void Read()
    {
        if (!File.Exists(_filePath)) return;
        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            Settings = data;
            
        }
        catch {}
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}