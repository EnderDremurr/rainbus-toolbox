using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public class CoinListItemViewModel
{
    public int Index { get; set; } // Coin 1, Coin 2...
    public ObservableCollection<CoinDesc> Descs { get; } = [];
}

public partial class SkillsEgoTranslationEditorViewModel(
    string currentId = "",
    string currentEgoName = "",
    string referenceEgoName = "")
    : TranslationEditorViewModel<SkillsEgoFile, SkillEgo>
{
    private int _currentLevelIndex;

    [ObservableProperty] private SkillEgoLevel? _currentLevel;
    [ObservableProperty] private SkillEgoLevel? _referenceLevel;

    private string _currentId = currentId;
    [ObservableProperty] private string _currentEgoName = currentEgoName;
    [ObservableProperty] private string _referenceEgoName = referenceEgoName;

    [ObservableProperty] private bool _canGoNextLevel;
    [ObservableProperty] private bool _canGoPreviousLevel;

    [ObservableProperty] private string _levelNavigationText = "";

    public ObservableCollection<CoinListItemViewModel> CurrentCoins { get; } = new();
    public ObservableCollection<CoinListItemViewModel> ReferenceCoins { get; } = new();
    
    

    private readonly RepositoryManager _repositoryManager = (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    public string SkillName
    {
        get => CurrentItem?.LevelList.FirstOrDefault()?.Name ?? string.Empty;
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
        get => CurrentItem?.LevelList.FirstOrDefault()?.AbnormalityName ?? string.Empty;
        set
        {
            if (CurrentItem?.LevelList != null)
            {
                Console.WriteLine($"Updating {CurrentItem.LevelList.Count} levels with: {value}");
                foreach (var level in CurrentItem.LevelList)
                {
                    level.AbnormalityName = value;
                    Console.WriteLine($"Set level AbnormalityName to: {level.AbnormalityName}");
                }
                OnPropertyChanged();
            }
        }
    }
    

    private void GetCurrentSkillsEgoName()
    {
        if (CurrentItem == null) return;
        _currentId = CurrentItem.Id.ToString()[..5];

        CurrentEgoName = _repositoryManager.EgoNames.DataList.FirstOrDefault(i => i.Id.ToString() == _currentId)?.Name.ToString() ?? "Не найдено =(";
        ReferenceEgoName = _repositoryManager.EgoNamesReference.DataList.FirstOrDefault(i => i.Id.ToString() == _currentId)?.Name.ToString() ?? "Не найдено =(";
    }

    partial void OnCurrentEgoNameChanged(string value)
    {
        _repositoryManager.EgoNames.DataList.FirstOrDefault(i => i.Id.ToString() == _currentId)!.Name = value;
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

    public override void LoadReferenceFile(SkillsEgoFile file)
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
        if (EditableFile == null || CurrentItem == null || _currentLevelIndex >= CurrentItem.LevelList.Count - 1) return;
        _currentLevelIndex++;
        UpdateCurrent();
        UpdateReference();
        UpdateNavigation();
    }

    private void UpdateCurrent()
    {
        if (EditableFile is { DataList.Count: > 0 })
        {
            CurrentItem = EditableFile.DataList[CurrentIndex];
            CurrentLevel = CurrentItem.LevelList.Count > _currentLevelIndex ? CurrentItem.LevelList[_currentLevelIndex] : null;

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
            CurrentItem = ReferenceFile.DataList.First(r => r.Id.ToString() == CurrentItem?.Id.ToString());
            ReferenceLevel = CurrentItem.LevelList.Count > _currentLevelIndex ? CurrentItem.LevelList[_currentLevelIndex] : null;
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
        CanGoPrevious = CurrentIndex > 0;
        CanGoNext = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 1;

        CanGoPreviousLevel = CurrentItem?.LevelList != null && _currentLevelIndex > 0;
        CanGoNextLevel = CurrentItem?.LevelList != null && _currentLevelIndex < (CurrentItem.LevelList.Count - 1);

        NavigationText = $"{CurrentIndex + 1} / {EditableFile?.DataList.Count ?? 0}";
        LevelNavigationText = CurrentItem?.LevelList != null ? $"{_currentLevelIndex + 1} / {CurrentItem.LevelList.Count}" : "";
    }
}
