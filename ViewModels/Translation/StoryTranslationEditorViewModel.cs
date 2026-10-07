using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class
    StoryTranslationEditorViewModel(LocalizationManager localizationManager)
    : GenericTranslationEditorViewModel<StoryDataFile, StoryDataItem>
{
    [ObservableProperty] private ScenarioModelCode? _scenarioModel;
    [ObservableProperty] private ScenarioModelCode? _scenarioModelReference;

    public string DisplayTeller
    {
        get => CurrentItem?.Teller ?? ScenarioModel?.Name ?? string.Empty;
        set
        {
            if (CurrentItem?.Teller == null) return;
            CurrentItem.Teller = value;
            OnPropertyChanged();
        }
    }

    public string DisplayTitle
    {
        get => CurrentItem?.Title ?? ScenarioModel?.NickName ?? string.Empty;
        set
        {
            if (CurrentItem?.Title == null) return;
            CurrentItem.Title = value;
            OnPropertyChanged();
        }
    }


    public string DisplayTellerReference => ReferenceItem?.Teller ?? ScenarioModelReference?.Name ?? string.Empty;
    public string DisplayTitleReference => ReferenceItem?.Title ?? ScenarioModelReference?.NickName ?? string.Empty;

    public string? EditableTeller
    {
        get => CurrentItem?.Teller;
        set
        {
            if (CurrentItem?.Teller == null) return;
            CurrentItem.Teller = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayTeller));
        }
    }

    public string? EditableTitle
    {
        get => CurrentItem?.Title;
        set
        {
            if (CurrentItem?.Title == null) return;
            CurrentItem.Title = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayTitle));
        }
    }

    public bool CanEditTeller => CurrentItem?.Teller is not null;
    public bool CanEditTitle => CurrentItem?.Title is not null;

    protected override void UpdateCurrentItem()
    {
        base.UpdateCurrentItem();

        if (CurrentItem?.Model != null)
            ScenarioModel = localizationManager.ScenarioModelCodes.DataList
                .FirstOrDefault(x => x.Id == CurrentItem.Model);
        else
            ScenarioModel = null;

        OnPropertyChanged(nameof(DisplayTeller));
        OnPropertyChanged(nameof(DisplayTitle));
        OnPropertyChanged(nameof(EditableTeller));
        OnPropertyChanged(nameof(EditableTitle));
        OnPropertyChanged(nameof(CanEditTeller));
        OnPropertyChanged(nameof(CanEditTitle));
    }

    protected override void UpdateReferenceItem()
    {
        base.UpdateReferenceItem();

        if (ReferenceItem?.Model != null)
            ScenarioModelReference = localizationManager.ScenarioModelCodesReference.DataList
                .FirstOrDefault(x => x.Id == ReferenceItem.Model);
        else
            ScenarioModelReference = null;

        OnPropertyChanged(nameof(DisplayTellerReference));
        OnPropertyChanged(nameof(DisplayTitleReference));
    }
}