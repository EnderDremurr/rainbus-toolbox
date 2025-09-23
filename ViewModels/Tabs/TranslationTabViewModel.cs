using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    private readonly Dictionary<Type, IFileEditor> _editorMap = new()
    {
        { typeof(StoryDataFile), new StoryTranslationEditor() },
        { typeof(EGOGiftFile), new EGOGiftTranslationEditor() },
        {typeof(SkillsEgoFile), new SkillsEgoTranslationEditor()},
        { typeof(SkillsFile), new SkillsTranslationEditor() },
        {typeof(BattleHintsFile), new BattleHintsTranslationEditor()},
        { typeof(PanicInfoFile), new PanicTranslationEditor() },
        {typeof(PassivesFile), new PassiveTranslationEditor()},
        {typeof(BattleAnnouncerFile), new BattleAnnouncerTranslationEditor()},
        { typeof(BuffsFile), new BuffTranslationEditor() },
        {typeof(PersonalityVoiceFile), new PersonalityVoiceTranslationEditor()},
        {typeof(EGOVoiceFile), new EGOVoiceTranslationEditor()},
        {typeof(AbnormalityGuideFile), new AbnormalityGuideTranslationEditor()},
        {typeof(UnidentifiedFile), new GenericTranslationEditor()}
        
        
        
    };
    private readonly IFileEditor _genericEditor = new GenericTranslationEditor();
    
    private RepositoryManager _repositoryManager = (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    [RelayCommand]
    public async void SelectFile()
    {
        var top = Avalonia.Application.Current!.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
        if (top == null) return;

        var storage = top.StorageProvider;

        var fileTypes = new[]
        {
            new FilePickerFileType("Translation Files")
            {
                Patterns = new[] { "*.json" }
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

        CurrentEditor = detectedType != null && _editorMap.ContainsKey(detectedType)
            ? _editorMap[detectedType]
            : _genericEditor;

        FileName = Path.GetFileName(filePath);
        FileType = detectedType?.Name ?? "Unknown";
        IsFileLoaded = true;


        var file = _repositoryManager.GetObjectFromPath(filePath);
        var refFile = _repositoryManager.GetReference(file!);
        
        CurrentEditor.SetFileToEdit(file!);
        CurrentEditor.SetReferenceFile(refFile!);
        
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditor()
    {
        Console.WriteLine("=== SaveObjectFromCurrentEditor Debug Start ===");
        Console.WriteLine($"CurrentEditor: {CurrentEditor}");
        Console.WriteLine($"CurrentEditor type: {CurrentEditor?.GetType().Name}");
    
        if (CurrentEditor is GenericTranslationEditor ed)
        {
            Console.WriteLine("Taking GenericTranslationEditor path");
            ed.SaveUnknownFile();
            return;
        }
    
        Console.WriteLine($"CurrentEditor is UserControl: {CurrentEditor is UserControl}");
        if (CurrentEditor is UserControl editor && editor.DataContext is not null)
        {
            Console.WriteLine($"Editor DataContext type: {editor.DataContext.GetType().Name}");
        
            var vm = editor.DataContext;
            var prop = vm.GetType().GetProperty("EditableFile");
            Console.WriteLine($"EditableFile property found: {prop != null}");
        
            var obj = prop?.GetValue(vm);
            Console.WriteLine($"EditableFile value: {obj}");
            Console.WriteLine($"EditableFile type: {obj?.GetType().Name}");

            if (obj is LocalizationFileBase file)
            {
                Console.WriteLine("Object is LocalizationFileBase - calling SaveObjectToFile");
                _repositoryManager.SaveObjectToFile(file);
            }
            else
            {
                Console.WriteLine("Object is NOT LocalizationFileBase");
            }
        }
        else
        {
            Console.WriteLine("CurrentEditor is not UserControl or DataContext is null");
        }
    
        Console.WriteLine("=== SaveObjectFromCurrentEditor Debug End ===");
    }
}
