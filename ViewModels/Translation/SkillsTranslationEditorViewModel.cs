using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class SkillsTranslationEditorViewModel : TranslationEditorViewModel<SkillsFile, Skill>
{
    private int _currentLevelIndex = 0;

    [ObservableProperty] private SkillLevel? _currentLevel;
    [ObservableProperty] private SkillLevel? _referenceLevel;

    [ObservableProperty] private bool _canGoNextLevel;
    [ObservableProperty] private bool _canGoPreviousLevel;

    [ObservableProperty] private string _levelNavigationText = "";

    public ObservableCollection<CoinDesc> ReferenceCoinDescs { get; } = new();
    public ObservableCollection<CoinDesc> CurrentCoinDescs { get; } = new();


    public override void LoadEditableFile(SkillsFile file)
    {
        _editableFile = file;
        _currentIndex = 0;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
    }

    public void LoadReferenceFile(SkillsFile file)
    {
        _referenceFile = file;
        UpdateReference();
    }

    public void GoPreviousSkill()
    {
        if (_currentIndex <= 0) return;
        _currentIndex--;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    public void GoNextSkill()
    {
        if (_editableFile == null || _currentIndex >= _editableFile.DataList.Count - 1) return;
        _currentIndex++;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    public void GoPreviousLevel()
    {
        if (_currentLevelIndex <= 0) return;
        _currentLevelIndex--;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    public void GoNextLevel()
    {
        if (_editableFile == null || _currentItem == null || _currentLevelIndex >= _currentItem.LevelList.Count - 1) return;
        _currentLevelIndex++;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    private void UpdateCurrent()
    {
        if (_editableFile != null && _editableFile.DataList.Count > 0)
        {
            _currentItem = _editableFile.DataList[_currentIndex];
            if (_currentItem.LevelList.Count > _currentLevelIndex)
                CurrentLevel = _currentItem.LevelList[_currentLevelIndex];
            else
                CurrentLevel = null;

            // Update CurrentCoinDescs
            CurrentCoinDescs.Clear();
            if (CurrentLevel != null)
            {
                foreach (var coin in CurrentLevel.CoinList.SelectMany(c => c.CoinDescs))
                    CurrentCoinDescs.Add(coin);
            }
        }
    }

    private void UpdateReference()
    {
        if (_referenceFile != null && _referenceFile.DataList.Count > _currentIndex)
        {
            _referenceItem = _referenceFile.DataList[_currentIndex];
            if (_referenceItem.LevelList.Count > _currentLevelIndex)
                ReferenceLevel = _referenceItem.LevelList[_currentLevelIndex];
            else
                ReferenceLevel = null;
        }
        else
        {
            ReferenceItem = null;
            ReferenceLevel = null;
        }

        // Update ReferenceCoinDescs
        ReferenceCoinDescs.Clear();
        if (ReferenceLevel != null)
        {
            foreach (var coin in ReferenceLevel.CoinList.SelectMany(c => c.CoinDescs))
                ReferenceCoinDescs.Add(coin);
        }
    }

    private void UpdateNavigation()
    {
        CanGoPrevious = _currentIndex > 0;
        CanGoNext = _editableFile != null && _currentIndex < _editableFile.DataList.Count - 1;

        CanGoPreviousLevel = _currentItem?.LevelList != null && _currentLevelIndex > 0;
        CanGoNextLevel = _currentItem?.LevelList != null && _currentLevelIndex < (_currentItem.LevelList.Count - 1);

        NavigationText = $"{_currentIndex + 1} / {_editableFile?.DataList.Count ?? 0}";
        LevelNavigationText = _currentItem?.LevelList != null ? $"{_currentLevelIndex + 1} / {_currentItem.LevelList.Count}" : "";
    }
}
