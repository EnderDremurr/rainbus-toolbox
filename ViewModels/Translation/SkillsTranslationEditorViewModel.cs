using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class SkillsTranslationEditorViewModel : ObservableObject
{
    private SkillsFile? _editableFile;
    private SkillsFile? _referenceFile;
    private int _currentSkillIndex = 0;
    private int _currentLevelIndex = 0;

    [ObservableProperty] private Skill? _currentSkill;
    [ObservableProperty] private SkillLevel? _currentLevel;
    [ObservableProperty] private Skill? _referenceSkill;
    [ObservableProperty] private SkillLevel? _referenceLevel;

    [ObservableProperty] private bool _canGoPreviousSkill;
    [ObservableProperty] private bool _canGoNextSkill;
    [ObservableProperty] private bool _canGoPreviousLevel;
    [ObservableProperty] private bool _canGoNextLevel;

    [ObservableProperty] private string _skillNavigationText = "";
    [ObservableProperty] private string _levelNavigationText = "";
    
    public List<CoinDesc> ReferenceCoinDescs => ReferenceLevel?.CoinList.SelectMany(c => c.CoinDescs).ToList() ?? new();
    public List<CoinDesc> CurrentCoinDescs => CurrentLevel?.CoinList.SelectMany(c => c.CoinDescs).ToList() ?? new();


    public bool IsSkillsLoaded => _editableFile != null && _editableFile.DataList.Count > 0;

    public void LoadEditableFile(SkillsFile file)
    {
        _editableFile = file;
        _currentSkillIndex = 0;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsSkillsLoaded));
    }

    public void LoadReferenceFile(SkillsFile file)
    {
        _referenceFile = file;
        UpdateReference();
    }

    public void GoPreviousSkill()
    {
        if (_currentSkillIndex <= 0) return;
        _currentSkillIndex = _currentSkillIndex - 1;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    public void GoNextSkill()
    {
        if (_editableFile == null || _currentSkillIndex >= _editableFile.DataList.Count - 1) return;
        _currentSkillIndex++;
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
        if (_editableFile == null || CurrentSkill == null || _currentLevelIndex >= CurrentSkill.LevelList.Count - 1) return;
        _currentLevelIndex++;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    private void UpdateCurrent()
    {
        if (_editableFile != null && _editableFile.DataList.Count > 0)
        {
            CurrentSkill = _editableFile.DataList[_currentSkillIndex];
            if (CurrentSkill.LevelList.Count > _currentLevelIndex)
                CurrentLevel = CurrentSkill.LevelList[_currentLevelIndex];
        }
    }

    private void UpdateReference()
    {
        if (_referenceFile != null && _referenceFile.DataList.Count > _currentSkillIndex)
        {
            ReferenceSkill = _referenceFile.DataList[_currentSkillIndex];
            if (ReferenceSkill.LevelList.Count > _currentLevelIndex)
                ReferenceLevel = ReferenceSkill.LevelList[_currentLevelIndex];
        }
        else
        {
            ReferenceSkill = null;
            ReferenceLevel = null;
        }
    }

    private void UpdateNavigation()
    {
        CanGoPreviousSkill = _currentSkillIndex > 0;
        CanGoNextSkill = _editableFile != null && _currentSkillIndex < _editableFile.DataList.Count - 1;

        CanGoPreviousLevel = CurrentSkill?.LevelList != null && _currentLevelIndex > 0;
        CanGoNextLevel = CurrentSkill?.LevelList != null && _currentLevelIndex < (CurrentSkill.LevelList.Count - 1);

        SkillNavigationText = $"{_currentSkillIndex + 1} / {_editableFile?.DataList.Count ?? 0}";
        LevelNavigationText = CurrentSkill?.LevelList != null ? $"{_currentLevelIndex + 1} / {CurrentSkill.LevelList.Count}" : "";
    }
}
