using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LibGit2Sharp;
using Newtonsoft.Json;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Utilities;
using RainbusToolbox.Utilities.Data;
using Serilog;
using Version = System.Version;

namespace RainbusToolbox.Models.Managers;

public class RepositoryManager
{
    private readonly PersistentDataManager _dataManager;
    public readonly string LocalizationFolder = "localize";


    // Constructor
    public RepositoryManager(PersistentDataManager dataManager)
    {
        _dataManager = dataManager;
        TryInitialize();
        ParseFileMap();
    }


    public Repository Repository { get; private set; }
    public bool IsValid { get; private set; }

    private string FindRepositoryPath(string originalPath)
    {
        if (Repository.IsValid(originalPath))
            return originalPath;

        var parentDir = Directory.GetParent(originalPath)?.FullName;
        if (!string.IsNullOrWhiteSpace(parentDir) && Repository.IsValid(parentDir))
            return parentDir;

        if (Directory.Exists(originalPath))
            foreach (var subDir in Directory.GetDirectories(originalPath))
                if (Repository.IsValid(subDir))
                    return subDir;

        return null;
    }

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
    public string PathToDistribution => Path.Combine(_dataManager.Settings.RepositoryPath!, ".dist");

    // Game paths

    public string PathToGameRoot => _dataManager.Settings.PathToLimbus;

    #endregion

    #region ConstantItems

    public string PathToKeywordColorList => Path.Combine(_dataManager.Settings.RepositoryPath!, "keyword_colors.json");
    public string PathToEgoNames => Path.Combine(PathToLocalization, "Egos.json");
    public EgoLocalizationFile EgoNames;
    public EgoLocalizationFile EgoNamesReference;

    public string PathToAnnouncerNames => Path.Combine(PathToLocalization, "Announcer.json");
    public AnnouncerLocalizationFile AnnouncerNames;
    public AnnouncerLocalizationFile AnnouncerNamesReference;

    public string PathToModelCodes => Path.Combine(PathToLocalization, "ScenarioModelCodes-AutoCreated.json");
    public ScenarioModelCodesLocalizationFile ScenarioModelCodes;
    public ScenarioModelCodesLocalizationFile ScenarioModelCodesReference;

    public string PathToAnnouncerVoiceTypes => Path.Combine(PathToLocalization, "AnnouncerVoiceType.json");
    public AnnouncerVoiceTypeLocalizationFile AnnouncerVoiceTypes;

    public string PathToFileMap => Path.Combine(PathToGameRoot,
        "LimbusCompany_Data/Assets/Resources_moved/Localize/RemoteLocalizeFileList.json");

    public readonly Dictionary<string, string> DeveloperFileTypeMap = new();

    #endregion

    #region Initialization

    public void TryInitialize()
    {
        var originalPath = _dataManager.Settings.RepositoryPath!;

        try
        {
            var foundRepoPath = FindRepositoryPath(originalPath) ?? null;
            if (foundRepoPath == null ||
                string.IsNullOrWhiteSpace(_dataManager.Settings.PathToLimbus))
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
        catch (Exception ex)
        {
            IsValid = false;
            _ = App.Current.HandleNonFatalExceptionAsync(ex);
        }
    }

    public void ParseFileMap()
    {
        if (!IsValid)
            return;

        var json = File.ReadAllText(PathToFileMap);
        var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);

        foreach (var entry in parsed!)
        {
            var type = entry.Key;
            var fileNames = entry.Value;
            foreach (var fileName in fileNames) DeveloperFileTypeMap.TryAdd(fileName, type);
        }

        EgoNames = (EgoLocalizationFile)GetObjectFromPath(PathToEgoNames)!;
        EgoNamesReference = (EgoLocalizationFile)GetReference(EgoNames)!;

        ScenarioModelCodes = (ScenarioModelCodesLocalizationFile)GetObjectFromPath(PathToModelCodes)!;
        ScenarioModelCodesReference = (ScenarioModelCodesLocalizationFile)GetReference(ScenarioModelCodes)!;

        AnnouncerNames = (AnnouncerLocalizationFile)GetObjectFromPath(PathToAnnouncerNames)!;
        AnnouncerNamesReference = (AnnouncerLocalizationFile)GetReference(AnnouncerNames)!;

        AnnouncerVoiceTypes = (AnnouncerVoiceTypeLocalizationFile)GetObjectFromPath(PathToAnnouncerVoiceTypes)!;
    }

    #endregion


    #region Serialization

