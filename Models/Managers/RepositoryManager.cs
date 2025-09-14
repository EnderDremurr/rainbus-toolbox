using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Models.Managers
{
    public class RepositoryManager
    {
        // Fields
        public Repository Repository { get; private set; }
        private readonly string _distPath = ".dist/";
        private readonly string _packageConfigName = "config.json";
        private readonly string _localizationFolder = "localize";
        private readonly string _referenceLangAppendage = "LimbusCompany_Data/Assets/Resources_moved/Localize/en/";
        private string _pathToReference = string.Empty;
        private readonly PersistentDataManager _dataManager;
        public bool IsValid { get; private set; }
        
        private static readonly Dictionary<string, System.Type> FileTypeMap = new Dictionary<string, System.Type>
        {
            { "*AbDlg*", typeof(DialogueFile) },
            { "EGOgift*", typeof(EGOGiftFile) },
            { "*Announcer*", typeof(BattleAnnouncerFile) },
            { "*Voice*", typeof(PersonalityVoiceFile) }
        };

        // Constructor
        public RepositoryManager(PersistentDataManager dataManager)
        {
            _dataManager = dataManager;
            TryInitialize();
        }

        public void UpdateToGame() => ParseNewAdditionsFromGame();

        public void ParseNewAdditionsFromGame()
        {
            var pathToGame = Path.Combine(_dataManager.Settings.PathToLimbus, _referenceLangAppendage);
            var pathToLocalization = Path.Combine(_dataManager.Settings.RepositoryPath, _localizationFolder);

            Directory.CreateDirectory(pathToLocalization);

            var gameFiles = Directory.GetFiles(pathToGame, "*.json", SearchOption.AllDirectories);

            foreach (var gameFile in gameFiles)
            {
                var relativePath = Path.GetRelativePath(pathToGame, gameFile);

                // Sanitize the file name by removing "EN_" prefix
                var fileName = Path.GetFileName(relativePath);
                if (fileName.StartsWith("EN_"))
                {
                    fileName = fileName.Substring(3); // Remove first 3 chars ("EN_")
                }

                var relativeDirectory = Path.GetDirectoryName(relativePath)!;
                var localizationFile = Path.Combine(pathToLocalization, relativeDirectory, fileName);

                Directory.CreateDirectory(Path.GetDirectoryName(localizationFile)!);

                if (!File.Exists(localizationFile))
                {
                    File.Copy(gameFile, localizationFile);
                }
                else
                {
                    var gameJson = JObject.Parse(File.ReadAllText(gameFile));
                    var localizationJson = JObject.Parse(File.ReadAllText(localizationFile));

                    if (MergeJsonObjects(gameJson, localizationJson))
                    {
                        File.WriteAllText(localizationFile, localizationJson.ToString(Formatting.Indented));
                    }
                }
            }

            CommitLocalChanges("Merged new files from the game [Rainbus Toolbox]");
        }


        private bool MergeJsonObjects(JToken source, JToken target)
        {
            bool updated = false;

            if (source is JObject sourceObj && target is JObject targetObj)
            {
                foreach (var prop in sourceObj.Properties())
                {
                    if (targetObj[prop.Name] == null)
                    {
                        targetObj[prop.Name] = prop.Value.DeepClone();
                        updated = true;
                    }
                    else
                    {
                        updated |= MergeJsonObjects(prop.Value, targetObj[prop.Name]!);
                    }
                }
            }
            else if (source is JArray sourceArr && target is JArray targetArr)
            {
                foreach (var item in sourceArr)
                {
                    if (!targetArr.Any(t => JToken.DeepEquals(t, item)))
                    {
                        targetArr.Add(item.DeepClone());
                        updated = true;
                    }
                }
            }

            return updated;
        }
        
        public static LocalizationFileBase DeserializeJsonFile(string filePath)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        
        Type targetType = GetFileTypeFromPattern(fileName);
        if (targetType == null)
        {
            throw new ArgumentException($"No mapping found for file: {fileName}");
        }

        string jsonContent = File.ReadAllText(filePath);
        var result = (LocalizationFileBase)JsonConvert.DeserializeObject(jsonContent, targetType);
        
        // Set the path info after deserialization
        result.SetPathInfo(filePath);
        
        return result;
    }

    // Pattern matching method that handles wildcards
    private static Type GetFileTypeFromPattern(string fileName)
    {
        foreach (var kvp in FileTypeMap)
        {
            if (MatchesPattern(fileName, kvp.Key))
            {
                return kvp.Value;
            }
        }
        return null;
    }

    // Wildcard pattern matching method
    private static bool MatchesPattern(string fileName, string pattern)
    {
        // Handle exact matches (no wildcards)
        if (!pattern.Contains("*"))
        {
            return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        // Convert wildcard pattern to regex-like matching
        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        {
            // *text* - contains
            string searchText = pattern.Substring(1, pattern.Length - 2);
            return fileName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.StartsWith("*"))
        {
            // *text - ends with
            string suffix = pattern.Substring(1);
            return fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.EndsWith("*"))
        {
            // text* - starts with
            string prefix = pattern.Substring(0, pattern.Length - 1);
            return fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // Handle more complex patterns if needed
        return false;
    }

    // Helper methods
    public static Type GetFileType(string fileName)
    {
        return GetFileTypeFromPattern(fileName);
    }

    public static bool IsRecognizedFile(string fileName)
    {
        return GetFileTypeFromPattern(fileName) != null;
    }
        
        public static LocalizationFileBase DeserializeLocalizationFile(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
        
            if (!FileTypeMap.TryGetValue(fileName, out System.Type targetType))
            {
                throw new System.ArgumentException($"No mapping found for file: {fileName}");
            }

            string jsonContent = File.ReadAllText(filePath);
            var result = (LocalizationFileBase)JsonConvert.DeserializeObject(jsonContent, targetType);
        
            // Set the path info after deserialization
            result.SetPathInfo(filePath);
        
            return result;
        }

        #region Initialization
        public void TryInitialize()
        {
            var originalPath = _dataManager.Settings.RepositoryPath;

            try
            {
                var foundRepoPath = FindRepositoryPath(originalPath);
                if (foundRepoPath == null)
                {
                    IsValid = false;
                    return;
                }

                if (foundRepoPath != originalPath)
                {
                    _dataManager.Settings.RepositoryPath = foundRepoPath;
                }

                Repository = new Repository(foundRepoPath);
                Directory.CreateDirectory(Path.Combine(foundRepoPath, _distPath));
                _pathToReference = Path.Combine(_dataManager.Settings.PathToLimbus, _referenceLangAppendage);
                IsValid = true;
            }
            catch
            {
                IsValid = false;
            }
        }

        private string FindRepositoryPath(string originalPath)
        {
            if (Repository.IsValid(originalPath))
            {
                return originalPath;
            }

            var parentDir = Directory.GetParent(originalPath)?.FullName;
            if (!string.IsNullOrEmpty(parentDir) && Repository.IsValid(parentDir))
            {
                return parentDir;
            }

            if (Directory.Exists(originalPath))
            {
                foreach (var subDir in Directory.GetDirectories(originalPath))
                {
                    if (Repository.IsValid(subDir))
                    {
                        return subDir;
                    }
                }
            }

            return null;
        }
        #endregion

        #region Repository Operations
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

        public void SynchronizeWithOrigin()
        {
            FetchFromOrigin();
            MergeWithOrigin();
            CommitLocalChanges("Synchronization of local and remote changes [RainbusToolbox]");
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
            Commands.Stage(Repository, "*");

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
        #endregion

        #region Localization Management
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

        public void DeleteHintAtId(int id, BattleHintTypes hintType)
        {
            var path = GetBattleHintPath(hintType);;
            var hints = GetBattleHints(hintType);

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
        
        public void UpdateHint(int id, string newContent, BattleHintTypes hintType)
        {
            var path = GetBattleHintPath(hintType);
            var hints = GetBattleHints(hintType);

            var hintToUpdate = hints.DataList.FirstOrDefault(h => int.TryParse(h.Id, out var hId) && hId == id);
            if (hintToUpdate == null)
            {
                Console.WriteLine($"Hint with ID {id} not found.");
                return;
            }

            hintToUpdate.Content = newContent;

            var json = JsonConvert.SerializeObject(hints, Formatting.Indented);
            File.WriteAllText(path, json);
            Console.WriteLine($"Hint with ID {id} updated successfully.");
        }

        public void AddHint(string text, BattleHintTypes hintType)
        {
            var path = GetBattleHintPath(hintType);
            var hints = GetBattleHints(hintType);

            int nextId = hints.DataList.Any() ? hints.DataList.Max(h => int.TryParse(h.Id, out var id) ? id : 0) + 1 : 1;

            hints.DataList.Add(new GenericIdContent()
            {
                Id = nextId.ToString(),
                Content = text
            });

            var json = JsonConvert.SerializeObject(hints, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public BattleHintsFile GetBattleHints(BattleHintTypes hintType)
        {
            var path = GetBattleHintPath(hintType);

            if (!File.Exists(path))
                throw new FileNotFoundException("BattleHints.json not found", path);

            var json = File.ReadAllText(path);
            var hints = JsonConvert.DeserializeObject<BattleHintsFile>(json);

            if (hints == null)
                throw new InvalidOperationException("Failed to deserialize BattleHint.json");

            return hints;
        }

        public string GetBattleHintPath(BattleHintTypes hintType)
        {
            var hintName = "";
            switch (hintType)

            {
             case BattleHintTypes.Loading:
                 hintName = "BattleHint.json";
                 break;
             case BattleHintTypes.Battle:
                 hintName = "BattleHint_NormalBattle.json";
                 break;
             case BattleHintTypes.Abnormality:
                 hintName = "BattleHint_AbnorBattle.json";
                 break;
            }
            
            
            
            var path = Path.Combine(_dataManager.Settings.RepositoryPath, _localizationFolder, hintName);
            
            return path;
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

        private void ReassignIds(BattleHintsFile hints)
        {
            for (int i = 0; i < hints.DataList.Count; i++)
            {
                hints.DataList[i].Id = (i + 1).ToString();
            }
        }
        #endregion

        #region Git Operations
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
        #endregion
    }
}
