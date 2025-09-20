using System;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Models.Managers;

public class RepositoryManager
{
    #region Folders

    // Relative to repo root
    private const string DistPath = ".dist/";

    // Relative to game root
    private const string ReferenceLangAppendage = "LimbusCompany_Data/Assets/Resources_moved/Localize/en/";

    #endregion

    #region AbsolutePaths

    // Repo paths
    public string RepositoryRoot { get; private set; }
    public string PathToLocalization => Path.Combine(_dataManager.Settings.RepositoryPath!, LocalizationFolder);
    public string PathToReferenceLocalization;
    public string PathToDistribution { get; private set; }

    // Game paths

    #endregion

    private string FindRepositoryPath(string originalPath)
    {
        if (Repository.IsValid(originalPath))
            return originalPath;

        var parentDir = Directory.GetParent(originalPath)?.FullName;
        if (!string.IsNullOrEmpty(parentDir) && Repository.IsValid(parentDir))
            return parentDir;

        if (Directory.Exists(originalPath))
            foreach (var subDir in Directory.GetDirectories(originalPath))
                if (Repository.IsValid(subDir))
                    return subDir;

        return null;
    }


    public Repository Repository { get; private set; }
    public readonly string LocalizationFolder = "localize";


    private readonly PersistentDataManager _dataManager;
    public bool IsValid { get; private set; }


    // Constructor
    public RepositoryManager(PersistentDataManager dataManager)
    {
        _dataManager = dataManager;
        TryInitialize();
    }


    public void ParseNewAdditionsFromGame()
    {
        var pathToGame = Path.Combine(_dataManager.Settings.PathToLimbus!, ReferenceLangAppendage);
        var pathToLocalization = Path.Combine(_dataManager.Settings.RepositoryPath!, LocalizationFolder);

        Directory.CreateDirectory(pathToLocalization);

        var gameFiles = Directory.GetFiles(pathToGame, "*.json", SearchOption.AllDirectories);

        foreach (var gameFile in gameFiles)
        {
            var relativePath = Path.GetRelativePath(pathToGame, gameFile);

            // Sanitize the file name by removing "EN_" prefix
            var fileName = Path.GetFileName(relativePath);
            if (fileName.StartsWith("EN_")) fileName = fileName.Substring(3); // Remove first 3 chars ("EN_")

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
                    File.WriteAllText(localizationFile, localizationJson.ToString(Formatting.Indented));
            }
        }

