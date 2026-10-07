using System.Collections.Generic;
using System.IO;
using System.Text;
using LibGit2Sharp;
using Newtonsoft.Json;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Utilities;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Models.Managers;

public sealed class LocalizationManager
{
    private readonly GitManager _gitManager;
    private readonly PersistentDataManager _persistentDataManager;

    public readonly string LocalizationFolder = "localize";


    // Constructor
    public LocalizationManager(PersistentDataManager persistentDataManager, GitManager gitManager)
    {
        _persistentDataManager = persistentDataManager;
        _gitManager = gitManager;

        TryInitialize(); // TODO: ig it won't hurt to split this into game path initializer and repo and shi, cuz right now changing any would reset both
    }


    public bool IsValid { get; private set; }

    #region Events

    public event Action? OnInitializedSuccessfully;

    #endregion

    #region Folders

    // Relative to repo root
    private const string DistPath = ".dist/";

    // Relative to game root
    private const string ReferenceLangAppendage = "LimbusCompany_Data/Assets/Resources_moved/Localize/en/";

    #endregion

    #region AbsolutePaths

    // Repo paths
    public required string RepositoryRoot;
    public string PathToLocalization => Path.Combine(RepositoryRoot, LocalizationFolder);
    public required string PathToReferenceLocalization = null!;
    public string PathToDistribution => Path.Combine(RepositoryRoot, DistPath);
    public string PathToVSCodeSettings => Path.Combine(RepositoryRoot, ".vscode/settings.json");
    public string PathToRegexJson => Path.Combine(RepositoryRoot, "regexes.json");

    // Game paths

    public required string PathToGameRoot;

    #endregion

    #region ConstantItems

    public string PathToKeywordColorList =>
        Path.Combine(_persistentDataManager.Settings.RepositoryPath!, "keyword_colors.json");

    public string PathToEgoNames => Path.Combine(PathToLocalization, "Egos.json");
    public required EgoLocalizationFile EgoNames { get; set; }
    public required EgoLocalizationFile EgoNamesReference { get; set; }

    public string PathToAnnouncerNames => Path.Combine(PathToLocalization, "Announcer.json");
    public required AnnouncerLocalizationFile AnnouncerNames { get; set; }
    public required AnnouncerLocalizationFile AnnouncerNamesReference { get; set; }

    public string PathToModelCodes => Path.Combine(PathToLocalization, "ScenarioModelCodes-AutoCreated.json");
    public required ScenarioModelCodesLocalizationFile ScenarioModelCodes { get; set; }
    public required ScenarioModelCodesLocalizationFile ScenarioModelCodesReference { get; set; }

    public string PathToAnnouncerVoiceTypes => Path.Combine(PathToLocalization, "AnnouncerVoiceType.json");
    public required AnnouncerVoiceTypeLocalizationFile AnnouncerVoiceTypes { get; set; }

    public string PathToFileMap => Path.Combine(PathToGameRoot,
        "LimbusCompany_Data/Assets/Resources_moved/Localize/RemoteLocalizeFileList.json");

    public readonly Dictionary<string, string> DeveloperFileTypeMap = new();

    #endregion

    #region Initialization

    public bool TryInitialize()
    {
        var oldRepoPath = RepositoryRoot;
        var oldLimbusPath = PathToGameRoot;

        var originalRepoPath = _persistentDataManager.Settings.RepositoryPath;
        var originalLimbusPath = _persistentDataManager.Settings.PathToLimbus;

        Repository? tempRepository = null;

        var didSucceed = false;

        try
        {
            var validatedRepoPath = PersistentDataManager.ValidateRepoPath(originalRepoPath);
            var validatedGamePath = PersistentDataManager.ValidateLimbusPath(originalLimbusPath);
            if (validatedRepoPath == null || validatedGamePath == null)
                throw new InvalidOperationException("Path to localization repo or game is not valid!");

            // TODO: this will be split later cuz right now this shit is too tangled ngl

            // try creating a repo into local first, to not override if th throws
            tempRepository = new Repository(validatedRepoPath);
            Directory.CreateDirectory(Path.Combine(validatedRepoPath,
                DistPath)); // i think this all should be moved into git manager, and also
            // TODO: add persistent data manager events so i can properly subscribe my shit to changes!!!

            // overwrite only if nothing threw
            if (validatedRepoPath != originalRepoPath)
                _persistentDataManager.Settings.RepositoryPath = validatedRepoPath;
            if (validatedGamePath != originalLimbusPath)
                _persistentDataManager.Settings.PathToLimbus = validatedGamePath;

            RepositoryRoot = validatedRepoPath;
            PathToGameRoot = validatedGamePath;
            PathToReferenceLocalization = Path.Combine(validatedGamePath, ReferenceLangAppendage);


            var oldRepository = _gitManager.Repository;
            _gitManager.SetRepository(tempRepository);

            didSucceed = true;
            IsValid = true;

            tempRepository = null;
            oldRepository?.Dispose();

            ParseFileMap();
        }
        catch (Exception ex)
        {
            tempRepository?.Dispose();
            _ = App.Current.HandleNonFatalExceptionAsync(ex);

            if (!string.IsNullOrEmpty(oldLimbusPath) && !string.IsNullOrEmpty(oldRepoPath))
            {
                _persistentDataManager.Settings.PathToLimbus = oldLimbusPath;
                _persistentDataManager.Settings.RepositoryPath = oldRepoPath;
                // revert old paths
            }
        }

        if (didSucceed)
            OnInitializedSuccessfully?.Invoke();
        return didSucceed;
    }

