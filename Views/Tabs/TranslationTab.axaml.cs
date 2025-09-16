using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Views
{
    public partial class TranslationTab : UserControl, INotifyPropertyChanged
    {
        // Type to editor mapping - maps your data file types to their corresponding editors
        private readonly Dictionary<Type, (string DisplayName, Func<UserControl> CreateEditor)> _editorMap = 
            new Dictionary<Type, (string, Func<UserControl>)>
        {
            { typeof(SkillsFile), ("Skills", () => new SkillsTranslationEditor()) },
            { typeof(BattleHintsFile), ("Battle Hints", () => new BattleHintsTranslationEditor()) }
            // Add more mappings as you add new file types and editors
        };
        private bool _isFileLoaded;
        private string _fileType = string.Empty;
        private UserControl? _currentEditor;

        public TranslationTab()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool IsFileLoaded
        {
            get => _isFileLoaded;
            set
            {
                if (_isFileLoaded != value)
                {
                    _isFileLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FileType
        {
            get => _fileType;
            set
            {
                if (_fileType != value)
                {
                    _fileType = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserControl? CurrentEditor
        {
            get => _currentEditor;
            set
            {
                if (_currentEditor != value)
                {
                    _currentEditor = value;
                    OnPropertyChanged();
                }
            }
        }

        private async void OnSelectFileClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var storageProvider = TopLevel.GetTopLevel(this)?.StorageProvider;
                if (storageProvider == null) return;

                var fileTypes = new[]
                {
                    new FilePickerFileType("Translation Files")
                    {
                        Patterns = new[] { "*.json" },
                        AppleUniformTypeIdentifiers = new[] { "public.json" },
                        MimeTypes = new[] { "application/json" }
                    },
                    FilePickerFileTypes.All
                };

                var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = "Select Translation File",
                    AllowMultiple = false,
                    FileTypeFilter = fileTypes
                });

                if (files.Count > 0)
                {
                    var file = files[0];
                    var fileName = file.Name;
                    var filePath = file.Path.LocalPath;

                    // Update file path display
                    var filePathText = this.FindControl<TextBlock>("FilePathText");
                    if (filePathText != null)
                    {
                        filePathText.Text = fileName;
                    }

                    // Determine file type using FileToObjectCaster
                    Type? detectedType = FileToObjectCaster.GetType(filePath);
                    if (detectedType != null && _editorMap.ContainsKey(detectedType))
                    {
                        FileType = _editorMap[detectedType].DisplayName;
                        await LoadEditor(detectedType, filePath);
                        IsFileLoaded = true;
                    }
                    else
                    {
                        // Use default case for unknown file types
                        FileType = "General";
                        await LoadDefaultEditor(filePath);
                        IsFileLoaded = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error selecting file: {ex.Message}");
                // You might want to show an error dialog here
            }
        }

        private async Task LoadEditor(Type fileType, string filePath)
        {
            try
            {
                if (!_editorMap.ContainsKey(fileType))
                {
                    Console.WriteLine($"No editor mapping found for type: {fileType.Name}");
                    await LoadDefaultEditor(filePath);
                    return;
                }

                var (displayName, createEditor) = _editorMap[fileType];
                var editor = createEditor();

                // Initialize the editor with the file
                await InitializeEditor(editor, fileType, filePath);

                CurrentEditor = editor;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading editor for {fileType.Name}: {ex.Message}");
                CurrentEditor = null;
                IsFileLoaded = false;
            }
        }

        private async Task LoadDefaultEditor(string filePath)
        {
            try
            {
                // Create a default/generic editor for unknown file types
                // You can create a generic text editor or use one of your existing editors
                var editor = new BuffTranslationEditor(); // Or create a GenericTranslationEditor
                await InitializeEditor(editor, null, filePath);
                CurrentEditor = editor;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading default editor: {ex.Message}");
                CurrentEditor = null;
                IsFileLoaded = false;
            }
        }

        private async Task InitializeEditor(UserControl editor, Type? fileType, string filePath)
        {
            // Generic initialization logic that works with any editor
            // You'll need to implement this based on how your editors are structured
            
            switch (editor)
            {
                case SkillsTranslationEditor skillsEditor:
                    await InitializeSkillsEditor(skillsEditor, filePath);
                    break;
                
                case BattleHintsTranslationEditor battleHintsEditor:
                    await InitializeBattleHintsEditor(battleHintsEditor, filePath);
                    break;
                
                case BuffTranslationEditor buffEditor:
                    await InitializeBuffEditor(buffEditor, filePath);
                    break;
                
                case EGOGiftTranslationEditor egoGiftEditor:
                    await InitializeEGOGiftEditor(egoGiftEditor, filePath);
                    break;
                
                case PanicTranslationEditor panicEditor:
                    await InitializePanicEditor(panicEditor, filePath);
                    break;
                
                default:
                    Console.WriteLine($"No initialization method found for editor type: {editor.GetType().Name}");
                    break;
            }
        }

        // Editor initialization methods - implement these based on your existing editor initialization logic
        
        private async Task InitializeSkillsEditor(SkillsTranslationEditor editor, string filePath)
        {
            try
            {
                // Option 1: Call the private method using reflection
                var method = typeof(SkillsTranslationEditor).GetMethod("LoadSkillsFile", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    await (Task)method.Invoke(editor, new object[] { filePath });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing skills editor: {ex.Message}");
            }
        }

        private async Task InitializeBattleHintsEditor(BattleHintsTranslationEditor editor, string filePath)
        {
            // TODO: Implement battle hints editor initialization
            // Example:
            // await editor.LoadFile(filePath);
        }

        private async Task InitializeBuffEditor(BuffTranslationEditor? editor, string filePath)
        {
            if (editor == null) return;
            
            // TODO: Implement buff editor initialization
            // This should load the file data into the editor
            // You might need to call methods on your editor or set properties
            // Example:
            // await editor.LoadFile(filePath);
        }

        private async Task InitializeEGOGiftEditor(EGOGiftTranslationEditor? editor, string filePath)
        {
            if (editor == null) return;
            
            // TODO: Implement EGO Gift editor initialization
            // Example:
            // await editor.LoadFile(filePath);
        }

        private async Task InitializePanicEditor(PanicTranslationEditor? editor, string filePath)
        {
            if (editor == null) return;
            
            // TODO: Implement panic editor initialization
            // Example:
            // await editor.LoadFile(filePath);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}