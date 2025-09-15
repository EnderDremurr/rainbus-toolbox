using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Views
{
    public partial class PanicTranslationEditor : UserControl, INotifyPropertyChanged
    {
        private PanicInfoFile? _panicFile;
        private PanicInfoFile? _referencePanicFile;
        private int _currentPanicIndex = 0;
        private string _filePath = string.Empty;
        private string _referenceFilePath = string.Empty;

        private PersistentDataManager _dataManager;

        public PanicTranslationEditor()
        {
            InitializeComponent();
            DataContext = this;
            if (Application.Current is App app)
            {
                _dataManager = (PersistentDataManager)app.ServiceProvider.GetService(typeof(PersistentDataManager));
            }
        }

        #region Properties

        public bool IsPanicLoaded => _panicFile?.DataList?.Any() == true;

        // Current editable panic
        public PanicInfo? CurrentPanic
        {
            get
            {
                if (!IsPanicLoaded) return null;
                return _panicFile!.DataList[_currentPanicIndex];
            }
        }

        // Reference panic
        public PanicInfo? ReferencePanic
        {
            get
            {
                if (_referencePanicFile?.DataList?.Count > _currentPanicIndex)
                {
                    return _referencePanicFile.DataList[_currentPanicIndex];
                }
                return null;
            }
        }

        // Navigation properties
        public bool CanGoPrevious => IsPanicLoaded && _currentPanicIndex > 0;
        public bool CanGoNext => IsPanicLoaded && _currentPanicIndex < _panicFile!.DataList.Count - 1;
        
        public string NavigationText => IsPanicLoaded ? $"Panic {_currentPanicIndex + 1} of {_panicFile!.DataList.Count}" : string.Empty;

        #endregion

        #region Event Handlers

        private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Panic Data File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON Files"){ Patterns = new[] { "*.json" } },
                    new FilePickerFileType("All Files"){ Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
                await LoadPanicFile(files[0].Path.LocalPath);
        }

        private void OnPreviousPanicClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoPrevious)
            {
                SaveCurrentData();
                _currentPanicIndex--;
                UpdateAllProperties();
            }
        }

        private void OnNextPanicClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoNext)
            {
                SaveCurrentData();
                _currentPanicIndex++;
                UpdateAllProperties();
            }
        }

        #endregion

        #region Methods

        private async Task LoadPanicFile(string filePath)
        {
            try
            {
                _filePath = filePath;
                var json = await File.ReadAllTextAsync(filePath);
                _panicFile = JsonConvert.DeserializeObject<PanicInfoFile>(json);
                _currentPanicIndex = 0;

                if (_panicFile?.DataList?.Any() != true)
                    throw new InvalidOperationException("No panic data found in the file or invalid format.");

                await LoadReferenceFile(filePath);

                if (this.FindControl<TextBlock>("FilePathText") is TextBlock filePathText)
                {
                    var fileName = Path.GetFileName(filePath);
                    var referenceStatus = _referencePanicFile != null ? " (Reference loaded)" : " (No reference)";
                    filePathText.Text = fileName + referenceStatus;
                }

                UpdateAllProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading panic file: {ex.Message}");
                if (this.FindControl<TextBlock>("FilePathText") is TextBlock filePathText)
                    filePathText.Text = $"Error loading file: {ex.Message}";
            }
        }

        private async Task LoadReferenceFile(string currentFilePath)
        {
            try
            {
                _referenceFilePath = GetReferenceFile(currentFilePath);
                if (!string.IsNullOrEmpty(_referenceFilePath) && File.Exists(_referenceFilePath))
                {
                    var json = await File.ReadAllTextAsync(_referenceFilePath);
                    _referencePanicFile = JsonConvert.DeserializeObject<PanicInfoFile>(json);
                }
                else _referencePanicFile = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reference file: {ex.Message}");
                _referencePanicFile = null;
            }
        }

        private string GetReferenceFile(string pathToCurrentFile)
        {
            var filename = Path.GetFileName(pathToCurrentFile);
            var newPath = Path.Join(_dataManager.Settings.PathToLimbus,
                "LimbusCompany_Data/Assets/Resources_moved/Localize/en",
                "EN_" + filename);
            Console.WriteLine(newPath);
            return newPath;
        }

        private void SaveCurrentData()
        {
            if (!IsPanicLoaded) return;
            SaveToFile();
        }

        private async void SaveToFile()
        {
            if (_panicFile == null || string.IsNullOrEmpty(_filePath)) return;
            try
            {
                var json = JsonConvert.SerializeObject(_panicFile, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsPanicLoaded));
            OnPropertyChanged(nameof(CurrentPanic));
            OnPropertyChanged(nameof(ReferencePanic));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(NavigationText));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }

    // Data model classes with INotifyPropertyChanged implementation
    public class PanicInfoFile
    {
        [JsonProperty("dataList")]
        public List<PanicInfo> DataList { get; set; } = new();
    }

    public class PanicInfo : INotifyPropertyChanged
    {
        private int _id;
        private string _panicName = string.Empty;
        private string _lowMoraleDescription = string.Empty;
        private string _panicDescription = string.Empty;

        [JsonProperty("id")]
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        [JsonProperty("panicName")]
        public string PanicName { get => _panicName; set { _panicName = value; OnPropertyChanged(); } }

        [JsonProperty("lowMoraleDescription")]
        public string LowMoraleDescription { get => _lowMoraleDescription; set { _lowMoraleDescription = value; OnPropertyChanged(); } }

        [JsonProperty("panicDescription")]
        public string PanicDescription { get => _panicDescription; set { _panicDescription = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}