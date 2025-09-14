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
    public partial class SkillsTranslationEditor : UserControl, INotifyPropertyChanged
    {
        private SkillsFile? _skillsFile;
        private SkillsFile? _referenceSkillsFile;
        private int _currentSkillIndex = 0;
        private int _currentLevelIndex = 0;
        private string _filePath = string.Empty;
        private string _referenceFilePath = string.Empty;

        private PersistentDataManager _dataManager;

        public SkillsTranslationEditor()
        {
            InitializeComponent();
            DataContext = this;
            if (Application.Current is App app)
            {
                _dataManager = (PersistentDataManager)app.ServiceProvider.GetService(typeof(PersistentDataManager));
            }
        }

        #region Properties

        public bool IsSkillsLoaded => _skillsFile?.DataList?.Any() == true;

        // Current editable skill and level
        public Skill? CurrentSkill
        {
            get
            {
                if (!IsSkillsLoaded) return null;
                return _skillsFile!.DataList[_currentSkillIndex];
            }
        }

        public SkillLevel? CurrentLevel
        {
            get
            {
                if (CurrentSkill?.LevelList?.Count > _currentLevelIndex)
                {
                    return CurrentSkill.LevelList[_currentLevelIndex];
                }
                return null;
            }
        }

        // Reference skill and level
        public Skill? ReferenceSkill
        {
            get
            {
                if (_referenceSkillsFile?.DataList?.Count > _currentSkillIndex)
                {
                    return _referenceSkillsFile.DataList[_currentSkillIndex];
                }
                return null;
            }
        }

        public SkillLevel? ReferenceLevel
        {
            get
            {
                if (ReferenceSkill?.LevelList?.Count > _currentLevelIndex)
                {
                    return ReferenceSkill.LevelList[_currentLevelIndex];
                }
                return null;
            }
        }

        // Coin descriptions for easier binding
        public List<CoinDesc> CurrentCoinDescs
        {
            get
            {
                var coinDescs = new List<CoinDesc>();
                if (CurrentLevel?.CoinList != null)
                {
                    foreach (var coin in CurrentLevel.CoinList)
                    {
                        if (coin.CoinDescs != null)
                            coinDescs.AddRange(coin.CoinDescs);
                    }
                }
                return coinDescs;
            }
        }

        public List<CoinDesc> ReferenceCoinDescs
        {
            get
            {
                var coinDescs = new List<CoinDesc>();
                if (ReferenceLevel?.CoinList != null)
                {
                    foreach (var coin in ReferenceLevel.CoinList)
                    {
                        if (coin.CoinDescs != null)
                            coinDescs.AddRange(coin.CoinDescs);
                    }
                }
                return coinDescs;
            }
        }

        // Navigation properties
        public bool CanGoPreviousSkill => IsSkillsLoaded && _currentSkillIndex > 0;
        public bool CanGoNextSkill => IsSkillsLoaded && _currentSkillIndex < _skillsFile!.DataList.Count - 1;
        public bool CanGoPreviousLevel => CurrentSkill?.LevelList?.Count > 0 && _currentLevelIndex > 0;
        public bool CanGoNextLevel => CurrentSkill?.LevelList?.Count > 0 && _currentLevelIndex < CurrentSkill.LevelList.Count - 1;
        
        public string SkillNavigationText => IsSkillsLoaded ? $"Skill {_currentSkillIndex + 1} of {_skillsFile!.DataList.Count}" : string.Empty;
        public string LevelNavigationText => CurrentSkill?.LevelList?.Count > 0 ? $"Level {_currentLevelIndex + 1} of {CurrentSkill.LevelList.Count}" : string.Empty;

        #endregion

        #region Event Handlers

        private async void OnSelectFileClick(object? sender, RoutedEventArgs e)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select Skills Data File",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("JSON Files"){ Patterns = new[] { "*.json" } },
                    new FilePickerFileType("All Files"){ Patterns = new[] { "*.*" } }
                }
            });

            if (files.Count > 0)
                await LoadSkillsFile(files[0].Path.LocalPath);
        }

        private void OnPreviousSkillClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoPreviousSkill)
            {
                SaveCurrentData();
                _currentSkillIndex--;
                _currentLevelIndex = 0; // Reset to first level of new skill
                UpdateAllProperties();
            }
        }

        private void OnNextSkillClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoNextSkill)
            {
                SaveCurrentData();
                _currentSkillIndex++;
                _currentLevelIndex = 0; // Reset to first level of new skill
                UpdateAllProperties();
            }
        }

        private void OnPreviousLevelClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoPreviousLevel)
            {
                SaveCurrentData();
                _currentLevelIndex--;
                UpdateAllProperties();
            }
        }

        private void OnNextLevelClick(object? sender, RoutedEventArgs e)
        {
            if (CanGoNextLevel)
            {
                SaveCurrentData();
                _currentLevelIndex++;
                UpdateAllProperties();
            }
        }

        #endregion

        #region Methods

        private async Task LoadSkillsFile(string filePath)
        {
            try
            {
                _filePath = filePath;
                var json = await File.ReadAllTextAsync(filePath);
                _skillsFile = JsonConvert.DeserializeObject<SkillsFile>(json);
                _currentSkillIndex = 0;
                _currentLevelIndex = 0;

                if (_skillsFile?.DataList?.Any() != true)
                    throw new InvalidOperationException("No skills data found in the file or invalid format.");

                await LoadReferenceFile(filePath);

                if (this.FindControl<TextBlock>("FilePathText") is TextBlock filePathText)
                {
                    var fileName = Path.GetFileName(filePath);
                    var referenceStatus = _referenceSkillsFile != null ? " (Reference loaded)" : " (No reference)";
                    filePathText.Text = fileName + referenceStatus;
                }

                UpdateAllProperties();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading skills file: {ex.Message}");
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
                    _referenceSkillsFile = JsonConvert.DeserializeObject<SkillsFile>(json);
                }
                else _referenceSkillsFile = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading reference file: {ex.Message}");
                _referenceSkillsFile = null;
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
            if (!IsSkillsLoaded) return;
            SaveToFile();
        }

        private async void SaveToFile()
        {
            if (_skillsFile == null || string.IsNullOrEmpty(_filePath)) return;
            try
            {
                var json = JsonConvert.SerializeObject(_skillsFile, Formatting.Indented);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
            }
        }

        private void UpdateAllProperties()
        {
            OnPropertyChanged(nameof(IsSkillsLoaded));
            OnPropertyChanged(nameof(CurrentSkill));
            OnPropertyChanged(nameof(CurrentLevel));
            OnPropertyChanged(nameof(ReferenceSkill));
            OnPropertyChanged(nameof(ReferenceLevel));
            OnPropertyChanged(nameof(CurrentCoinDescs));
            OnPropertyChanged(nameof(ReferenceCoinDescs));
            OnPropertyChanged(nameof(CanGoPreviousSkill));
            OnPropertyChanged(nameof(CanGoNextSkill));
            OnPropertyChanged(nameof(CanGoPreviousLevel));
            OnPropertyChanged(nameof(CanGoNextLevel));
            OnPropertyChanged(nameof(SkillNavigationText));
            OnPropertyChanged(nameof(LevelNavigationText));
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }

    // Your existing classes with INotifyPropertyChanged implementation
    public class SkillsFile
    {
        [JsonProperty("dataList")]
        public List<Skill> DataList { get; set; } = new();
    }

    public class Skill : INotifyPropertyChanged
    {
        private int _id;
        private List<SkillLevel> _levelList = new();

        [JsonProperty("id")]
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        [JsonProperty("levelList")]
        public List<SkillLevel> LevelList { get => _levelList; set { _levelList = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class SkillLevel : INotifyPropertyChanged
    {
        private int _level;
        private string _name = string.Empty;
        private string _desc = string.Empty;
        private List<CoinListItem> _coinList = new();

        [JsonProperty("level")]
        public int Level { get => _level; set { _level = value; OnPropertyChanged(); } }

        [JsonProperty("name")]
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        [JsonProperty("desc")]
        public string Desc { get => _desc; set { _desc = value; OnPropertyChanged(); } }

        [JsonProperty("coinlist")]
        public List<CoinListItem> CoinList { get => _coinList; set { _coinList = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class CoinListItem : INotifyPropertyChanged
    {
        private List<CoinDesc> _coinDescs = new();

        [JsonProperty("coindescs")]
        public List<CoinDesc> CoinDescs { get => _coinDescs; set { _coinDescs = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class CoinDesc : INotifyPropertyChanged
    {
        private string _desc = string.Empty;

        [JsonProperty("desc")]
        public string Desc { get => _desc; set { _desc = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}