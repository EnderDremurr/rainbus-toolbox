using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Views.Misc;

public partial class
    BattleHintsEditorViewModel : TranslationEditorViewModel<NormalBattleHintLocalizationFile, GenericIdContent>
{
    public ObservableCollection<GenericIdContent> ObservableDataList { get; } = [];

    public override void LoadEditableFile(NormalBattleHintLocalizationFile file)
    {
        base.LoadEditableFile(file);


        ObservableDataList.Clear();
        foreach (var item in EditableFile?.DataList!)
            ObservableDataList.Add(item);

        ObservableDataList.CollectionChanged += (_, _) => { EditableFile.DataList = ObservableDataList.ToList(); };
    }

    [RelayCommand]
    public async Task AddHint()
    {
        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        var vm = await PopUpWindow.ShowAsync(
            parent!,
            "Добавление нового хинта",
            "Нужно ввести текст нового хинта",
            true,
            "Текст хинта", null,
            new PopupButton { Label = "Отмена", ResultValue = "cancel" },
            new PopupButton { Label = "Добавить", ResultValue = "ok" }
        );

        if (vm.Result != "ok" || string.IsNullOrWhiteSpace(vm.InputValue))
            return;


        var nextId = ObservableDataList.Any()
            ? ObservableDataList.Max(h => int.Parse(h.Id)) + 1
            : 1;

        var newHint = new GenericIdContent
        {
            Id = nextId.ToString(),
            Content = vm.InputValue
        };

        ObservableDataList.Add(newHint);
    }

    [RelayCommand]
    public void DeleteHint(string id)
    {
        var hint = ObservableDataList.FirstOrDefault(h => int.Parse(h.Id) == int.Parse(id));
        if (hint != null)
            ObservableDataList.Remove(hint);
    }

    [RelayCommand]
    public async Task UpdateHint(int id)
    {
        var hint = ObservableDataList.FirstOrDefault(h => int.Parse(h.Id) == id);
        if (hint == null)
            return;

        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var vm = await PopUpWindow.ShowAsync(
            parent!,
            "Изменение хинта",
            "Здесь можно изменить текст",
            true,
            "Текст хинта",
            hint.Content!,
            new PopupButton { Label = "Отмена", ResultValue = "cancel" },
            new PopupButton { Label = "Применить", ResultValue = "ok" }
        );

        if (vm.Result != "ok" || string.IsNullOrWhiteSpace(vm.InputValue))
            return;

        var index = ObservableDataList.IndexOf(hint);
        if (index < 0)
            return;

        hint.Content = vm.InputValue;
        ObservableDataList[index] = hint;
    }
}