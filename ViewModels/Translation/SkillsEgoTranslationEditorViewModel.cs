using System.Collections.ObjectModel;
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
    : TranslationEditorViewModel<SkillLocalizationFile, Skill>
{
    private readonly RepositoryManager _repositoryManager =
        (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    [ObservableProperty] private bool _canGoNextLevel;
    [ObservableProperty] private bool _canGoPreviousLevel;
    [ObservableProperty] private string _currentEgoName = currentEgoName;

    private string _currentId = currentId;

    [ObservableProperty] private SkillLevel? _currentLevel;
    private int _currentLevelIndex;

    [ObservableProperty] private string _levelNavigationText = "";
    [ObservableProperty] private string _referenceEgoName = referenceEgoName;
    [ObservableProperty] private SkillLevel? _referenceLevel;

    public ObservableCollection<CoinListItemViewModel> CurrentCoins { get; } = new();
    public ObservableCollection<CoinListItemViewModel> ReferenceCoins { get; } = new();

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
                Console.WriteLine(AppLang.AbnormalityRenameStart, CurrentItem.LevelList.Count, value);
                foreach (var level in CurrentItem.LevelList)
                {
                    level.AbnormalityName = value;
                    Console.WriteLine(AppLang.AbnormalityRenameProcess, level.AbnormalityName);
                }

                OnPropertyChanged();
            }
        }
    }

    public bool HasAnAbnormalityName =>
        !string.IsNullOrWhiteSpace(CurrentItem?.LevelList.FirstOrDefault()?.AbnormalityName);

    private void GetCurrentSkillsEgoName()
    {
        if (CurrentItem == null) return;

        var idString = CurrentItem.Id;

        _currentId = idString.Length >= 5 ? idString[..5] : idString;


        CurrentEgoName = _repositoryManager.EgoNames.DataList
                             .FirstOrDefault(i => i.Id.ToString() == _currentId)?.Name.ToString()
                         ?? "Не найдено =(";

        ReferenceEgoName = _repositoryManager.EgoNamesReference.DataList
                               .FirstOrDefault(i => i.Id.ToString() == _currentId)?.Name.ToString()
                           ?? "Не найдено =(";
    }


    public override void LoadEditableFile(SkillLocalizationFile file)
    {
        base.LoadEditableFile(file);
        CurrentIndex = 0;
        _currentLevelIndex = 0;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
        GetCurrentSkillsEgoName();
    }

    public override void LoadReferenceFile(SkillLocalizationFile file)
    {
        base.LoadReferenceFile(file);
        UpdateReferenceItem();
    }

    public override void GoPrevious(object stepObj)
    {
        var step = int.Parse(stepObj.ToString() ?? throw new InvalidOperationException()) * -1;

        if (EditableFile == null)
            return;

        var maxIndex = EditableFile.DataList.Count - 1;

        var tempIndex = CurrentIndex + step;
        if (tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if (tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        _currentLevelIndex = 0;

        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        GetCurrentSkillsEgoName();
    }

    public override void GoNext(object stepObj)
    {
        var step = int.Parse(stepObj.ToString() ?? throw new InvalidOperationException());

        if (EditableFile == null)
            return;

        var maxIndex = EditableFile.DataList.Count - 1;

        var tempIndex = CurrentIndex + step;
        if (tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if (tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        _currentLevelIndex = 0;


        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        GetCurrentSkillsEgoName();
    }

    public void GoPreviousLevel()
    {
        if (_currentLevelIndex <= 0) return;
        _currentLevelIndex--;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    public void GoNextLevel()
    {
        if (EditableFile == null || CurrentItem == null ||
            _currentLevelIndex >= CurrentItem.LevelList.Count - 1) return;
        _currentLevelIndex++;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    protected override void UpdateCurrentItem()
    {
        if (EditableFile is { DataList.Count: > 0 })
        {
            CurrentItem = EditableFile.DataList[CurrentIndex];
            CurrentLevel = CurrentItem.LevelList.Count > _currentLevelIndex
                ? CurrentItem.LevelList[_currentLevelIndex]
                : null;

            CurrentCoins.Clear();
            if (CurrentLevel != null)
            {
                var coinIndex = 1;
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
        OnPropertyChanged(nameof(HasAnAbnormalityName));
        OnPropertyChanged(nameof(CurrentLevel));
        OnPropertyChanged(nameof(CurrentCoins));
    }

    protected override void UpdateReferenceItem()
    {
        if (ReferenceFile != null && ReferenceFile.DataList.Count > CurrentIndex)
        {
            ReferenceItem = ReferenceFile.DataList.First(r => r.Id.ToString() == CurrentItem?.Id);
            ReferenceLevel = ReferenceItem.LevelList.Count > _currentLevelIndex
                ? ReferenceItem.LevelList[_currentLevelIndex]
                : null;
        }
        else
        {
            ReferenceItem = null;
            ReferenceLevel = null;
        }

        ReferenceCoins.Clear();
        if (ReferenceLevel != null)
        {
            var coinIndex = 1;
            foreach (var coin in ReferenceLevel.CoinList)
            {
                var vm = new CoinListItemViewModel { Index = coinIndex++ };
                foreach (var desc in coin.CoinDescs)
                    vm.Descs.Add(desc);
                ReferenceCoins.Add(vm);
            }
        }

        OnPropertyChanged(nameof(ReferenceLevel));
        OnPropertyChanged(nameof(ReferenceCoins));
    }

    protected override void UpdateNavigation()
    {
        CanGoPreviousLevel = CurrentItem?.LevelList != null && _currentLevelIndex > 0;
        CanGoNextLevel = CurrentItem?.LevelList != null && _currentLevelIndex < CurrentItem.LevelList.Count - 1;

        NavigationText = $"{CurrentIndex + 1}";
        NavigationCountText = $"{EditableFile?.DataList.Count ?? 0}";
        LevelNavigationText = CurrentItem?.LevelList != null
            ? $"{_currentLevelIndex + 1} / {CurrentItem.LevelList.Count}"
            : "";
    }
}