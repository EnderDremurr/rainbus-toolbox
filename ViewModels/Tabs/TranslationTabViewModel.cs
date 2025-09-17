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
    private string _fileName = "No file selected";

    [ObservableProperty]
    private IFileEditor? _currentEditor;

    private readonly Dictionary<Type, IFileEditor> _editorMap;
    private readonly IFileEditor _genericEditor;
    
    private RepositoryManager _repositoryManager;

    public TranslationTabViewModel()
    {
        _editorMap = new Dictionary<Type, IFileEditor>
        {
            { typeof(StoryDataFile), new StoryTranslationEditor() },
            { typeof(EGOGiftFile), new EGOGiftTranslationEditor() },
            { typeof(SkillsFile), new SkillsTranslationEditor() },
            { typeof(PanicInfoFile), new PanicTranslationEditor() }
        };
        
        _repositoryManager = App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager;

        _genericEditor = new GenericTranslationEditor();
    }

    [RelayCommand]
    public async void SelectFile()
    {
        var top = Avalonia.Application.Current.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null;
        if (top == null) return;

        var storage = top.StorageProvider;
        if (storage == null) return;

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
        var refFile = _repositoryManager.GetReference(file);
        CurrentEditor.SetFileToEdit(file);
        CurrentEditor.SetReferenceFile(refFile);
        
    }
}