        CommitLocalChanges("Merged new files from the game [Rainbus Toolbox]");
    }

    private bool MergeJsonObjects(JToken source, JToken target)
    {
        var updated = false;

        if (source is JObject sourceObj && target is JObject targetObj)
            foreach (var prop in sourceObj.Properties())
                if (targetObj[prop.Name] == null)
                {
                    targetObj[prop.Name] = prop.Value.DeepClone();
                    updated = true;
                }
                else
                {
                    updated |= MergeJsonObjects(prop.Value, targetObj[prop.Name]!);
                }
        else if (source is JArray sourceArr && target is JArray targetArr)
            foreach (var item in sourceArr)
                if (!targetArr.Any(t => JToken.DeepEquals(t, item)))
                {
                    targetArr.Add(item.DeepClone());
                    updated = true;
                }

        return updated;
    }


    #region Initialization

    public void TryInitialize()
    {
        var originalPath = _dataManager.Settings.RepositoryPath!;

        try
        {
            var foundRepoPath = FindRepositoryPath(originalPath) ?? null;
            if (foundRepoPath == null)
            {
                IsValid = false;
                return;
            }

            if (foundRepoPath != originalPath) _dataManager.Settings.RepositoryPath = foundRepoPath;

            Repository = new Repository(foundRepoPath);
            Directory.CreateDirectory(Path.Combine(foundRepoPath, DistPath));
            PathToReferenceLocalization = Path.Combine(_dataManager.Settings.PathToLimbus!, ReferenceLangAppendage);
            IsValid = true;
        }
        catch
        {
            IsValid = false;
        }
    }

    #endregion


    #region Serialization

    public LocalizationFileBase GetObjectFromPath(string path, LocalizationFileBase? file = null)
    {
        string rawFile;
        try
        {
            rawFile = File.ReadAllText(path, new UTF8Encoding(false));
        }
        catch
        {
            Console.WriteLine("Unable to read file, please check manually.");
            return null;
        }

        var targetType = file?.GetType() ?? FileToObjectCaster.GetType(path);
        if (targetType == null)
            Console.WriteLine("Unable to determine file type from path pattern.");
        

        LocalizationFileBase? deserialized;
        if (targetType == null || targetType == typeof(UnidentifiedFile))
        {
            deserialized = new UnidentifiedFile();
        }
        else
        {
            try
            {
                deserialized = (LocalizationFileBase?)JsonConvert.DeserializeObject(rawFile, targetType);
            }
            catch (Exception ex)
            {
                App.Current.HandleGlobalExceptionAsync(ex);
                return null;
            }
        }

        if (deserialized == null)
            return null;

        var justName = Path.GetFileName(path);
        var justPath = Path.GetDirectoryName(path);

        deserialized.FileName = justName;
        deserialized.FullPath = path;
        deserialized.PathTo = justPath ?? Path.DirectorySeparatorChar.ToString();

        return deserialized;
    }


    public LocalizationFileBase? GetReference(LocalizationFileBase? refTo)
    {
        if(refTo == null)
            return null;
        
        if (string.IsNullOrEmpty(refTo.FileName) || string.IsNullOrEmpty(refTo.FullPath))
            return null;

        if (!Directory.Exists(PathToReferenceLocalization))
            return null;

        var referenceFileName = "EN_" + refTo.FileName;
        var files = Directory.GetFiles(PathToReferenceLocalization, referenceFileName, SearchOption.AllDirectories);

        if (!files.Any())
        {
            Console.WriteLine("No reference files found.");
            return null;
        }

        if (files.Length > 1)
            Console.WriteLine("Multiple files found. This should not happen, taking the first found file.");

        var referencePath =
            files.FirstOrDefault(); // Parse one file, as in 100% of cases there must be only one matching file.

        if (referencePath == null)
            return null;

        return GetObjectFromPath(referencePath, refTo);
    }

    public bool SaveObjectToFile(LocalizationFileBase obj)
    {
        if (string.IsNullOrEmpty(obj.FileName) || string.IsNullOrEmpty(obj.FullPath))
            return false;

        Directory.CreateDirectory(Path.GetDirectoryName(obj.FullPath)!);

        try
        {
            string json;
        
            // Check if it's an UnidentifiedFile type
            if (obj.GetType().Name == "UnidentifiedFile" || obj is UnidentifiedFile)
            {
                // For UnidentifiedFile, serialize as plain object without type information
                json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.None
                });
            }
            else
            {
                // For other types, use normal serialization
                json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
        
            File.WriteAllText(obj.FullPath, json, new UTF8Encoding(false));
            return true;
        }
        catch (Exception e)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(e);
            return false;
        }
    }

    #endregion

    

    #region Git shit

    public Signature GetLocalSignature(Repository repo)
    {
        var name = repo.Config.Get<string>("user.name")?.Value;
        var email = repo.Config.Get<string>("user.email")?.Value;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            throw new InvalidOperationException("Git user.name or user.email not set. Please configure Git.");

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
                    Username = "x-access-token",
                    Password = _dataManager.Settings.GitHubToken
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
        try
        {
            Console.WriteLine("Starting synchronization with origin...");

            // Step 1: Fetch latest remote info
            FetchFromOrigin();
            Console.WriteLine("Fetch completed.");

            // Step 2: Check divergence between local and remote
            var divergence = CheckRepositoryChanges();
            var behind = divergence[0];
            var ahead = divergence[1];

            if (behind > 0)
            {
                Console.WriteLine($"Local branch is behind by {behind} commit(s). Pulling...");
                PullFromOrigin();
                Console.WriteLine("Pull completed.");
            }
            else
            {
                Console.WriteLine("No remote commits to pull.");
            }

            // Step 3: Commit local changes if any
            var status = Repository.RetrieveStatus();
            if (status.IsDirty)
            {
                var changes = status
                    .Where(s => s.State != FileStatus.Ignored && s.State != FileStatus.NewInWorkdir)
                    .ToList();

                if (changes.Any())
                {
                    Console.WriteLine($"Found {changes.Count} local changes to commit.");
                    CommitLocalChanges("Synchronization of local and remote changes [RainbusToolbox]");
                    Console.WriteLine("Local changes committed.");
                    ahead++; // we just added a commit
                }
                else
                {
                    var untracked = status.Where(s => s.State == FileStatus.NewInWorkdir).ToList();
                    if (untracked.Any())
                        Console.WriteLine($"Found {untracked.Count} untracked files (not committed).");
                }
            }
            else
            {
                Console.WriteLine("No local changes to commit.");
            }

            // Step 4: Push if ahead
            if (ahead > 0)
            {
                Console.WriteLine($"Local branch is ahead by {ahead} commit(s). Pushing...");
                PushToOrigin();
                Console.WriteLine("Push completed.");
            }
            else
            {
                Console.WriteLine("No local commits to push.");
            }

            Console.WriteLine("Synchronization with origin completed successfully.");
        }
        catch (Exception ex)
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Synchronization failed: {ex.Message}", ex)
            );
        }
    }


    public void FetchFromOrigin()
    {
        var remote = Repository.Network.Remotes["origin"];
        var fetchOptions = CreateFetchOptions();
        Commands.Fetch(Repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions,
            "Fetching from origin");
    }

    public void PullFromOrigin()
    {
        try
        {
            var currentBranch = Repository.Head ?? throw new Exception("No HEAD is set.");
            var remote = Repository.Network.Remotes["origin"]
                         ?? throw new Exception("Remote 'origin' not found.");

            // Step 1: Fetch again to ensure latest remote refs
            var fetchOptions = CreateFetchOptions();
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            Commands.Fetch(Repository, remote.Name, refSpecs, fetchOptions, "Fetching before pull");

            // Step 2: Get tracking branch
            var trackingBranch = currentBranch.TrackedBranch ??
                                 Repository.Branches[$"{remote.Name}/{currentBranch.FriendlyName}"];

            if (trackingBranch == null)
                throw new Exception($"No tracking branch found for {currentBranch.FriendlyName}.");

            var localCommit = currentBranch.Tip;
            var remoteCommit = trackingBranch.Tip;

            // Step 3: Check divergence
            var divergence = Repository.ObjectDatabase.CalculateHistoryDivergence(localCommit, remoteCommit);
            var behind = divergence?.BehindBy ?? 0;
            var ahead = divergence?.AheadBy ?? 0;

            if (behind == 0)
            {
                Console.WriteLine("No remote commits to pull (up to date).");
                return;
            }

            Console.WriteLine($"Local is behind by {behind} commit(s). Updating...");

            // Step 4: Check if fast-forward possible
            var mergeBase = Repository.ObjectDatabase.FindMergeBase(localCommit, remoteCommit);
            var canFastForward = mergeBase?.Sha == localCommit.Sha;

            if (canFastForward)
            {
                Repository.Reset(ResetMode.Hard, remoteCommit);
                Console.WriteLine($"Fast-forwarded to {remoteCommit.Sha.Substring(0, 8)}");
            }
            else
            {
                var signature = GetLocalSignature(Repository);
                var mergeOptions = new MergeOptions
                {
                    FastForwardStrategy = FastForwardStrategy.Default,
                    FileConflictStrategy = CheckoutFileConflictStrategy.Normal
                };

                var mergeResult = Repository.Merge(remoteCommit, signature, mergeOptions);

                switch (mergeResult.Status)
                {
                    case MergeStatus.UpToDate:
                        Console.WriteLine("Already up to date after merge.");
                        break;

                    case MergeStatus.FastForward:
                        Console.WriteLine($"Fast-forwarded to {remoteCommit.Sha.Substring(0, 8)}");
                        break;

                    case MergeStatus.NonFastForward:
                        Console.WriteLine(
                            $"Merged successfully. New commit: {Repository.Head.Tip.Sha.Substring(0, 8)}");
                        break;

                    case MergeStatus.Conflicts:
                        var conflictedFiles = Repository.RetrieveStatus()
                            .Where(s => s.State == FileStatus.Conflicted)
                            .Select(s => s.FilePath)
                            .ToList();
                        var conflictList = string.Join("\n", conflictedFiles.Select(f => $"  {f}"));
                        throw new Exception($"Pull resulted in conflicts:\n{conflictList}\nPlease resolve manually.");
                }
            }
        }
        catch (CheckoutConflictException ex)
        {
            var conflictedFiles = Repository.RetrieveStatus()
                .Where(s => s.State == FileStatus.Conflicted)
                .Select(s => s.FilePath)
                .ToList();

            if (conflictedFiles.Any())
            {
                var conflictList = string.Join("\n", conflictedFiles.Select(f => $"  {f}"));
                App.Current.HandleGlobalExceptionAsync(
                    new Exception($"Pull failed due to checkout conflicts:\n{conflictList}", ex)
                );
            }
            else
            {
                App.Current.HandleGlobalExceptionAsync(
                    new Exception($"Pull failed due to checkout conflicts. {ex.Message}", ex)
                );
            }
        }
        catch (Exception ex)
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Pull failed: {ex.Message}", ex)
            );
        }
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
        try
        {
            // Validate repository state
            var currentBranch = Repository.Head;
            if (currentBranch == null)
                throw new Exception("No HEAD is set.");

            if (currentBranch.Tip == null)
                throw new Exception("Current branch has no commits.");

            // Get the remote
            var remote = Repository.Network.Remotes["origin"];
            if (remote == null)
                throw new Exception("Remote 'origin' not found.");

            // Check if there are any commits to push
            var trackingBranch = currentBranch.TrackedBranch;
            if (trackingBranch != null)
            {
                var localCommit = currentBranch.Tip;
                var remoteCommit = trackingBranch.Tip;

                if (localCommit.Sha == remoteCommit.Sha)
                {
                    Console.WriteLine("Already up to date with remote.");
                    return;
                }
            }

            // Create push options with GitHub token
            var pushOptions = new PushOptions
            {
                CredentialsProvider = (url, user, cred) =>
                    new UsernamePasswordCredentials
                    {
                        Username = "token", // GitHub uses "token" as username for personal access tokens
                        Password = _dataManager.Settings.GitHubToken
                    }
            };

            // Construct the refspec
            var refSpec = $"refs/heads/{currentBranch.FriendlyName}:refs/heads/{currentBranch.FriendlyName}";

            Console.WriteLine($"Pushing branch '{currentBranch.FriendlyName}' to origin...");

            // Perform the push
            Repository.Network.Push(remote, refSpec, pushOptions);

            Console.WriteLine($"Successfully pushed branch '{currentBranch.FriendlyName}' to origin.");
        }
        catch (NonFastForwardException ex)
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Push rejected because the remote contains work that you do not have locally. " +
                              $"Try pulling from origin first to integrate remote changes.\n" +
                              $"Details: {ex.Message}", ex)
            );
        }
        catch (LibGit2SharpException ex) when (ex.Message.Contains("authentication") || ex.Message.Contains("401") ||
                                               ex.Message.Contains("403"))
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Authentication failed during push. Please check your GitHub token.\n" +
                              $"Make sure the token has 'repo' permissions and is not expired.\n" +
                              $"Details: {ex.Message}", ex)
            );
        }
        catch (LibGit2SharpException ex) when (ex.Message.Contains("network") || ex.Message.Contains("timeout"))
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Network error during push. Please check your internet connection.\n" +
                              $"Details: {ex.Message}", ex)
            );
        }
        catch (LibGit2SharpException ex) when (ex.Message.Contains("permission") || ex.Message.Contains("access"))
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Permission denied during push. Please check repository access rights.\n" +
                              $"Make sure your GitHub token has write access to this repository.\n" +
                              $"Details: {ex.Message}", ex)
            );
        }
        catch (LibGit2SharpException ex)
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Git push failed: {ex.Message}\n" +
                              $"This could be due to:\n" +
                              $"- Invalid or expired GitHub token\n" +
                              $"- Insufficient token permissions (needs 'repo' scope)\n" +
                              $"- Network connectivity problems\n" +
                              $"- Repository permission restrictions\n" +
                              $"- Branch protection rules", ex)
            );
        }
        catch (Exception ex)
        {
            App.Current.HandleGlobalExceptionAsync(
                new Exception($"Unexpected error during push: {ex.Message}", ex)
            );
        }
    }

    #endregion
}