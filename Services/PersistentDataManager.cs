using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using LibGit2Sharp;
using RainbusToolbox.Models;

namespace RainbusToolbox.Services;

public sealed class PersistentDataManager
{
    private readonly string _filePath;

    private SettingsData _settings;

    public PersistentDataManager()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RainbusToolbox");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");

        _settings = new SettingsData();
        ReadFromFile();
    }

    public SettingsData Settings
    {
        get => _settings;
        set
        {
            Log.Debug("Settings object was changed");

            var oldValue = _settings;
            _settings = value;

            if (oldValue.PathToLimbus != _settings.PathToLimbus)
                OnLimbusPathChanged?.Invoke();

            if (oldValue.RepositoryPath != _settings.RepositoryPath)
                OnRepositoryPathChanged?.Invoke();

            if (oldValue.SelectedDiscordRPCAssetId != _settings.SelectedDiscordRPCAssetId)
                OnRPCAssetIdChanged?.Invoke();
        }
    }

    public event Action? OnLimbusPathChanged;
    public event Action? OnRepositoryPathChanged;

    public event Action? OnRPCAssetIdChanged;

    public void ReadFromFile()
    {
        if (!File.Exists(_filePath)) return;
        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<SettingsData>(json);
            if (data != null)
                Settings = data;
        }
        catch (Exception exception)
        {
            _ = App.Current.HandleGlobalExceptionAsync(exception);
        }
    }

    public void SaveToFile()
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

        if (SafeIsValidRepo(inputPath) && Path.Exists(Path.Combine(inputPath, "localize"))) return inputPath;
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
            if (SafeIsValidRepo(path) && Path.Exists(Path.Combine(path, "localize")))
                return path;

        return null;
    }

    // so the isvalid for some reason throws on windows, thus i need to silence it cuz it's lowkey retarded tbh
    private static bool SafeIsValidRepo(string path)
    {
        try
        {
            return Repository.IsValid(path);
        }
        catch (LibGit2SharpException)
        {
            return false;
        }
    }

    // TODO: both validations are kinda the same, i can merge them later

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