using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.Utilities.Converters;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.Views;
using RainbusToolbox.Views.Translation;

namespace RainbusToolbox.ViewModels;

public partial class TranslationTabViewModel : ObservableObject
{
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

    private readonly RepositoryManager _repositoryManager =
        (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    [ObservableProperty]
    private IFileEditor? _currentEditor;

    [ObservableProperty]
    private string _fileName = "Не выбран";

    private ObservableCollection<FileShortcut> _fileShortcuts;

    [ObservableProperty]
    private string _fileType = "";

    [ObservableProperty]
    private bool _isFileLoaded;

    public TranslationTabViewModel()
    {
        _ = InitShortcuts();
        _discordRpcService = (App.Current.ServiceProvider.GetService(typeof(DiscordRPCService)) as DiscordRPCService)!;
    }

    public ObservableCollection<FileShortcut> FileShortcuts
    {
        get => _fileShortcuts;
        private set => SetProperty(ref _fileShortcuts, value);
    }

    public IEnumerable<ShortcutTypeGroup> GroupedShortcuts =>
        _fileShortcuts?
            .GroupBy(s => string.IsNullOrWhiteSpace(s.Type) ? "Разное" : s.Type)
            .Select(typeGroup => new ShortcutTypeGroup
            {
                Name = typeGroup.Key,
                Groups = typeGroup
                    .GroupBy(s => string.IsNullOrWhiteSpace(s.Group) ? "Общее" : s.Group)
                    .Select(group => new ShortcutFolderGroup
                    {
                        Name = group.Key,
                        Shortcuts = group.OrderBy(s => s.Alias)
                    })
                    .OrderBy(g => g.Name)
            })
            .OrderBy(t => t.Name)
        ?? Enumerable.Empty<ShortcutTypeGroup>();

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

        CurrentEditor.SetFileToEdit(file!);
        CurrentEditor.SetReferenceFile(refFile!);

        _discordRpcService.SetState($"Делает перевоз файла {FileName} ({file!.GetSanityName()})");
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditorAndClose()
    {
        CurrentEditor?.AskEditorToSave(_repositoryManager);
        FileName = "Не выбран";
        FileType = "";
        CurrentEditor = null;
        IsFileLoaded = false;
        FileShortcuts = FileShortcuts;
        _discordRpcService.SetState("Готовится делать перевоз");
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditor()
    {
        CurrentEditor?.AskEditorToSave(_repositoryManager);
    }

    private async Task InitShortcuts()
    {
        Log.Debug(AppLang.TranslationTabViewModel_InitShortcuts_Getting_root);
        var timeout = TimeSpan.FromSeconds(10); // 10 second timeout
        var start = DateTime.Now;

        while (string.IsNullOrWhiteSpace(_repositoryManager.PathToLocalization))
        {
            if (DateTime.Now - start > timeout)
            {
                Log.Debug(AppLang.TranslationTabViewModel_InitShortcuts_Timeout_waiting_for_repository_root);
                _fileShortcuts = new ObservableCollection<FileShortcut>();
                return;
            }

            Log.Debug(AppLang.TranslationTabViewModel_InitShortcuts_Didn_t_receive_root_for_0_1_ms);
            await Task.Delay(100);
        }

        var root = _repositoryManager.PathToLocalization;
        Log.Debug(AppLang.TranslationTabViewModel_InitShortcuts_Repository_root___0_, root);

        var fileShortcuts = new ObservableCollection<FileShortcut>
        {
            new()
            {
                Alias = "Баттл хинты (загрузка)", FullPath = Path.Combine(root, "BattleHint.json"), Desc = "-",
                Group = "Интерфейс"
            },
            new()
            {
                Alias = "Баттл хинты (обычная битва)", FullPath = Path.Combine(root, "BattleHint_NormalBattle.json"),
                Desc = "-", Group = "Интерфейс"
            },
            new()
            {
                Alias = "Баттл хинты (битва с аномалией)", FullPath = Path.Combine(root, "BattleHint_AbnorBattle.json"),
                Desc = "-", Group = "Интерфейс"
            },
            new()
            {
                Alias = "Представления (Куриный шашлычок)", FullPath = Path.Combine(root, "IntroductionPreset.json"),
                Desc = "-", Group = "Интерфейс"
            },
            new()
            {
                Alias = "Кейворды скиллов", FullPath = Path.Combine(root, "SkillTag.json"), Desc = "-",
                Group = "Интерфейс"
            },
            new()
            {
                Alias = "Battle кейворды", FullPath = Path.Combine(root, "BattleKeywords.json"), Desc = "-",
                Group = "Интерфейс"
            },
            new()
            {
                Alias = "Battle UI Text", FullPath = Path.Combine(root, "BattleUIText.json"), Desc = "-",
                Group = "Интерфейс"
            },

            new()
            {
                Alias = "Старые ЭГО", FullPath = Path.Combine(root, "Skills_Ego.json"),
                Type = "Скиллы", Desc = "эго, что были добавлены в игру давно, все грешники в одном файле",
                Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго И Сана", FullPath = Path.Combine(root, "Skills_Ego_Personality-01.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Фауст", FullPath = Path.Combine(root, "Skills_Ego_Personality-02.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Дон", FullPath = Path.Combine(root, "Skills_Ego_Personality-03.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Решу", FullPath = Path.Combine(root, "Skills_Ego_Personality-04.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Мерсо", FullPath = Path.Combine(root, "Skills_Ego_Personality-05.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Хонлу", FullPath = Path.Combine(root, "Skills_Ego_Personality-06.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Хитклифа", FullPath = Path.Combine(root, "Skills_Ego_Personality-07.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Ишмы", FullPath = Path.Combine(root, "Skills_Ego_Personality-08.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Роди", FullPath = Path.Combine(root, "Skills_Ego_Personality-09.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Синклера", FullPath = Path.Combine(root, "Skills_Ego_Personality-10.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Отис", FullPath = Path.Combine(root, "Skills_Ego_Personality-11.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Эго Грегора", FullPath = Path.Combine(root, "Skills_Ego_Personality-12.json"), Desc = "-",
                Type = "Скиллы", Group = "ЭГО"
            },
            new()
            {
                Alias = "Пассивки ЭГО", Type = "Скиллы", FullPath = Path.Combine(root, "Passive_Ego.json"), Desc = "-",
                Group = "ЭГО"
            },

            new()
            {
                Alias = "Скиллы айдишек И Сана", FullPath = Path.Combine(root, "Skills_personality-01.json"),
                Type = "Скиллы", Desc = "-", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Фауст", FullPath = Path.Combine(root, "Skills_personality-02.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Дон", FullPath = Path.Combine(root, "Skills_personality-03.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Решу", FullPath = Path.Combine(root, "Skills_personality-04.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Мерсо", FullPath = Path.Combine(root, "Skills_personality-05.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Хонлу", FullPath = Path.Combine(root, "Skills_personality-06.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Хитклифа", FullPath = Path.Combine(root, "Skills_personality-07.json"),
                Type = "Скиллы", Desc = "-", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Ишмы", FullPath = Path.Combine(root, "Skills_personality-08.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Роди", FullPath = Path.Combine(root, "Skills_personality-09.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Синклера", FullPath = Path.Combine(root, "Skills_personality-10.json"),
                Type = "Скиллы", Desc = "-", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Отис", FullPath = Path.Combine(root, "Skills_personality-11.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Скиллы айдишек Грегора", FullPath = Path.Combine(root, "Skills_personality-12.json"),
                Type = "Скиллы", Desc = "-", Group = "Скиллы айдишек"
            },
            new()
            {
                Alias = "Пассивки скиллов", FullPath = Path.Combine(root, "Passives.json"), Desc = "-",
                Type = "Скиллы", Group = "Скиллы айдишек"
            }
        };

        var categories = new[]
        {
            new { Type = "Разное", Group = "Гифты", Pattern = "EGOgift_*.json", Subfolder = "" },
            new { Type = "Разное", Group = "Анонсеры", Pattern = "*.json", Subfolder = "BattleAnnouncerDlg" },
            new { Type = "Разное", Group = "Фразы айдишек", Pattern = "*.json", Subfolder = "PersonalityVoiceDlg" },
            new { Type = "Разное", Group = "Кейворды", Pattern = "BattleKeywords*.json", Subfolder = "" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 1", Pattern = "S1*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 2", Pattern = "S2*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 3", Pattern = "S3*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 4", Pattern = "S4*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 5", Pattern = "S5*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 6", Pattern = "S6*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 7", Pattern = "S7*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 8", Pattern = "S8*.json", Subfolder = "StoryData" },
            new { Type = "Сюжет канто", Group = "Сюжет канто 9", Pattern = "S9*.json", Subfolder = "StoryData" },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 3", Pattern = "E3*.json", Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 4", Pattern = "E4*.json", Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 5", Pattern = "E5*.json", Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 6", Pattern = "E6*.json", Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 7", Pattern = "E7*.json", Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 8", Pattern = "E8*.json", Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 8 (доп файлы)", Pattern = "ES*.json",
                Subfolder = "StoryData"
            },
            new
            {
                Type = "Сюжет интервало", Group = "Интервало канто 9", Pattern = "E9*.json", Subfolder = "StoryData"
            },


            new { Type = "Разное", Group = "Ачивки МД", Pattern = "UI_Mission*.json", Subfolder = "" }
        };

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
            fileShortcuts = new ObservableCollection<FileShortcut>(fileShortcuts.OrderBy(f => f.Alias));
        }

        foreach (var shortcut in fileShortcuts)
        {
            shortcut.DoesExist = File.Exists(shortcut.FullPath);
            shortcut.OpenCommand = OpenShortcutFileCommand;
        }

        Log.Debug(AppLang.TranslationTabViewModel_InitShortcuts_Created__0__shortcuts, fileShortcuts.Count);

        FileShortcuts = fileShortcuts;
        OnPropertyChanged(nameof(GroupedShortcuts));
    }

    [RelayCommand]
    public void OpenShortcutFile(string filePath)
    {
        Log.Debug(AppLang.TranslationTabViewModel_OpenShortcutFile_OpenShortcutFile_called_with___0_, filePath);

        if (!string.IsNullOrWhiteSpace(filePath))
        {
            Log.Debug(AppLang.TranslationTabViewModel_OpenShortcutFile_Calling_LoadFile);
            LoadFile(filePath);
        }
        else
        {
            Log.Debug(AppLang.TranslationTabViewModel_OpenShortcutFile_FilePath_is_null_or_empty);
        }
    }

    #region Events

    public void OnTabOpened()
    {
        _discordRpcService.SetState("Готовится делать перевоз");
    }

    #endregion

    #region Data types for shortcut dogshit

    public class ShortcutTypeGroup
    {
        public string Name { get; set; } = "";
        public IEnumerable<ShortcutFolderGroup> Groups { get; set; } = [];
    }

    public class ShortcutFolderGroup
    {
        public string Name { get; set; } = "";
        public IEnumerable<FileShortcut> Shortcuts { get; set; } = [];
    }

    #endregion
}