using System.IO;
using System.Text.Json;
using LibGit2Sharp;

namespace RainbusToolbox.Services;

public sealed class PersistentDataManager
{
    private readonly string _filePath;

    private readonly List<Action<SettingsData, SettingsData>> _watchers = new();

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
            var oldValue = _settings;
            if (oldValue == value)
                return;
            Log.Debug("Settings object was changed");
            _settings = value;


            foreach (var watcher in _watchers.ToArray())
                try
                {
                    watcher.Invoke(oldValue, value);
                }
                catch (Exception e)
                {
                    Log.Error("{E}", e.ToString());
                }
        }
    }

    public void Watch<T>(Func<SettingsData, T> selector, Action<T> onChanged)
    {
        _watchers.Add((oldSetting, newSetting) =>
        {
            var before = selector(oldSetting);
            var after = selector(newSetting);
            if (!EqualityComparer<T>.Default.Equals(before, after))
                onChanged(after);
        });
    }

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
    //TODO: move this shit to a separate static helper
    //validates path of the localization, cuz it must be a git repo and contain /localize/ folder at the root
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