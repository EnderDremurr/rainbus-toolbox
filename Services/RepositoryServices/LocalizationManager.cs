using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.Utilities;

namespace RainbusToolbox.Services.RepositoryServices;

public sealed class LocalizationManager
{
    private readonly GitManager _gitManager;
    private readonly PersistentDataManager _persistentDataManager;
    private bool _areGameFilesValid;


    private bool _isLocalizationValid;


    // Constructor
    public LocalizationManager(PersistentDataManager persistentDataManager,
        GitManager gitManager) // this service now requires proper working git first
    {
        _persistentDataManager = persistentDataManager;
        _gitManager = gitManager;

        TryInitializeGameFiles(); // game files init first, because they contain file type map that's needed further
        persistentDataManager.OnLimbusPathChanged += TryInitializeGameFiles;

        TryInitializeLocalization();
        _gitManager.OnInitializedSuccessfully += TryInitializeLocalization;
    }

    public bool IsValid => _isLocalizationValid && _areGameFilesValid;

    #region Events

    public event Action? OnInitializedSuccessfully;

    #endregion

    #region Path constants

    // Relative to repo root
    private const string DistPath = ".dist/";
    public const string LocalizationFolder = "localize";

    // Relative to game root
    private const string ReferenceLangFolder = "LimbusCompany_Data/Assets/Resources_moved/Localize/en/";

    private const string RemoteLocalizeFileListPath =
        "LimbusCompany_Data/Assets/Resources_moved/Localize/RemoteLocalizeFileList.json";

    #endregion

    #region AbsolutePaths

    // Repo paths

    public string PathToLocalization { get; private set; }
    public string PathToReferenceLocalization;


    public string PathToKeywordColorList =>
        Path.Combine(_persistentDataManager.Settings.RepositoryPath!, "keyword_colors.json");

    public string PathToDistribution => Path.Combine(_gitManager.RepositoryRootPath, DistPath);
    public string PathToVSCodeSettings => Path.Combine(_gitManager.RepositoryRootPath, ".vscode/settings.json");

    public string PathToRegexJson => Path.Combine(_gitManager.RepositoryRootPath, "regexes.json");
    // i wonder do these four need to be here? maybe i should move those paths to be resolved where they're actually are used TODO: probably move these to the places they're accessed from

    // Game paths

    public required string PathToGameRoot;

    #endregion

    #region Global files

    // translations + references that are required to be constantly available for some other editors


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


    public string PathToFileMap { get; private set; }
    public readonly Dictionary<string, string> DeveloperFileTypeMap = new();

    #endregion

    #region Initialization

    public void TryInitializeLocalization()
    {
        Directory.CreateDirectory(Path.Combine(_gitManager.RepositoryRootPath,
            DistPath)); // just ensure dist for now, cuz i'm not sure what to init here? or no, i actually do -
        // TODO: make all constant paths be retrieved here instead of anon funcs, so they will be ensured to exist

        PathToLocalization = Path.Combine(_gitManager.RepositoryRootPath, LocalizationFolder);
        Directory.CreateDirectory(PathToLocalization);
        // tbh since this folder is very variable, i don't think i should to really check anything? i just have to make sure localize folder is present
        // and other checks are mmm
        // or i can? but then this thing won't allow empty new translations, and people would need to make the first init manually (or should i add such function into init window later?
        // yeah tbh i feel that'd be better, so i'll global files retrieval into two inits instead of a unified method
        // actually, i've tried that but there's still was a problem cuz i needed localized files first to get their references, so i still have to init them in one place
        // for now i think they'd better sit here

        #region Global files init (localized)

        var localizedEgoNames = (EgoLocalizationFile?)GetObjectFromPath(PathToEgoNames);
        var localizedScenarioModelCodes = (ScenarioModelCodesLocalizationFile?)GetObjectFromPath(PathToModelCodes);
        var localizedAnnouncerNames = (AnnouncerLocalizationFile?)GetObjectFromPath(PathToAnnouncerNames);
        var announcerVoiceTypes = (AnnouncerVoiceTypeLocalizationFile?)GetObjectFromPath(PathToAnnouncerVoiceTypes);

        if (localizedEgoNames == null || localizedScenarioModelCodes == null ||
            localizedAnnouncerNames == null ||
            announcerVoiceTypes == null)
            throw new InvalidOperationException(
                "Некоторые из обязательных файлов перевода отсутствуют или были испорчены!");

        EgoNames = localizedEgoNames;
        ScenarioModelCodes = localizedScenarioModelCodes;
        AnnouncerNames = localizedAnnouncerNames;
        AnnouncerVoiceTypes = announcerVoiceTypes;

        #endregion

        #region Global files init (reference)

        var egoNamesReference = (EgoLocalizationFile?)GetReference(EgoNames);
        var referenceScenarioModelCodes =
            (ScenarioModelCodesLocalizationFile?)GetReference(ScenarioModelCodes);
        var referenceAnnouncerNames = (AnnouncerLocalizationFile?)GetReference(AnnouncerNames);

        if (egoNamesReference == null ||
            referenceScenarioModelCodes == null || referenceAnnouncerNames == null)
            throw new InvalidOperationException(
                "Некоторые из обязательных файлов перевода отсутствуют или были испорчены!");

        EgoNamesReference = egoNamesReference;
        ScenarioModelCodesReference = referenceScenarioModelCodes;
        AnnouncerNamesReference = referenceAnnouncerNames;

        #endregion

        _isLocalizationValid = true;
        EnsureInitNotification();
    }

    public void TryInitializeGameFiles()
    {
        var oldLimbusPath = PathToGameRoot;
        var originalLimbusPath = _persistentDataManager.Settings.PathToLimbus;

        try
        {
            var validatedGamePath = PersistentDataManager.ValidateLimbusPath(originalLimbusPath);
            if (validatedGamePath == null)
                throw new InvalidOperationException("Path to game is invalid!");

            if (validatedGamePath != originalLimbusPath)
                _persistentDataManager.Settings.PathToLimbus = validatedGamePath;

            PathToGameRoot = validatedGamePath;
            PathToReferenceLocalization = Path.Combine(validatedGamePath, ReferenceLangFolder);

            PathToFileMap = Path.Combine(PathToGameRoot, RemoteLocalizeFileListPath);
            UpdateSavedFileMap();

            _areGameFilesValid = true;
            EnsureInitNotification();
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrEmpty(oldLimbusPath))
            {
                _ = App.Current
                    .HandleNonFatalExceptionAsync(
                        ex); // message is only when old path exists cuz init window ensures there would be working path when first launching
                _persistentDataManager.Settings.PathToLimbus = oldLimbusPath;
                // revert old game path
            }
        }
    }

    // this is called from game path validator, so i don't need to make additional guards here ig?
    private void UpdateSavedFileMap()
    {
        var json = File.ReadAllText(PathToFileMap);
        var parsed = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
        DeveloperFileTypeMap.Clear();

        foreach (var (type, fileNames) in parsed!)
        foreach (var fileName in fileNames)
            DeveloperFileTypeMap.TryAdd(fileName, type);
    }

    private void EnsureInitNotification()
    {
        if (!IsValid)
            return;

        Log.Debug("Both paths were successfully validated! Invoking init event!");
        OnInitializedSuccessfully?.Invoke(); // moved this here so it'd fire only where everything is actually set up
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
        if (targetType == null || targetType == typeof(UnknownFile))
            deserialized = new UnknownFile();
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

            if (obj is UnknownFile)
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