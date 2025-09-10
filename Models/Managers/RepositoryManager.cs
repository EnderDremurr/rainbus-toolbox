using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json;
using RainbusTools.Utilities.Data;

namespace RainbusTools.Models.Managers
{
    public class RepositoryManager
    {
        public Repository Repository { get; private set; }
        private readonly string _distPath = ".dist/";
        private readonly string _packageConfigName = "config.json";
        private readonly string _localizationFolder = "localize";
        private readonly string _referenceLangAppendage = "/LimbusCompany_Data/Assets/Resources_moved/Localize/en/";
        private string _pathToReference = string.Empty;
        private readonly PersistentDataManager _dataManager;

        public bool IsValid { get; private set; }

        public RepositoryManager(PersistentDataManager dataManager)
        {
            _dataManager = dataManager;
            TryInitialize();
        }

        public void TryInitialize()
        {
            var path = _dataManager.Settings.RepositoryPath;

            try
            {
                if (!Repository.IsValid(path))
                {
                    IsValid = false;
                    return;
                }

                Repository = new Repository(path);
                Directory.CreateDirectory(Path.Combine(path, _distPath));
                _pathToReference = Path.Combine(_dataManager.Settings.PathToLimbus, _referenceLangAppendage);
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }

        public int[] CheckRepositoryChanges()
        {
            var branch = Repository.Head;
            Console.WriteLine($"Current branch: {branch.FriendlyName}");

            if (string.IsNullOrEmpty(branch.RemoteName))
            {
                Console.WriteLine("No remote set for the current branch.");
                return new[] { 0, 0 };
            }

            var remote = Repository.Network.Remotes[branch.RemoteName];
            Console.WriteLine($"Remote: {remote.Name}");

            var fetchOptions = CreateFetchOptions();

            try
            {
                Console.WriteLine("Fetching...");
                Repository.Network.Fetch(remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions);
                Console.WriteLine("Fetch completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fetch failed: {ex.Message}");
                return new[] { 0, 0 };
            }

            branch = Repository.Head;
            var tracked = GetTrackedBranch(branch, remote);

            if (tracked == null)
            {
                Console.WriteLine($"Could not find remote branch: {remote.Name}/{branch.FriendlyName}");
                return new[] { 0, 0 };
            }

            Console.WriteLine($"Local branch tip: {branch.Tip.Sha}");
            Console.WriteLine($"Tracked branch tip: {tracked.Tip.Sha}");

            var divergence = Repository.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, tracked.Tip);
            Console.WriteLine($"Divergence: AheadBy {divergence?.AheadBy}, BehindBy {divergence?.BehindBy}");

            return new[] { divergence?.BehindBy ?? 0, divergence?.AheadBy ?? 0 };
        }

        public string PackageLocalization(string version)
        {
            SynchronizeWithOrigin();

            var repoPath = Repository.Info.WorkingDirectory;
            var zipFileName = $"RCR v{version}.zip";
            var zipPath = Path.Combine(repoPath, _distPath, zipFileName);

            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                AddLocalizationFilesToZip(zip, repoPath);
            }

            return zipPath;
        }

        public void DeleteHintAtId(int id)
        {
            var path = Path.Combine(_dataManager.Settings.RepositoryPath, _localizationFolder, "BattleHint.json");
            var hints = GetBattleHints();

            var hintToRemove = hints.DataList.FirstOrDefault(h => int.TryParse(h.Id, out var hId) && hId == id);
            if (hintToRemove == null)
            {
                Console.WriteLine($"Hint with ID {id} not found.");
                return;
            }

            hints.DataList.Remove(hintToRemove);
            ReassignIds(hints);

            var json = JsonConvert.SerializeObject(hints, Formatting.Indented);
            File.WriteAllText(path, json);
            Console.WriteLine($"Hint with original ID {id} deleted. IDs updated sequentially.");
        }

        public void SynchronizeWithOrigin()
{
    FetchFromOrigin();
    MergeWithOrigin();      
    CommitLocalChanges("Synchronization of local and remote changes [RainbusTools]");   
    PushToOrigin();
}

public void FetchFromOrigin()
{
    var remote = Repository.Network.Remotes["origin"];
    var fetchOptions = CreateFetchOptions();

    Commands.Fetch(Repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions, "Fetching from origin");
}

public void MergeWithOrigin()
{
    var currentBranch = Repository.Head;
    var remoteBranch = Repository.Branches[$"origin/{currentBranch.FriendlyName}"];

    if (remoteBranch == null)
        throw new Exception($"Remote branch origin/{currentBranch.FriendlyName} not found.");

    var merger = Repository.Merge(remoteBranch, GetLocalSignature(Repository));

    if (merger.Status == MergeStatus.Conflicts)
        throw new Exception("Merge conflicts occurred. Please resolve manually.");
}

public void CommitLocalChanges(string comment)
{
    // Stage all changes (new, modified, deleted, renamed, type change)
    Commands.Stage(Repository, "*");

    // Check if there are any staged changes
    if (Repository.RetrieveStatus().Any(entry => entry.State.HasFlag(FileStatus.NewInIndex) ||
                                                 entry.State.HasFlag(FileStatus.ModifiedInIndex) ||
                                                 entry.State.HasFlag(FileStatus.DeletedFromIndex) ||
                                                 entry.State.HasFlag(FileStatus.RenamedInIndex) ||
                                                 entry.State.HasFlag(FileStatus.TypeChangeInIndex)))
    {
        var author = GetLocalSignature(Repository);
        Repository.Commit(comment, author, author);
    }
}

public void PushToOrigin()
{
    var remote = Repository.Network.Remotes["origin"];
    var currentBranch = Repository.Head;
    var pushOptions = CreatePushOptions();

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
            var hints = GetBattleHints();

            int nextId = hints.DataList.Any() ? hints.DataList.Max(h => int.TryParse(h.Id, out var id) ? id : 0) + 1 : 1;

            hints.DataList.Add(new BattleHint
            {
                Id = nextId.ToString(),
                Content = text
            });

            var json = JsonConvert.SerializeObject(hints, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private FetchOptions CreateFetchOptions()
        {
            return new FetchOptions
            {
                CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = "token",
                        Password = _dataManager.Settings.GitHubToken
                    }
            };
        }

        private PushOptions CreatePushOptions()
        {
            return new PushOptions
            {
                CredentialsProvider = (_url, _user, _cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = _dataManager.Settings.GitHubToken,
                        Password = string.Empty
                    }
            };
        }

        private Branch GetTrackedBranch(Branch branch, Remote remote)
        {
            var tracked = branch.TrackedBranch;
            if (tracked == null)
            {
                Console.WriteLine("Tracked branch is null. Trying to get remote branch manually.");
                tracked = Repository.Branches[$"{remote.Name}/{branch.FriendlyName}"];
            }
            return tracked;
        }

        private void AddLocalizationFilesToZip(ZipArchive zip, string repoPath)
        {
            var localizePath = Path.Combine(repoPath, _localizationFolder);
            if (Directory.Exists(localizePath))
            {
                foreach (var file in Directory.GetFiles(localizePath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(localizePath, file);
                    zip.CreateEntryFromFile(file, relativePath);
                }
            }
        }

        private void ReassignIds(BattleHints hints)
        {
            for (int i = 0; i < hints.DataList.Count; i++)
            {
                hints.DataList[i].Id = (i + 1).ToString();
            }
        }
    }
}
