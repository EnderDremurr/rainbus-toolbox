using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class SkillsTranslationEditorViewModel : TranslationEditorViewModel<SkillsFile, Skill>
{
    private int _currentLevelIndex;

    [ObservableProperty] private SkillLevel? _currentLevel;
    [ObservableProperty] private SkillLevel? _referenceLevel;

    [ObservableProperty] private bool _canGoNextLevel;
    [ObservableProperty] private bool _canGoPreviousLevel; 

    [ObservableProperty] private string _levelNavigationText = "";
    
    public string SkillName 
    {
        get => CurrentItem?.LevelList?.FirstOrDefault()?.Name ?? string.Empty;
        set 
        {
            if (CurrentItem?.LevelList != null)
            {
                foreach (var level in CurrentItem.LevelList)
                    level.Name = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<CoinListItemViewModel> CurrentCoins { get; } = new();
    public ObservableCollection<CoinListItemViewModel> ReferenceCoins { get; } = new();

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

    public override void LoadReferenceFile(SkillsFile file)
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
        if (EditableFile == null || CurrentItem == null || _currentLevelIndex >= CurrentItem.LevelList.Count - 1) return;
        _currentLevelIndex++;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    private void UpdateCurrent()
    {
        if (EditableFile != null && EditableFile.DataList.Count > 0)
        {
            CurrentItem  = EditableFile.DataList[CurrentIndex];
            CurrentLevel = CurrentItem .LevelList.Count > _currentLevelIndex ? CurrentItem .LevelList[_currentLevelIndex] : null;

            CurrentCoins.Clear();
            if (CurrentLevel != null)
            {
                int coinIndex = 1;
                foreach (var coin in CurrentLevel.CoinList)
                {
                    var vm = new CoinListItemViewModel { Index = coinIndex++ };
                    foreach (var desc in coin.CoinDescs)
                        vm.Descs.Add(desc);
                    CurrentCoins.Add(vm);
                }
            }
        }
        OnPropertyChanged(nameof(SkillName));
    }

    private void UpdateReference()
    {
        if (ReferenceFile != null && ReferenceFile.DataList.Count > CurrentIndex)
        {
            ReferenceItem = ReferenceFile.DataList.First(r => r.Id.ToString() == CurrentItem.Id.ToString());
            ReferenceLevel = ReferenceItem.LevelList.Count > _currentLevelIndex ? ReferenceItem.LevelList[_currentLevelIndex] : null;
        }
        else
        {
            ReferenceItem = null;
            ReferenceLevel = null;
        }

        ReferenceCoins.Clear();
        if (ReferenceLevel != null)
        {
            int coinIndex = 1;
            foreach (var coin in ReferenceLevel.CoinList)
            {
                var vm = new CoinListItemViewModel { Index = coinIndex++ };
                foreach (var desc in coin.CoinDescs)
                    vm.Descs.Add(desc);
                ReferenceCoins.Add(vm);
            }
        }
    }

    protected override void UpdateNavigation()
    {

        CanGoPreviousLevel = CurrentItem?.LevelList != null && _currentLevelIndex > 0;
        CanGoNextLevel = CurrentItem?.LevelList != null && _currentLevelIndex < (CurrentItem.LevelList.Count - 1);

        NavigationText = $"{CurrentIndex + 1} / {EditableFile?.DataList.Count ?? 0}";
        LevelNavigationText = CurrentItem?.LevelList != null ? $"{_currentLevelIndex + 1} / {CurrentItem.LevelList.Count}" : "";
    }
}
