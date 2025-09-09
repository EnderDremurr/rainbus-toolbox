using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LibGit2Sharp;

namespace RainbusTools.Converters.Managers;

public class RepositoryManager
{
    public Repository Repository { get; private set; }
    private string _distPath = ".dist/";
    
    private string _packageConfigName = "config.json";
    private string _localizationFolder = "localize/";
    
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


}