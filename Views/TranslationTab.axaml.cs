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
    public partial class TranslationTab : UserControl, INotifyPropertyChanged
    {
        private StoryDataFile? _storyDataFile;
        private StoryDataFile? _referenceDataFile;
        private int _currentIndex = 0;
        private string _filePath = string.Empty;
        private string _referenceFilePath = string.Empty;

        private PersistentDataManager _dataManager;

        public TranslationTab()
        {
            InitializeComponent();
            DataContext = this;
            if (Application.Current is App app)
            {
                _dataManager = (PersistentDataManager)app.ServiceProvider.GetService(typeof(PersistentDataManager));
            }
        }

        #region Properties

        public bool IsStoryLoaded => _storyDataFile?.DataList?.Any() == true;

        // Editable
        public StoryDataItem? CurrentItem
        {
            get
            {
                if (!IsStoryLoaded) return null;
                return _storyDataFile!.DataList[_currentIndex];
            }
        }

        // Reference
        public StoryDataItem? ReferenceItem
        {
            get
            {
                if (_referenceDataFile?.DataList?.Count > _currentIndex)
                {
                    return _referenceDataFile.DataList[_currentIndex];
                }
                return null;
            }
        }

        public bool CanGoPrevious => IsStoryLoaded && _currentIndex > 0;
        public bool CanGoNext => IsStoryLoaded && _currentIndex < _storyDataFile!.DataList.Count - 1;
        public string NavigationText => IsStoryLoaded ? $"{_currentIndex + 1} of {_storyDataFile!.DataList.Count}" : string.Empty;

        #endregion

        #region Event Handlers

        private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Story Data File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON Files"){ Patterns = new[] { "*.json" } },
                    new FilePickerFileType("All Files"){ Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
                await LoadStoryFile(files[0].Path.LocalPath);
        }

        private void OnPreviousClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoPrevious)
            {
                SaveCurrentItem();
                _currentIndex--;
                UpdateCurrentItemProperties();
            }
        }

        private void OnNextClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoNext)
            {
                SaveCurrentItem();
                _currentIndex++;
                UpdateCurrentItemProperties();
            }
        }

        #endregion

        #region Methods

        private async Task LoadStoryFile(string filePath)
        {
            try
            {
                _filePath = filePath;
                var json = await File.ReadAllTextAsync(filePath);
                _storyDataFile = JsonConvert.DeserializeObject<StoryDataFile>(json);
                _currentIndex = 0;

                if (_storyDataFile?.DataList?.Any() != true)
                    throw new InvalidOperationException("No story data found in the file or invalid format.");

                await LoadReferenceFile(filePath);

                if (this.FindControl<TextBlock>("FilePathText") is TextBlock filePathText)
                {
                    var fileName = Path.GetFileName(filePath);
                    var referenceStatus = _referenceDataFile != null ? " (Reference loaded)" : " (No reference)";
                    filePathText.Text = fileName + referenceStatus;
                }

                UpdateAllProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading story file: {ex.Message}");
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
                    _referenceDataFile = JsonConvert.DeserializeObject<StoryDataFile>(json);
                }
                else _referenceDataFile = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reference file: {ex.Message}");
                _referenceDataFile = null;
            }
        }

        private string GetReferenceFile(string pathToCurrentFile)
        {
            var filename = Path.GetFileName(pathToCurrentFile);
            var newPath = Path.Join(_dataManager.Settings.PathToLimbus,
                "LimbusCompany_Data/Assets/Resources_moved/Localize/en/StoryData",
                "EN_" + filename);
            Console.WriteLine(newPath);
            return newPath;
        }

        private void SaveCurrentItem()
        {
            if (!IsStoryLoaded || CurrentItem == null) return;
            SaveToFile();
        }

        private async void SaveToFile()
        {
            if (_storyDataFile == null || string.IsNullOrEmpty(_filePath)) return;
            try
            {
                var json = JsonConvert.SerializeObject(_storyDataFile, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        private void UpdateCurrentItemProperties()
        {
            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(ReferenceItem));
            OnPropertyChanged(nameof(CanGoPrevious));
            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(NavigationText));
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsStoryLoaded));
            OnPropertyChanged(nameof(CurrentItem));
            OnPropertyChanged(nameof(ReferenceItem));
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

    public class StoryDataFile
    {
        [JsonProperty("dataList")]
        public List<StoryDataItem> DataList { get; set; } = new();
    }

    public class StoryDataItem : INotifyPropertyChanged
    {
        private int _id;
        private string? _model;
        private string? _teller;
        private string? _title;
        private string? _place;
        private string _content = string.Empty;

        [JsonProperty("id")]
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        [JsonProperty("model")]
        public string? Model { get => _model; set { _model = value; OnPropertyChanged(); } }

        [JsonProperty("teller")]
        public string? Teller { get => _teller; set { _teller = value; OnPropertyChanged(); } }

        [JsonProperty("title")]
        public string? Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        [JsonProperty("place")]
        public string? Place { get => _place; set { _place = value; OnPropertyChanged(); } }

        [JsonProperty("content")]
        public string Content { get => _content; set { _content = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}