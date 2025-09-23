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
        EditableFile = file;
        CurrentIndex = 0;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
        
    }

    public void LoadReferenceFile(SkillsFile file)
    {
        ReferenceFile = file;
        UpdateReference();
    }

    public void GoPreviousSkill()
    {
        if (CurrentIndex <= 0) return;
        CurrentIndex--;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    public void GoNextSkill()
    {
        if (EditableFile == null || CurrentIndex >= EditableFile.DataList.Count - 1) return;
        CurrentIndex++;
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
        if (EditableFile == null || _currentItem == null || _currentLevelIndex >= _currentItem.LevelList.Count - 1) return;
        _currentLevelIndex++;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    private void UpdateCurrent()
    {
        if (EditableFile != null && EditableFile.DataList.Count > 0)
        {
            _currentItem = EditableFile.DataList[CurrentIndex];
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
        if (ReferenceFile != null && ReferenceFile.DataList.Count > CurrentIndex)
        {
            _referenceItem = ReferenceFile.DataList[CurrentIndex];
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
        CanGoPrevious = CurrentIndex > 0;
        CanGoNext = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 1;

        CanGoPreviousLevel = _currentItem?.LevelList != null && _currentLevelIndex > 0;
        CanGoNextLevel = _currentItem?.LevelList != null && _currentLevelIndex < (_currentItem.LevelList.Count - 1);

        NavigationText = $"{CurrentIndex + 1} / {EditableFile?.DataList.Count ?? 0}";
        LevelNavigationText = _currentItem?.LevelList != null ? $"{_currentLevelIndex + 1} / {_currentItem.LevelList.Count}" : "";
    }
}