    public void ParseFileMap()
    {
        if (!IsValid)
            return;

        var json = File.ReadAllText(PathToFileMap);
        var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        DeveloperFileTypeMap.Clear();

        foreach (var (type, fileNames) in parsed!)
        foreach (var fileName in fileNames)
            DeveloperFileTypeMap.TryAdd(fileName, type);

        var localizedEgoNames = (EgoLocalizationFile?)GetObjectFromPath(PathToEgoNames);
        var egoNamesReference = (EgoLocalizationFile?)GetReference(localizedEgoNames);

        var localizedScenarioModelCodes = (ScenarioModelCodesLocalizationFile?)GetObjectFromPath(PathToModelCodes);
        var referenceScenarioModelCodes =
            (ScenarioModelCodesLocalizationFile?)GetReference(localizedScenarioModelCodes);

        var localizedAnnouncerNames = (AnnouncerLocalizationFile?)GetObjectFromPath(PathToAnnouncerNames);
        var referenceAnnouncerNames = (AnnouncerLocalizationFile?)GetReference(localizedAnnouncerNames);

        var announcerVoiceTypes = (AnnouncerVoiceTypeLocalizationFile?)GetObjectFromPath(PathToAnnouncerVoiceTypes);

        if (localizedEgoNames == null || egoNamesReference == null || localizedScenarioModelCodes == null ||
            referenceScenarioModelCodes == null || localizedAnnouncerNames == null || referenceAnnouncerNames == null ||
            announcerVoiceTypes == null)
            throw new InvalidOperationException(
                "Некоторые из обязательных файлов перевода отсутствуют или были испорчены!");

        EgoNames = localizedEgoNames;
        EgoNamesReference = egoNamesReference;

        ScenarioModelCodes = localizedScenarioModelCodes;
        ScenarioModelCodesReference = referenceScenarioModelCodes;

        AnnouncerNames = localizedAnnouncerNames;
        AnnouncerNamesReference = referenceAnnouncerNames;

        AnnouncerVoiceTypes = announcerVoiceTypes;
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
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(
                new InvalidOperationException("Unable to read file, please check manually!", ex));
            return null;
        }

        var targetType = file?.GetType() ?? FileToObjectCaster.GetType(path, DeveloperFileTypeMap);
        if (targetType == null)
            Log.Debug("Unable to determine file type from path pattern!");


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


    // TODO: check nullables on these later, cuz if i think about it, reference should never be null, as it's pulled from the game, or dunno tbh
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

        switch (files.Length)
        {
            case 0:
                Log.Debug("No reference files found");
                return null;
            case > 1:
                Log.Debug("Multiple files found. This should not happen, taking the first found file");
                break;
        }

        var referencePath =
            files.FirstOrDefault(); // Parse one file, as in 100% of cases there must be only one matching file.

        return referencePath == null ? null : GetObjectFromPath(referencePath, refTo);
    }

    public static bool SaveObjectToFile(LocalizationFileBase obj)
    {
        Log.Debug("Saving an object: {Name}, FileName: {ObjFileName}, FullPath: {ObjFullPath}", obj.GetType().Name,
            obj.FileName, obj.FullPath);

        if (string.IsNullOrWhiteSpace(obj.FileName) || string.IsNullOrWhiteSpace(obj.FullPath))
        {
            Log.Debug("FileName or FullPath is null/empty - returning false");
            return false;
        }

        var directoryPath = Path.GetDirectoryName(obj.FullPath);
        Directory.CreateDirectory(directoryPath!);
        Log.Debug("Directory path: '{DirectoryPath}'", directoryPath);

        try
        {
            string json;

            if (obj is UnidentifiedFile)
            {
                Log.Debug("Using UnidentifiedFile serialization (no type info)");

                json = JsonConvert.SerializeObject(
                    obj,
                    Formatting.Indented,
                    LocalizationJsonSettings.Unidentified
                );
            }
            else
            {
                Log.Debug("Using normal serialization");

                json = JsonConvert.SerializeObject(
                    obj,
                    Formatting.Indented,
                    LocalizationJsonSettings.Default
                );
            }

            Log.Debug("JSON length: {JsonLength} characters", json.Length);


            File.WriteAllText(obj.FullPath, json, new UTF8Encoding(false));

            // edge case of cloned original files, always force to open only one of those and also save them as copies
            if (!Path.GetFileName(obj.FullPath).StartsWith("BattleKeywords"))
                return true;
            var keywordsPath = obj.FullPath.Replace("BattleKeywords", "Bufs");
            File.WriteAllText(keywordsPath, json, new UTF8Encoding(false));
            return true;
        }
        catch (Exception e)
        {
            Log.Debug("{Error}", e.ToString());
            _ = App.Current.HandleNonFatalExceptionAsync(e);
            return false;
        }
    }

    #endregion
}