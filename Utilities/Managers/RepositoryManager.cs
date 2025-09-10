using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json;
using RainbusTools.Utilities.Data;

namespace RainbusTools.Converters.Managers;

public class RepositoryManager
{
    public Repository Repository { get; private set; }
    private string _distPath = ".dist/";
    
    private string _packageConfigName = "config.json";
    private string _localizationFolder = "localize";
    private string _referenceLangAppendage = "/LimbusCompany_Data/Assets/Resources_moved/Localize/en/";
    private string _pathToReference = "";
    
    private PersistentDataManager _dataManager;
    
    public RepositoryManager(PersistentDataManager dataManager)
    {
        _dataManager = dataManager;
        TryInitialize();
    }
    
    
    public void TryInitialize()
    {
        var path = _dataManager.Settings.RepositoryPath;

        if (!Repository.IsValid(path))
        {
            return;
        }

        Repository = new Repository(path);

        // Ensure .dist folder exists
        Directory.CreateDirectory(Path.Combine(path, _distPath));
        
        _pathToReference = Path.Combine(_dataManager.Settings.PathToLimbus, _referenceLangAppendage);
    }


    public int[] CheckRepositoryChanges()
{
    var branch = Repository.Head;
    Console.WriteLine($"Current branch: {branch.FriendlyName}");

    if (string.IsNullOrEmpty(branch.RemoteName)) 
    {
        Console.WriteLine("No remote set for the current branch.");
        return new int[] { 0, 0 };
    }

    var remote = Repository.Network.Remotes[branch.RemoteName];
    Console.WriteLine($"Remote: {remote.Name}");

    var fetchOptions = new FetchOptions
    {
        CredentialsProvider = (_url, _user, _cred) => 
            new UsernamePasswordCredentials 
            { 
                Username = "token",
                Password = _dataManager.Settings.GitHubToken
            }
    };

    try
    {
        Console.WriteLine("Fetching...");
        Repository.Network.Fetch(remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions);
        Console.WriteLine("Fetch completed.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fetch failed: {ex.Message}");
        return new int[] { 0, 0 };
    }

    // After fetch, we can try to get the tracked branch again (the remote tracking branch might have been updated)
    branch = Repository.Head; // This is the same as before, so not necessary? Actually, the local branch didn't change, but the remote tracking branch did.

    var tracked = branch.TrackedBranch;
    if (tracked == null)
    {
        Console.WriteLine("Tracked branch is null. Trying to get remote branch manually.");
        tracked = Repository.Branches[$"{remote.Name}/{branch.FriendlyName}"];
    }

    if (tracked == null)
    {
        Console.WriteLine($"Could not find remote branch: {remote.Name}/{branch.FriendlyName}");
        return new int[] { 0, 0 };
    }

    Console.WriteLine($"Local branch tip: {branch.Tip.Sha}");
    Console.WriteLine($"Tracked branch tip: {tracked.Tip.Sha}");

    var divergence = Repository.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, tracked.Tip);
    Console.WriteLine($"Divergence: AheadBy {divergence?.AheadBy}, BehindBy {divergence?.BehindBy}");

