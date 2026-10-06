using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData.Binding;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Utilities.Converters;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.Views;
using RainbusToolbox.Views.Translation;

namespace RainbusToolbox.ViewModels;

public partial class TranslationTabViewModel : ObservableObject
{
    private readonly ConfigProvider _configProvider;

    private readonly DiscordRPCService _discordRpcService;


    private readonly Dictionary<Type, Type> _editorMap = new()
    {
        { typeof(StoryDataFile), typeof(StoryTranslationEditor) },
        { typeof(EgoGiftsLocalizationFile), typeof(EGOGiftTranslationEditor) },
        { typeof(SkillLocalizationFile), typeof(SkillsEgoTranslationEditor) },
        { typeof(NormalBattleHintLocalizationFile), typeof(BattleHintsTranslationEditor) },
        { typeof(PanicInfoLocalizationFile), typeof(PanicTranslationEditor) },
        { typeof(PassiveLocalizationFile), typeof(PassiveTranslationEditor) },
        { typeof(AnnouncerVoiceLocalizationFile), typeof(BattleAnnouncerTranslationEditor) },
        { typeof(KeywordLocalizationFile), typeof(KeywordTranslationEditor) },
        { typeof(PersonalityVoiceLocalizationFile), typeof(PersonalityVoiceTranslationEditor) },
        { typeof(EgoVoiceLocalizationFile), typeof(EGOVoiceTranslationEditor) },
        { typeof(AbnormalityGuideContentLocalizationFile), typeof(AbnormalityGuideTranslationEditor) },
        { typeof(UnidentifiedFile), typeof(GenericTranslationEditor) },
        { typeof(UiLocalizationFile), typeof(UiElementTranslationEditor) }
    };


    private readonly RepositoryManager _repositoryManager;

    [ObservableProperty]
    private IFileEditor? _currentEditor;

    [ObservableProperty]
    private string _fileName = "Не выбран";

    private ObservableCollection<FileShortcut> _fileShortcuts = [];

    [ObservableProperty]
    private string _fileType = "";

    [ObservableProperty]
    private ObservableCollection<ShortcutTypeGroup> _groupedShortcuts = [];

    [ObservableProperty]
    private bool _isFileLoaded;

    public TranslationTabViewModel(RepositoryManager repositoryManager, ConfigProvider configProvider,
        DiscordRPCService discordRpcService)
    {
        _discordRpcService = discordRpcService;
        _configProvider = configProvider;
        _repositoryManager = repositoryManager;

        _repositoryManager.OnInitializedSuccessfully += InitShortcuts;

        // first run won't see event so run it by hand
        if (_repositoryManager.IsValid)
            InitShortcuts();
    }

    public ObservableCollection<FileShortcut> FileShortcuts
    {
        get => _fileShortcuts;
        private set
        {
            SetProperty(ref _fileShortcuts, value);
            GroupShortcuts();
        }
    }

