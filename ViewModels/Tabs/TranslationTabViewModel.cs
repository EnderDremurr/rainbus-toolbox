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
using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.Services;
using RainbusToolbox.Services.ExternalServices;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Utilities;

namespace RainbusToolbox.ViewModels.Tabs;

public partial class TranslationTabViewModel : ObservableObject, IMainWindowTab
{
    private readonly ConfigProvider _configProvider;
    private readonly DialogService _dialogService;

    private readonly DiscordRPCService _discordRpcService;

    private readonly EditorFactory _editorFactory;


    private readonly LocalizationManager _localizationManager;

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

    public TranslationTabViewModel(LocalizationManager localizationManager, ConfigProvider configProvider,
        DiscordRPCService discordRpcService, EditorFactory editorFactory, DialogService dialogService)
    {
        _discordRpcService = discordRpcService;
        _configProvider = configProvider;
        _localizationManager = localizationManager;
        _editorFactory = editorFactory;
        _dialogService = dialogService;

        _localizationManager.OnInitializedSuccessfully += InitShortcuts;

        // first run won't see event so run it by hand
        if (_localizationManager.IsValid)
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


    #region Events

    public Task OnTabOpened()
    {
        _discordRpcService.SetState("Готовится делать перевоз");
        return Task.CompletedTask;
    }

    #endregion

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

        var file = await _dialogService.PickSingleFileAsync(new FilePickerOpenOptions
        {
            Title = "Выбери файлик пупсик",
            AllowMultiple = false,
            FileTypeFilter = fileTypes
        });

        if (file == null)
            return;


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

        try
        {
            var detectedType = FileToObjectCaster.GetType(filePath, _localizationManager.DeveloperFileTypeMap) ??
                               typeof(UnknownFile);
            var file = _localizationManager.GetObjectFromPath(filePath);

            if (file == null)
                throw new ArgumentException($"Файл {filePath} не удалось прочесть! Проверь файл руками.");

            var refFile = _localizationManager.GetReference(file);

            FileName = Path.GetFileName(filePath);
            FileType = detectedType.Name;

            var newEditor = _editorFactory.CreateFileEditor(detectedType);
            newEditor.SetFileToEdit(file);
            newEditor.SetReferenceFile(refFile);

            CurrentEditor = newEditor;
            IsFileLoaded = true;

            _discordRpcService.SetState($"Делает перевоз файла {FileName} ({file.GetSanityName()})");
        }
        catch (Exception e)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(e);
        }
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditorAndClose()
    {
        CurrentEditor?.AskEditorToSave(_localizationManager);
        FileName = "Не выбран";
        FileType = "";
        CurrentEditor = null;
        IsFileLoaded = false;
        _discordRpcService.SetState("Готовится делать перевоз");
    }

    [RelayCommand]
    public void SaveObjectFromCurrentEditor()
    {
        CurrentEditor?.AskEditorToSave(_localizationManager);
    }

    private void InitShortcuts()
    {
        Log.Debug("Starting shortcut initialization");

        _fileShortcuts = [];

        var root = _localizationManager.PathToLocalization;
        Log.Debug("Repository root: {Root}", root);

        // "hard" shortcuts init here
        var fileShortcuts = _configProvider.GetYamlConfig<ObservableCollection<FileShortcut>>("shortcut-files");
        // shortcuts are currently with no fullpath, so i add it:
        foreach (var fileShortcut in fileShortcuts)
            if (!string.IsNullOrEmpty(fileShortcut.PathRelativeToRoot))
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
}