    public LocalizationFileBase? GetObjectFromPath(string path, LocalizationFileBase? file = null)
    {
        string rawFile;
        try
        {
            rawFile = File.ReadAllText(path, new UTF8Encoding(false));
        }
        catch
        {
            Log.Debug(AppLang.CorruptedFileNotice);
            return null;
        }

        var targetType = file?.GetType() ?? FileToObjectCaster.GetType(path, DeveloperFileTypeMap);
        if (targetType == null)
            Log.Debug(AppLang.FileIsUnknown);


        LocalizationFileBase? deserialized;
        if (targetType == null || targetType == typeof(UnidentifiedFile))
            deserialized = new UnidentifiedFile();
        else
            try
            {
                deserialized = (LocalizationFileBase?)JsonConvert.DeserializeObject(
                    rawFile,
                    targetType,
                    LocalizationJsonSettings.Default
                );
            }
            catch (Exception ex)
            {
                _ = App.Current.HandleGlobalExceptionAsync(ex);
                return null;
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
        if (refTo == null)
            return null;

        if (string.IsNullOrWhiteSpace(refTo.FileName) || string.IsNullOrWhiteSpace(refTo.FullPath))
            return null;

        if (!Directory.Exists(PathToReferenceLocalization))
            return null;

        var referenceFileName = "EN_" + refTo.FileName;
        var files = Directory.GetFiles(PathToReferenceLocalization, referenceFileName, SearchOption.AllDirectories);

        if (!files.Any())
        {
            Log.Debug("No reference files found.");
            return null;
        }

        if (files.Length > 1)
            Log.Debug("Multiple files found. This should not happen, taking the first found file.");

        var referencePath =
            files.FirstOrDefault(); // Parse one file, as in 100% of cases there must be only one matching file.

        if (referencePath == null)
            return null;

        return GetObjectFromPath(referencePath, refTo);
    }

    public bool SaveObjectToFile(LocalizationFileBase obj)
    {
        Log.Debug("=== SaveObjectToFile Debug Start ===");
        Log.Debug($"Object type: {obj?.GetType().Name}");
        Log.Debug($"FileName: '{obj?.FileName}'");
        Log.Debug($"FullPath: '{obj?.FullPath}'");

        if (string.IsNullOrWhiteSpace(obj.FileName) || string.IsNullOrWhiteSpace(obj.FullPath))
        {
            Log.Debug("ERROR: FileName or FullPath is null/empty - returning false");
            return false;
        }

        var directoryPath = Path.GetDirectoryName(obj.FullPath);
        Log.Debug($"Directory path: '{directoryPath}'");

        Directory.CreateDirectory(directoryPath!);
        Log.Debug("Directory created/verified");

        try
        {
            string json;

            Log.Debug("Checking if object is UnidentifiedFile...");
            Log.Debug($"GetType().Name == 'UnidentifiedFile': {obj.GetType().Name == "UnidentifiedFile"}");
            Log.Debug($"obj is UnidentifiedFile: {obj is UnidentifiedFile}");

            // Check if it's an UnidentifiedFile type
            if (obj.GetType().Name == "UnidentifiedFile" || obj is UnidentifiedFile)
            {
                Log.Debug("Using UnidentifiedFile serialization (no type info)");
                // For UnidentifiedFile, serialize as plain object without type information
                json = JsonConvert.SerializeObject(
                    obj,
                    Formatting.Indented,
                    LocalizationJsonSettings.Unidentified
                );
            }
            else
            {
                Log.Debug("Using normal serialization");
                // For other types, use normal serialization
                json = JsonConvert.SerializeObject(
                    obj,
                    Formatting.Indented,
                    LocalizationJsonSettings.Default
                );
            }

            Log.Debug($"JSON length: {json?.Length ?? 0} characters");

            File.WriteAllText(obj.FullPath, json, new UTF8Encoding(false));
            if (Path.GetFileName(obj.FullPath).StartsWith("BattleKeywords"))
            {
                var keywordsPath = obj.FullPath.Replace("BattleKeywords", "Bufs");
                File.WriteAllText(keywordsPath, json, new UTF8Encoding(false));
            }

            Log.Debug("File written successfully");
            Log.Debug("=== SaveObjectToFile Debug End - SUCCESS ===");
            return true;
        }
        catch (Exception e)
        {
            Log.Debug($"ERROR: Exception occurred: {e.Message}");
            Log.Debug($"Exception type: {e.GetType().Name}");
            Log.Debug($"Stack trace: {e.StackTrace}");
            Log.Debug("=== SaveObjectToFile Debug End - EXCEPTION ===");
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

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Git user.name or user.email not set. Please configure Git.");

        return new Signature(name, email, DateTimeOffset.Now);
    }

    public string GetRepoDisplayName(Repository repo)
    {
        var remoteName = repo.Head?.RemoteName;
        if (string.IsNullOrWhiteSpace(remoteName)) remoteName = repo.Network.Remotes.FirstOrDefault()?.Name;

        if (string.IsNullOrWhiteSpace(remoteName)) return string.Empty;

        var remote = repo.Network.Remotes[remoteName];
        if (remote == null) return string.Empty;

        var url = remote.Url;

        if (url.Contains(':') && !url.StartsWith("http")) url = url.Split(':').Last();

        var name = Path.GetFileNameWithoutExtension(url);
        return name ?? string.Empty;
    }


    private FetchOptions CreateFetchOptions()
    {
        return new FetchOptions
        {
            CredentialsProvider = (_, _, _) =>
                new UsernamePasswordCredentials
                {
                    Username = "token",
                    Password = _dataManager.Settings.GitHubToken
                }
        };
    }


    private Branch GetTrackedBranch(Branch branch, Remote remote)
    {
        var tracked = branch.TrackedBranch;
        if (tracked == null)
        {
            Log.Debug("Tracked branch is null. Trying to get remote branch manually.");
            tracked = Repository.Branches[$"{remote.Name}/{branch.FriendlyName}"];
        }

        return tracked;
    }

    public int[] CheckRepositoryChanges()
    {
        var branch = Repository.Head;
        Log.Debug($"Current branch: {branch.FriendlyName}");

        if (string.IsNullOrWhiteSpace(branch.RemoteName))
        {
            Log.Debug("No remote set for the current branch.");
            return new[] { 0, 0 };
        }

        var remote = Repository.Network.Remotes[branch.RemoteName];
        Log.Debug($"Remote: {remote.Name}");

        var fetchOptions = CreateFetchOptions();

        try
        {
            Log.Debug("Fetching...");
            Repository.Network.Fetch(remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), fetchOptions);
            Log.Debug("Fetch completed.");
        }
        catch (Exception ex)
        {
            Log.Debug($"Fetch failed: {ex.Message}");
            return new[] { 0, 0 };
        }

        branch = Repository.Head;
        var tracked = GetTrackedBranch(branch, remote);

        if (tracked == null)
        {
            Log.Debug($"Could not find remote branch: {remote.Name}/{branch.FriendlyName}");
            return new[] { 0, 0 };
        }

        Log.Debug($"Local branch tip: {branch.Tip.Sha}");
        Log.Debug($"Tracked branch tip: {tracked.Tip.Sha}");

        var divergence = Repository.ObjectDatabase.CalculateHistoryDivergence(branch.Tip, tracked.Tip);
        Log.Debug($"Divergence: AheadBy {divergence?.AheadBy}, BehindBy {divergence?.BehindBy}");

        return new[] { divergence?.BehindBy ?? 0, divergence?.AheadBy ?? 0 };
    }

    public void SynchronizeWithOrigin()
    {
        try
        {
            if (Repository.RetrieveStatus().IsDirty)
            {
                _ = App.Current.HandleNonFatalExceptionAsync(
                    new Exception("В проекте найдены несохранённые изменения, сначала сделай коммит."));
                return;
            }

            Log.Debug("Starting synchronization with origin...");

            FetchFromOrigin();
            Log.Debug("Fetch completed.");

            var divergence = CheckRepositoryChanges();
            var behind = divergence[0];
            var ahead = divergence[1];

            var didRebase = false;

            if (behind > 0)
            {
                didRebase = true;
                Log.Debug($"Local branch is behind by {behind} commit(s). Rebasing...");

                var identity = new Identity(
                    Repository.Config.Get<string>("user.name")?.Value,
                    Repository.Config.Get<string>("user.email")?.Value
                );
                if (string.IsNullOrWhiteSpace(identity.Name) ||
                    string.IsNullOrWhiteSpace(identity.Email))
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception("Git user.name / user.email не настроены."));
                    return;
                }

                var upstream = Repository.Branches[$"origin/{Repository.Head.FriendlyName}"];

                var result = Repository.Rebase.Start(
                    Repository.Head,
                    upstream,
                    null,
                    identity,
                    new RebaseOptions()
                );

                if (result.Status == RebaseStatus.Conflicts)
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception("Обнаружены конфликты. Синхронизация остановлена.")
                    );

                    Repository.Rebase.Abort();
                    return;
                }

