using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public class CoinListItemViewModel
{
    public int Index { get; set; } // Coin 1, Coin 2...
    public ObservableCollection<CoinDesc> Descs { get; } = new();
}

public partial class SkillsEgoTranslationEditorViewModel : TranslationEditorViewModel<SkillsEgoFile, SkillEgo>
{
    private int _currentLevelIndex = 0;

    [ObservableProperty] private SkillEgoLevel? _currentLevel;
    [ObservableProperty] private SkillEgoLevel? _referenceLevel;

    private string currentId;
    [ObservableProperty] private string _currentEgoName;
    [ObservableProperty] private string _referenceEgoName;

    [ObservableProperty] private bool _canGoNextLevel;
    [ObservableProperty] private bool _canGoPreviousLevel;

    [ObservableProperty] private string _levelNavigationText = "";

    public ObservableCollection<CoinListItemViewModel> CurrentCoins { get; } = new();
    public ObservableCollection<CoinListItemViewModel> ReferenceCoins { get; } = new();
    
    

    private RepositoryManager _repositoryManager;
    public SkillsEgoTranslationEditorViewModel()
    {
        _repositoryManager = (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;
    }

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
    
    public string AbnormalityName
    {
        get => CurrentItem?.LevelList?.FirstOrDefault()?.AbnormalityName ?? string.Empty;
        set
        {
            if (CurrentItem?.LevelList != null)
            {
                foreach (var level in CurrentItem.LevelList)
                    level.AbnormalityName = value;
                OnPropertyChanged();
            }
        }
    }
    

    private void GetCurrentSkillsEgoName()
    {
        if (CurrentItem == null) return;
        currentId = CurrentItem.Id.ToString()[..5];

        CurrentEgoName = _repositoryManager.EgoNames.DataList.FirstOrDefault(i => i.Id.ToString() == currentId)?.Name.ToString() ?? "Не найдено =(";
        ReferenceEgoName = _repositoryManager.EgoNamesReference.DataList.FirstOrDefault(i => i.Id.ToString() == currentId)?.Name.ToString() ?? "Не найдено =(";
    }

    partial void OnCurrentEgoNameChanged(string value)
    {
        _repositoryManager.EgoNames.DataList.FirstOrDefault(i => i.Id.ToString() == currentId)!.Name = value;
        _repositoryManager.SaveObjectToFile(_repositoryManager.EgoNames);
    }

    public override void LoadEditableFile(SkillsEgoFile file)
    {
        base.LoadEditableFile(file);
        CurrentIndex = 0;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
        GetCurrentSkillsEgoName();
    }

    public void LoadReferenceFile(SkillsEgoFile file)
    {
        base.LoadReferenceFile(file);
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
        GetCurrentSkillsEgoName();
    }

    public void GoNextSkill()
    {
        if (EditableFile == null || CurrentIndex >= EditableFile.DataList.Count - 1) return;
        CurrentIndex++;
        _currentLevelIndex = 0;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
        GetCurrentSkillsEgoName();
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
        OnPropertyChanged(nameof(AbnormalityName));
    }

    private void UpdateReference()
    {
        if (ReferenceFile != null && ReferenceFile.DataList.Count > CurrentIndex)
        {
            _referenceItem = ReferenceFile.DataList.First(r => r.Id.ToString() == _currentItem.Id.ToString());
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