    [RelayCommand]
    public async Task SelectFile()
    {
        var top =
            Application.Current!.ApplicationLifetime is
                IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        if (top == null) return;

        var storage = top.StorageProvider;

        var fileTypes = new[]
        {
            new FilePickerFileType("Файлы перевода")
            {
                Patterns = ["*.json"]
            },
            FilePickerFileTypes.All
        };

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выбери файлик пупсик",
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });

        if (files.Count == 0) return;

        var file = files[0].Path.LocalPath;


        LoadFile(file);
    }


    [RelayCommand]
    public void LoadFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var fileName = Path.GetFileName(filePath);
        //force keyword file is buff is chosen (Bufs to battle keywords)

        if (fileName.StartsWith("Bufs"))
        {
            var name = fileName.Replace("Bufs", "BattleKeywords");
            filePath = Path.Combine(Path.GetDirectoryName(filePath) ?? string.Empty, name);
        }

        if (!File.Exists(filePath))
        {
            _ = App.Current.HandleNonFatalExceptionAsync(new FileNotFoundException(filePath, fileName));
            return;
        }

        var detectedType = FileToObjectCaster.GetType(filePath, _repositoryManager.DeveloperFileTypeMap);

        var editorType = detectedType != null && _editorMap.TryGetValue(detectedType, out var value)
            ? value
            : typeof(GenericTranslationEditor);

        CurrentEditor = (IFileEditor)Activator.CreateInstance(editorType)!;

        FileName = Path.GetFileName(filePath);
        FileType = detectedType?.Name ?? "Unknown";
        IsFileLoaded = true;

        var file = _repositoryManager.GetObjectFromPath(filePath);
        var refFile = _repositoryManager.GetReference(file);

        if (file == null || refFile == null)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(new FileNotFoundException(
                $"Референс для {filePath} не удалось найти в игре. Редактор не сможет открыться!", fileName));
            return;
        }

        CurrentEditor.SetFileToEdit(file);
        CurrentEditor.SetReferenceFile(refFile);

        _discordRpcService.SetState($"Делает перевоз файла {FileName} ({file.GetSanityName()})");
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditorAndClose()
    {
        CurrentEditor?.AskEditorToSave(_repositoryManager);
        FileName = "Не выбран";
        FileType = "";
        CurrentEditor = null;
        IsFileLoaded = false;
        _discordRpcService.SetState("Готовится делать перевоз");
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditor()
    {
        CurrentEditor?.AskEditorToSave(_repositoryManager);
    }

    private void InitShortcuts()
    {
        //TODO: I made a mistake by moving log messages to lang resource, as logs need only english anyway
        // i need to check other files for those too, later

        _fileShortcuts = [];

        Log.Debug("Getting root");
        var root = _repositoryManager.PathToLocalization;
        Log.Debug("Repository root: {Root}", root);

        // "hard" shortcuts init here
        var fileShortcuts = _configProvider.GetYamlConfig<ObservableCollection<FileShortcut>>("shortcut-files");
        // shortcuts are currently with no fullpath, so i add it:
        foreach (var fileShortcut in fileShortcuts)
            fileShortcut.FullPath = Path.Combine(root, fileShortcut.PathRelativeToRoot);
        fileShortcuts = new ObservableCollectionExtended<FileShortcut>(fileShortcuts.OrderBy(f => f.Alias));

        // categories init here
        var categories = _configProvider.GetYamlConfig<List<FileShortcutCategory>>("shortcut-categories");

        foreach (var category in categories)
        {
            var searchPath = Path.Combine(root, category.Subfolder);
            if (!Directory.Exists(searchPath)) continue;

            var files = Directory.GetFiles(searchPath, category.Pattern);
            foreach (var file in files)
                fileShortcuts.Add(new FileShortcut
                {
                    Alias = Path.GetFileNameWithoutExtension(file),
                    FullPath = file,
                    Desc = "-",
                    Group = category.Group,
                    Type = category.Type
                });
        }

        foreach (var shortcut in fileShortcuts)
        {
            shortcut.DoesExist = File.Exists(shortcut.FullPath);
            shortcut.OpenCommand = LoadFileCommand;
        }

        Log.Debug("Created {Count} shortcuts", fileShortcuts.Count);

        FileShortcuts = fileShortcuts;
    }

    private void GroupShortcuts()
    {
        var groupedShortcuts =
            _fileShortcuts
                .GroupBy(s => string.IsNullOrWhiteSpace(s.Type) ? "Разное" : s.Type)
                .Select(typeGroup => new ShortcutTypeGroup
                {
                    Name = typeGroup.Key,
                    Groups = typeGroup
                        .GroupBy(s => string.IsNullOrWhiteSpace(s.Group) ? "Общее" : s.Group)
                        .Select(group => new ShortcutFolderGroup
                        {
                            Name = group.Key,
                            Shortcuts = group.OrderBy(s => s.Alias).ToList()
                        })
                        .OrderBy(g => g.Name)
                        .ToList()
                })
                .OrderBy(t => t.Name)
                .ToList();

        GroupedShortcuts = new ObservableCollection<ShortcutTypeGroup>(groupedShortcuts);
    }


    #region Events

    public void OnTabOpened()
    {
        _discordRpcService.SetState("Готовится делать перевоз");
    }

    #endregion
}