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
    public partial class EGOGiftTranslationEditor : UserControl, INotifyPropertyChanged
    {
        private EGOGiftFile? _egoGiftFile;
        private EGOGiftFile? _referenceEGOGiftFile;
        private int _currentEGOGiftIndex = 0;
        private string _filePath = string.Empty;
        private string _referenceFilePath = string.Empty;

        private PersistentDataManager _dataManager;

        public EGOGiftTranslationEditor()
        {
            InitializeComponent();
            DataContext = this;
            if (Application.Current is App app)
            {
                _dataManager = (PersistentDataManager)app.ServiceProvider.GetService(typeof(PersistentDataManager));
            }
        }

        #region Properties

        public bool IsEGOGiftLoaded => _egoGiftFile?.DataList?.Any() == true;

        // Current editable EGO Gift
        public GenericIdNameDesc? CurrentEGOGift
        {
            get
            {
                if (!IsEGOGiftLoaded) return null;
                return _egoGiftFile!.DataList[_currentEGOGiftIndex];
            }
        }

        // Reference EGO Gift
        public GenericIdNameDesc? ReferenceEGOGift
        {
            get
            {
                if (_referenceEGOGiftFile?.DataList?.Count > _currentEGOGiftIndex)
                {
                    return _referenceEGOGiftFile.DataList[_currentEGOGiftIndex];
                }
                return null;
            }
        }

        // Navigation properties
        public bool CanGoPrevious => IsEGOGiftLoaded && _currentEGOGiftIndex > 0;
        public bool CanGoNext => IsEGOGiftLoaded && _currentEGOGiftIndex < _egoGiftFile!.DataList.Count - 1;
        
        public string NavigationText => IsEGOGiftLoaded ? $"Gift {_currentEGOGiftIndex + 1} of {_egoGiftFile!.DataList.Count}" : string.Empty;

        #endregion

        #region Event Handlers

        private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select EGO Gift Data File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON Files"){ Patterns = new[] { "*.json" } },
                    new FilePickerFileType("All Files"){ Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
                await LoadEGOGiftFile(files[0].Path.LocalPath);
        }

        private void OnPreviousEGOGiftClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoPrevious)
            {
                SaveCurrentData();
                _currentEGOGiftIndex--;
                UpdateAllProperties();
            }
        }

        private void OnNextEGOGiftClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoNext)
            {
                SaveCurrentData();
                _currentEGOGiftIndex++;
                UpdateAllProperties();
            }
        }

        #endregion

        #region Methods
        // In your ViewModel or code-behind
        public double CalculateNameFontSize(string text)
        {
            if (string.IsNullOrEmpty(text)) return 36;
    
            int length = text.Length;
            if (length <= 20) return 36;      // Normal size
            if (length <= 30) return 30;      // Slightly smaller
            if (length <= 40) return 24;      // Medium
            if (length <= 50) return 20;      // Small
            return 16;                        // Very small for long names
        }

        private async Task LoadEGOGiftFile(string filePath)
        {
            try
            {
                _filePath = filePath;
                var json = await File.ReadAllTextAsync(filePath);
                _egoGiftFile = JsonConvert.DeserializeObject<EGOGiftFile>(json);
                _currentEGOGiftIndex = 0;

                if (_egoGiftFile?.DataList?.Any() != true)
                    throw new InvalidOperationException("No EGO Gift data found in the file or invalid format.");

                await LoadReferenceFile(filePath);

                if (this.FindControl<TextBlock>("FilePathText") is TextBlock filePathText)
                {
                    var fileName = Path.GetFileName(filePath);
                    var referenceStatus = _referenceEGOGiftFile != null ? " (Reference loaded)" : " (No reference)";
                    filePathText.Text = fileName + referenceStatus;
                }

                UpdateAllProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EGO Gift file: {ex.Message}");
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
                    _referenceEGOGiftFile = JsonConvert.DeserializeObject<EGOGiftFile>(json);
                }
                else _referenceEGOGiftFile = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reference file: {ex.Message}");
                _referenceEGOGiftFile = null;
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
            if (!IsEGOGiftLoaded) return;
            SaveToFile();
        }

        private async void SaveToFile()
        {
            if (_egoGiftFile == null || string.IsNullOrEmpty(_filePath)) return;
            try
            {
                var json = JsonConvert.SerializeObject(_egoGiftFile, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsEGOGiftLoaded));
            OnPropertyChanged(nameof(CurrentEGOGift));
            OnPropertyChanged(nameof(ReferenceEGOGift));
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
    public class EGOGiftFile
    {
        [JsonProperty("dataList")]
        public List<GenericIdNameDesc> DataList { get; set; } = new();
    }

    public class GenericIdNameDesc : INotifyPropertyChanged
    {
        private int _id;
        private string _name = string.Empty;
        private string _desc = string.Empty;

        [JsonProperty("id")]
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        [JsonProperty("name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        [JsonProperty("desc")]
        public string Desc { get => _desc; set { _desc = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}