    return new int[] { divergence?.BehindBy ?? 0, divergence?.AheadBy ?? 0 };
}





    
    public string PackageLocalization(string version)
    {
        // Make sure local repo is up-to-date
        FetchMergeAndPushToOrigin();

        var repoPath = Repository.Info.WorkingDirectory;
        var zipFileName = $"RCR v{version}.zip";
        var zipPath = Path.Combine(repoPath, _distPath, zipFileName);

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            // Add localization folder preserving folder structure
            var localizePath = Path.Combine(repoPath, _localizationFolder);
            if (Directory.Exists(localizePath))
            {
                foreach (var file in Directory.GetFiles(localizePath, "*", SearchOption.AllDirectories))
                {
                    // Get path relative to localize/ so folder structure is preserved
                    var relativePath = Path.GetRelativePath(localizePath, file);
                    zip.CreateEntryFromFile(file, relativePath);
                }
            }
        }

        return zipPath;
    }

    public void DeleteHintAtId(int id)
    {
        
    }

    public void FetchMergeAndPushToOrigin()
    {
        var remote = Repository.Network.Remotes["origin"];
    
        var fetchOptions = new FetchOptions
        {
            CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials
                {
                    Username = _dataManager.Settings.GitHubToken, // token as username
                    Password = string.Empty
                }
        };

        // 1. Fetch latest changes from origin using token
        Commands.Fetch(Repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions, "Fetching from origin");

        // 2. Merge fetched changes into current branch
        var currentBranch = Repository.Head;
        var remoteBranch = Repository.Branches[$"origin/{currentBranch.FriendlyName}"];

        if (remoteBranch == null)
            throw new Exception($"Remote branch origin/{currentBranch.FriendlyName} not found.");

        var merger = Repository.Merge(remoteBranch, GetLocalSignature(Repository));

        if (merger.Status == MergeStatus.Conflicts)
            throw new Exception("Merge conflicts occurred. Please resolve manually.");

        // 3. Stage any remaining changes
        Commands.Stage(Repository, "*");

        var status = Repository.RetrieveStatus();

        // Only commit if there are staged changes
        if (status.Any(entry =>
                entry.State.HasFlag(FileStatus.NewInIndex) ||
                entry.State.HasFlag(FileStatus.ModifiedInIndex) ||
                entry.State.HasFlag(FileStatus.DeletedFromIndex) ||
                entry.State.HasFlag(FileStatus.RenamedInIndex) ||
                entry.State.HasFlag(FileStatus.TypeChangeInIndex)))
        {
            var author = GetLocalSignature(Repository);
            Repository.Commit("Auto-commit local changes after merge", author, author);
        }

        // 5. Push local branch to origin using token
        var pushOptions = new PushOptions
        {
            CredentialsProvider = (_url, _user, _cred) =>
                new UsernamePasswordCredentials
                {
                    Username = _dataManager.Settings.GitHubToken,
                    Password = string.Empty
                }
        };

        Repository.Network.Push(remote, $"refs/heads/{currentBranch.FriendlyName}", pushOptions);
    }
    
    public Signature GetLocalSignature(Repository repo)
    {
        var name = repo.Config.Get<string>("user.name")?.Value;
        var email = repo.Config.Get<string>("user.email")?.Value;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
        {
            throw new InvalidOperationException("Git user.name or user.email not set. Please configure Git.");
        }

        return new Signature(name, email, DateTimeOffset.Now);
    }

    public BattleHints GetBattleHints()
    {
        var path = Path.Combine(_dataManager.Settings.RepositoryPath, _localizationFolder, "BattleHint.json");

        if (!File.Exists(path))
            throw new FileNotFoundException("BattleHints.json not found", path);

        var json = File.ReadAllText(path);

        var hints = JsonConvert.DeserializeObject<BattleHints>(json);

        if (hints == null)
            throw new InvalidOperationException("Failed to deserialize BattleHint.json");

        return hints;
    }

    public void AddHint(string text)
    {
        var path = Path.Combine(_dataManager.Settings.RepositoryPath, _localizationFolder, "BattleHint.json");

        // Load existing hints
        var hints = GetBattleHints();

        // Find last ID (if empty, start from 1)
        int nextId = 1;
        if (hints.DataList.Any())
        {
            // Parse all IDs to int (assuming they’re always numeric strings like "191")
            nextId = hints.DataList
                .Select(h => int.TryParse(h.Id, out var id) ? id : 0)
                .Max() + 1;
        }

        // Add new hint
        hints.DataList.Add(new BattleHint
        {
            Id = nextId.ToString(),
            Content = text
        });

        // Save back to JSON file
        var json = JsonConvert.SerializeObject(hints, Formatting.Indented);
        File.WriteAllText(path, json);
    }

}