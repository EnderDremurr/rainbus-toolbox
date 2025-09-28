using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.Views;
using RainbusToolbox.Views.Translation;
using EGOGiftFile = RainbusToolbox.Utilities.Data.EGOGiftFile;
using SkillsFile = RainbusToolbox.Utilities.Data.SkillsFile;

namespace RainbusToolbox.ViewModels;

public partial class TranslationTabViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isFileLoaded;

    [ObservableProperty]
    private string _fileType = "";

    [ObservableProperty]
    private string _fileName = "Не выбран";

    [ObservableProperty]
    private IFileEditor? _currentEditor;

    public TranslationTabViewModel()
    {
        _ = InitShortcuts();
    }

    private readonly Dictionary<Type, Type> _editorMap = new()
    {
        { typeof(StoryDataFile), typeof(StoryTranslationEditor) },
        { typeof(EGOGiftFile), typeof(EGOGiftTranslationEditor) },
        { typeof(SkillsEgoFile), typeof(SkillsEgoTranslationEditor) },
        { typeof(SkillsFile), typeof(SkillsTranslationEditor) },
        { typeof(BattleHintsFile), typeof(BattleHintsTranslationEditor) },
        { typeof(PanicInfoFile), typeof(PanicTranslationEditor) },
        { typeof(PassivesFile), typeof(PassiveTranslationEditor) },
        { typeof(BattleAnnouncerFile), typeof(BattleAnnouncerTranslationEditor) },
        { typeof(BuffsFile), typeof(BuffTranslationEditor) },
        { typeof(PersonalityVoiceFile), typeof(PersonalityVoiceTranslationEditor) },
        { typeof(EGOVoiceFile), typeof(EGOVoiceTranslationEditor) },
        { typeof(AbnormalityGuideFile), typeof(AbnormalityGuideTranslationEditor) },
        { typeof(UnidentifiedFile), typeof(GenericTranslationEditor) }
    };

    private RepositoryManager _repositoryManager =
        (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    [RelayCommand]
    public async void SelectFile()
    {
        var top =
            Avalonia.Application.Current!.ApplicationLifetime is
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        if (top == null) return;

        var storage = top.StorageProvider;

        var fileTypes = new[]
        {
            new FilePickerFileType("Translation Files")
            {
                Patterns = ["*.json"]
            },
            FilePickerFileTypes.All
        };

        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Translation File",
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });

        if (files.Count == 0) return;

        var file = files[0];
        LoadFile(file.Path.LocalPath);
    }

    [RelayCommand]
    public void LoadFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var detectedType = FileToObjectCaster.GetType(filePath);

        var editorType = detectedType != null && _editorMap.TryGetValue(detectedType, out var value)
            ? value
            : typeof(GenericTranslationEditor);

        CurrentEditor = (IFileEditor)Activator.CreateInstance(editorType)!;

        FileName = Path.GetFileName(filePath);
        FileType = detectedType?.Name ?? "Unknown";
        IsFileLoaded = true;


        var file = _repositoryManager.GetObjectFromPath(filePath);
        var refFile = _repositoryManager.GetReference(file);

        CurrentEditor.SetFileToEdit(file);
        CurrentEditor.SetReferenceFile(refFile!);
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditor()
    {
        CurrentEditor?.AskEditorToSave(_repositoryManager);
        FileName = "Не выбран";
        FileType = "";
        CurrentEditor = null;
        IsFileLoaded = false;
        FileShortcuts = FileShortcuts;
    }

    private ObservableCollection<FileShortcut>? _fileShortcuts;

    public ObservableCollection<FileShortcut> FileShortcuts
    {
        get => _fileShortcuts;
        private set => SetProperty(ref _fileShortcuts, value);
    }


    private async Task InitShortcuts()
    {
        Console.WriteLine("Getting root");
        var timeout = TimeSpan.FromSeconds(10); // 10 second timeout
        var start = DateTime.Now;
    
        while (string.IsNullOrEmpty(_repositoryManager.PathToLocalization))
        {
            if (DateTime.Now - start > timeout)
            {
                Console.WriteLine("Timeout waiting for repository root");
                _fileShortcuts = new ObservableCollection<FileShortcut>();
                return;
            }
            Console.WriteLine("Didn't receive root for 0.1 ms");
            await Task.Delay(100);
        }
        var root = _repositoryManager.PathToLocalization;
        Console.WriteLine($"Repository root: {root}");
        
        var fileShortcuts = new ObservableCollection<FileShortcut>
        {
            new() { Alias = "Баттл хинты (загрузка)", FullPath = Path.Combine(root, $"BattleHint.json"), Desc = "-" },
            new() { Alias = "Баттл хинты (обычная битва)", FullPath = Path.Combine(root, $"BattleHint_NormalBattle.json"), Desc = "-" },
            new() { Alias = "Баттл хинты (битва с аномалией)", FullPath = Path.Combine(root, $"BattleHint_AbnorBattle.json"), Desc = "-" },
            new() { Alias = "Старые ЭГО", FullPath = Path.Combine(root, $"Skills_Ego.json"), Desc = "эго, что были добавлены в игру давно, все грешники в одном файле" },
            
            new() { Alias = "Эго И Сана", FullPath = Path.Combine(root, $"Skills_Ego_Personality-01.json"), Desc = "-" },
            new() { Alias = "Эго Фауст", FullPath = Path.Combine(root, $"Skills_Ego_Personality-02.json"), Desc = "-" },
            new() { Alias = "Эго Дон", FullPath = Path.Combine(root, $"Skills_Ego_Personality-03.json"), Desc = "-" },
            new() { Alias = "Эго Решу", FullPath = Path.Combine(root, $"Skills_Ego_Personality-04.json"), Desc = "-" },
            new() { Alias = "Эго Мерсо", FullPath = Path.Combine(root, $"Skills_Ego_Personality-05.json"), Desc = "-" },
            new() { Alias = "Эго Хонлу", FullPath = Path.Combine(root, $"Skills_Ego_Personality-06.json"), Desc = "-" },
            new() { Alias = "Эго Хитклифа", FullPath = Path.Combine(root, $"Skills_Ego_Personality-07.json"), Desc = "-" },
            new() { Alias = "Эго Ишмы", FullPath = Path.Combine(root, $"Skills_Ego_Personality-08.json"), Desc = "-" },
            new() { Alias = "Эго Роди", FullPath = Path.Combine(root, $"Skills_Ego_Personality-09.json"), Desc = "-" },
            new() { Alias = "Эго Синклера", FullPath = Path.Combine(root, $"Skills_Ego_Personality-10.json"), Desc = "-" },
            new() { Alias = "Эго Отис", FullPath = Path.Combine(root, $"Skills_Ego_Personality-11.json"), Desc = "-" },
            new() { Alias = "Эго Грегора", FullPath = Path.Combine(root, $"Skills_Ego_Personality-12.json"), Desc = "-" },
            
            new() { Alias = "Скиллы айдишек И Сана", FullPath = Path.Combine(root, $"Skills_personality-01.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Фауст", FullPath = Path.Combine(root, $"Skills_personality-02.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Дон", FullPath = Path.Combine(root, $"Skills_personality-03.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Решу", FullPath = Path.Combine(root, $"Skills_personality-04.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Мерсо", FullPath = Path.Combine(root, $"Skills_personality-05.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Хонлу", FullPath = Path.Combine(root, $"Skills_personality-06.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Хитклифа", FullPath = Path.Combine(root, $"Skills_personality-07.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Ишмы", FullPath = Path.Combine(root, $"Skills_personality-08.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Роди", FullPath = Path.Combine(root, $"Skills_personality-09.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Синклера", FullPath = Path.Combine(root, $"Skills_personality-10.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Отис", FullPath = Path.Combine(root, $"Skills_personality-11.json"), Desc = "-" },
            new() { Alias = "Скиллы айдишек Грегора", FullPath = Path.Combine(root, $"Skills_personality-12.json"), Desc = "-" },
        };

        
        foreach (var shortcut in fileShortcuts)
        {
            shortcut.DoesExist = true/*File.Exists(shortcut.Path)*/;
            shortcut.OpenCommand = OpenShortcutFileCommand;
        }
        Console.WriteLine($"Created {fileShortcuts.Count} shortcuts");

        _fileShortcuts = fileShortcuts;
        OnPropertyChanged(nameof(FileShortcuts));
    }

    [RelayCommand]
    public void OpenShortcutFile(string filePath)
    {
        Console.WriteLine($"OpenShortcutFile called with: {filePath}"); // Add this
    
        if (!string.IsNullOrEmpty(filePath))
        {
            Console.WriteLine("Calling LoadFile"); // Add this
            LoadFile(filePath);
        }
        else
        {
            Console.WriteLine("FilePath is null or empty"); // Add this
        }
    }
    
    // Add this temporarily to debug
    public string DebugCommands => string.Join(", ", this.GetType().GetProperties().Where(p => p.PropertyType.IsAssignableTo(typeof(ICommand))).Select(p => p.Name));
}