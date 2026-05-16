using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LibGit2Sharp;

namespace RainbusToolbox.Models.Managers;

public class PersistentDataManager
{
    private readonly string _filePath;

    public PersistentDataManager()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RainbusToolbox");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");

        Settings = new SettingsData();
        Read();
    }

    public SettingsData Settings { get; set; }

    public void Read()
    {
        if (!File.Exists(_filePath)) return;
        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            Settings = data;
        }
        catch
        {
        }
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }

    //these 2 return path if they could find it, otherwise null

    //validates path of localization, cuz it must be a git repo and contain /localize/ folder at root
    public static string? ValidateRepoPath(string? inputPath)
    {
        // basically, to fix error if user selected wrong level directory, it's better to start searching paths 2 above (recursively) and 1 below to make sure i can find the repo like that

        if (string.IsNullOrEmpty(inputPath))
            return null;

        if (Repository.IsValid(inputPath) && Path.Exists(Path.Combine(inputPath, "localize"))) return inputPath;
        // if it didn't return, means user selected wrong path

        var additionalPathsToCheck = new List<string>();

        var oneUpPath = Directory.GetParent(inputPath)?.FullName;
        var twoUpPath = Directory.GetParent(inputPath)?.Parent?.FullName;

        if (oneUpPath != null)
        {
            additionalPathsToCheck.Add(oneUpPath);
            additionalPathsToCheck.AddRange(Directory.GetDirectories(oneUpPath));
        }

        if (twoUpPath != null)
        {
            additionalPathsToCheck.Add(twoUpPath);
            additionalPathsToCheck.AddRange(Directory.GetDirectories(twoUpPath));
        }

        additionalPathsToCheck.AddRange(Directory.GetDirectories(inputPath));

        foreach (var path in additionalPathsToCheck)
            if (Repository.IsValid(path) && Path.Exists(Path.Combine(path, "localize")))
                return path;

        return null;
    }

    public static string? ValidateLimbusPath(string? inputPath)
    {
        if (string.IsNullOrEmpty(inputPath))
            return null;

        if (Path.Exists(Path.Combine(inputPath, "LimbusCompany_Data"))
            && File.Exists(Path.Combine(inputPath, "LimbusCompany.exe"))
           ) return inputPath;
        // if it didn't return, means user selected wrong path

        var additionalPathsToCheck = new List<string>();

        var oneUpPath = Directory.GetParent(inputPath)?.FullName;
        var twoUpPath = Directory.GetParent(inputPath)?.Parent?.FullName;

        if (oneUpPath != null)
        {
            additionalPathsToCheck.Add(oneUpPath);
            additionalPathsToCheck.AddRange(Directory.GetDirectories(oneUpPath));
        }

        if (twoUpPath != null)
        {
            additionalPathsToCheck.Add(twoUpPath);
            additionalPathsToCheck.AddRange(Directory.GetDirectories(twoUpPath));
        }

        additionalPathsToCheck.AddRange(Directory.GetDirectories(inputPath));

        foreach (var path in additionalPathsToCheck)
            if (Path.Exists(Path.Combine(path, "LimbusCompany_Data"))
                && File.Exists(Path.Combine(path, "LimbusCompany.exe"))
               )
                return path;

        return null;
    }
}