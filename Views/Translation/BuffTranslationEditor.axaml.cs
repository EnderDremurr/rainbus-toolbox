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
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Views
{
    public partial class BuffTranslationEditor : UserControl, INotifyPropertyChanged, IFileEditor
    {
        private BuffsFile? _buffFile;
        private BuffsFile? _referenceBuffFile;
        private int _currentBuffIndex = 0;
        private string _filePath = string.Empty;
        private string _referenceFilePath = string.Empty;

        private PersistentDataManager _dataManager;

        public BuffTranslationEditor()
        {
            InitializeComponent();
            DataContext = this;
            if (Application.Current is App app)
            {
                _dataManager = (PersistentDataManager)app.ServiceProvider.GetService(typeof(PersistentDataManager));
            }
        }

        #region Properties

        public bool IsBuffLoaded => _buffFile?.DataList?.Any() == true;

        // Current editable buff
        public Buff? CurrentBuff
        {
            get
            {
                if (!IsBuffLoaded) return null;
                return _buffFile!.DataList[_currentBuffIndex];
            }
        }

        // Reference buff
        public Buff? ReferenceBuff
        {
            get
            {
                if (_referenceBuffFile?.DataList?.Count > _currentBuffIndex)
                {
                    return _referenceBuffFile.DataList[_currentBuffIndex];
                }
                return null;
            }
        }

        // Navigation properties
        public bool CanGoPrevious => IsBuffLoaded && _currentBuffIndex > 0;
        public bool CanGoNext => IsBuffLoaded && _currentBuffIndex < _buffFile!.DataList.Count - 1;
        
        public string NavigationText => IsBuffLoaded ? $"Buff {_currentBuffIndex + 1} of {_buffFile!.DataList.Count}" : string.Empty;

        #endregion

        #region Event Handlers

        private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Buff Data File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON Files"){ Patterns = new[] { "*.json" } },
                    new FilePickerFileType("All Files"){ Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
                await LoadBuffFile(files[0].Path.LocalPath);
        }

        private void OnPreviousBuffClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoPrevious)
            {
                SaveCurrentData();
                _currentBuffIndex--;
                UpdateAllProperties();
            }
        }

        private void OnNextBuffClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoNext)
            {
                SaveCurrentData();
                _currentBuffIndex++;
                UpdateAllProperties();
            }
        }

        #endregion

        #region Methods

        private async Task LoadBuffFile(string filePath)
        {
            try
            {
                _filePath = filePath;
                var json = await File.ReadAllTextAsync(filePath);
                _buffFile = JsonConvert.DeserializeObject<BuffsFile>(json);
                _currentBuffIndex = 0;

                if (_buffFile?.DataList?.Any() != true)
                    throw new InvalidOperationException("No buff data found in the file or invalid format.");

                await LoadReferenceFile(filePath);

                if (this.FindControl<TextBlock>("FilePathText") is TextBlock filePathText)
                {
                    var fileName = Path.GetFileName(filePath);
                    var referenceStatus = _referenceBuffFile != null ? " (Reference loaded)" : " (No reference)";
                    filePathText.Text = fileName + referenceStatus;
                }

                UpdateAllProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading buff file: {ex.Message}");
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
                    _referenceBuffFile = JsonConvert.DeserializeObject<BuffsFile>(json);
                }
                else _referenceBuffFile = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reference file: {ex.Message}");
                _referenceBuffFile = null;
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
            if (!IsBuffLoaded) return;
            SaveToFile();
        }

        private async void SaveToFile()
        {
            if (_buffFile == null || string.IsNullOrEmpty(_filePath)) return;
            try
            {
                var json = JsonConvert.SerializeObject(_buffFile, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsBuffLoaded));
            OnPropertyChanged(nameof(CurrentBuff));
            OnPropertyChanged(nameof(ReferenceBuff));
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

        public void SetFileToEdit(LocalizationFileBase file)
        {
            throw new NotImplementedException();
        }

        public void SetReferenceFile(LocalizationFileBase file)
        {
            throw new NotImplementedException();
        }
    }

    // Data model classes with INotifyPropertyChanged implementation
    public class BuffsFile
    {
        [JsonProperty("dataList")]
        public List<Buff> DataList { get; set; } = new();
    }

    public class Buff : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _desc = string.Empty;
        private string _summary = string.Empty;
        private string _undefined = string.Empty;

        [JsonProperty("id")]
        public string Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        [JsonProperty("name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        [JsonProperty("desc")]
        public string Desc { get => _desc; set { _desc = value; OnPropertyChanged(); } }

        [JsonProperty("summary")]
        public string Summary { get => _summary; set { _summary = value; OnPropertyChanged(); } }

        [JsonProperty("undefined")]
        public string Undefined { get => _undefined; set { _undefined = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}