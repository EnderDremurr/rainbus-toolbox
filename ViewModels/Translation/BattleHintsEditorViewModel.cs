using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.Services;
using RainbusToolbox.ViewModels.Misc;
using RainbusToolbox.ViewModels.Translation.Base;

namespace RainbusToolbox.ViewModels.Translation;

public partial class
    BattleHintsEditorViewModel(DialogService dialogService)
    : GenericTranslationEditorViewModel<NormalBattleHintLocalizationFile, GenericIdContent>
{
    private const int PageSize = 20;

    public ObservableCollection<GenericIdContent> CurrentPageHintsList { get; } = [];

    public override int ItemCount =>
        Math.Max(1, (int)Math.Ceiling((EditableFile?.DataList.Count ?? 0) / (double)PageSize));

    protected override void UpdateNavigation()
    {
        NavigationText = $"{CurrentIndex + 1}";
        NavigationCountText = $"{ItemCount}";
    }

    protected override void UpdateCurrentItem()
    {
        CurrentPageHintsList.Clear();
        var start = CurrentIndex * PageSize;
        CurrentPageHintsList.AddRange(EditableFile?.DataList.Skip(start).Take(PageSize) ?? []);
    }

    [RelayCommand]
    public async Task AddHint()
    {
        var vm = await dialogService.ShowPopupAsync(
            "Добавление нового хинта",
            "Нужно ввести текст нового хинта",
            true,
            "Текст хинта", null,
            new PopupButton { Label = "Отмена", ResultValue = "cancel" },
            new PopupButton { Label = "Добавить", ResultValue = "ok" }
        );

        if (vm?.Result != "ok" || string.IsNullOrWhiteSpace(vm.InputValue))
            return;
        if (EditableFile == null)
            return;

        var nextId = EditableFile.DataList.Count != 0
            ? EditableFile.DataList.Max(h => int.Parse(h.Id)) + 1
            : 1;

        var newHint = new GenericIdContent
        {
            Id = nextId.ToString(),
            Content = vm.InputValue
        };

        EditableFile?.DataList.Add(newHint);
        CurrentIndex = ItemCount - 1;
        OnIndexChanged();
    }

    [RelayCommand]
    public void DeleteHint(string id)
    {
        var hint = CurrentPageHintsList.FirstOrDefault(h => int.Parse(h.Id) == int.Parse(id));
        if (hint != null)
            EditableFile?.DataList.Remove(hint);

        OnIndexChanged();
    }

    [RelayCommand]
    public async Task UpdateHint(int id)
    {
        var hint = CurrentPageHintsList.FirstOrDefault(h => int.Parse(h.Id) == id);
        if (hint == null)
            return;

        var vm = await dialogService.ShowPopupAsync(
            "Изменение хинта",
            "Здесь можно изменить текст",
            true,
            "Текст хинта",
            hint.Content!,
            new PopupButton { Label = "Отмена", ResultValue = "cancel" },
            new PopupButton { Label = "Применить", ResultValue = "ok" }
        );

        if (vm?.Result != "ok" || string.IsNullOrWhiteSpace(vm.InputValue))
            return;

        var index = CurrentPageHintsList.IndexOf(hint);
        if (index < 0)
            return;

        var globalIndex = CurrentIndex * PageSize + index;

        if (EditableFile != null)
            EditableFile.DataList[globalIndex].Content = vm.InputValue;

        OnIndexChanged();
    }

    public override void StepIndex(int step)
    {
        if (EditableFile == null)
            return;

        var maxIndex = ItemCount - 1;

        var tempIndex = CurrentIndex + step;
        if (tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if (tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        OnIndexChanged();
    }

    public override void GoCustom()
    {
        var index = int.Parse(NavigationText ?? throw new InvalidOperationException()) - 1;
        var sanitizedStep = index;
        if (index < 0) index = 0;
        if (index > EditableFile?.DataList.Count - 1)
            index = ItemCount - 1;

        if (CurrentIndex != index)
        {
            CurrentIndex = index;
            OnIndexChanged();
        }
    }

    public override void OnIndexChanged()
    {
        base.OnIndexChanged();
        OnPropertyChanged(nameof(ItemCount));
    }

    // this editor has no use of references, so i just cut them
    public override void LoadReferenceFile(NormalBattleHintLocalizationFile file)
    {
    }

    public override void SetReferenceFile(LocalizationFileBase file)
    {
    }

    protected override void UpdateReferenceItem()
    {
    }
}