                if (result.Status != RebaseStatus.Complete)
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception($"Rebase failed: {result.Status}")
                    );

                    Repository.Rebase.Abort();
                    return;
                }

                Log.Debug("Rebase completed.");
            }
            else
            {
                Log.Debug("No remote commits to rebase.");
            }

            if (ahead > 0 || didRebase)
            {
                Log.Debug(AppLang.GitLocalIsAheadNotice, ahead);
                PushToOrigin(true);
                Log.Debug(AppLang.GitPushCompleted);
            }
            else
            {
                Log.Debug(AppLang.GitNoLocalCommits);
            }

            Log.Debug(AppLang.GitSyncSuccess);
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
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

    public void PushToOrigin(bool force = false)
    {
        try
        {
            var currentBranch = Repository.Head;
            if (currentBranch == null)
                throw new Exception("No HEAD is set.");

            if (currentBranch.Tip == null)
                throw new Exception("Current branch has no commits.");

            var remote = Repository.Network.Remotes["origin"];
            if (remote == null)
                throw new Exception("Remote 'origin' not found.");

            var trackingBranch = currentBranch.TrackedBranch;

            if (trackingBranch?.Tip != null)
                if (currentBranch.Tip.Sha == trackingBranch.Tip.Sha)
                {
                    Log.Debug(AppLang.GitUpToDate);
                    return;
                }

            var pushOptions = new PushOptions
            {
                CredentialsProvider = (_, _, _) =>
                    new UsernamePasswordCredentials
                    {
                        Username = "token",
                        Password = _dataManager.Settings.GitHubToken
                    }
            };

            var prefix = force ? "+" : "";
            var refSpec = $"{prefix}refs/heads/{currentBranch.FriendlyName}:refs/heads/{currentBranch.FriendlyName}";

            Log.Debug(AppLang.PushingBranchProcess, currentBranch.FriendlyName);

            Repository.Network.Push(remote, refSpec, pushOptions);

            Log.Debug(AppLang.PushingBranchSuccess, currentBranch.FriendlyName);
        }
        catch (NonFastForwardException ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Push rejected (non-fast-forward). Remote contains changes you don’t have.\n" +
                    "If this happened after rebase, retry with force push enabled.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex) when (
            ex.Message.Contains("authentication") ||
            ex.Message.Contains("401") ||
            ex.Message.Contains("403"))
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Authentication failed during push. Check your GitHub token and permissions.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex) when (
            ex.Message.Contains("network") ||
            ex.Message.Contains("timeout"))
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Network error during push. Check your connection.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex) when (
            ex.Message.Contains("permission") ||
            ex.Message.Contains("access"))
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    "Permission denied during push. Check repository access rights.",
                    ex
                )
            );
        }
        catch (LibGit2SharpException ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception(
                    $"Git push failed: {ex.Message}",
                    ex
                )
            );
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception($"Unexpected error during push: {ex.Message}", ex)
            );
        }
    }

    public string GetLatestReleaseSemantic()
    {
        try
        {
            var semVerPattern = @"(\d+)\.(\d+)\.(\d+)";

            // Get all tags with valid semantic versions
            var tags = Repository.Tags
                .Select(t => new
                {
                    Tag = t,
                    Match = Regex.Match(t.FriendlyName, semVerPattern)
                })
                .Where(x => x.Match.Success)
                .Select(x =>
                {
                    try
                    {
                        return new
                        {
                            x.Tag,
                            Version = new Version(
                                int.Parse(x.Match.Groups[1].Value),
                                int.Parse(x.Match.Groups[2].Value),
                                int.Parse(x.Match.Groups[3].Value)
                            ),
                            VersionString = x.Match.Groups[0].Value,
                            IsValid = true
                        };
                    }
                    catch
                    {
                        Log.Debug($"Skipping tag {x.Tag.FriendlyName} - invalid version format");
                        return new { x.Tag, Version = (Version)null, VersionString = "", IsValid = false };
                    }
                })
                .Where(x => x.IsValid)
                .OrderByDescending(x => x.Version)
                .ToList();

            if (!tags.Any())
            {
                Log.Debug("No tags found in repository. Defaulting to 1.0.0");
                return "1.0.0";
            }

            var latest = tags.First();
            Log.Debug($"Found latest release: {latest.Tag.FriendlyName} as {latest.VersionString}");
            return latest.VersionString;
        }
        catch (Exception ex)
        {
            Log.Debug($"Failed to get latest release: {ex.Message}. Defaulting to 1.0.0");
            return "1.0.0";
        }
    }

    #endregion
}