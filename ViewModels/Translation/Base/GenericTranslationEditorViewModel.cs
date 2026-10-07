using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class GenericTranslationEditorViewModel<TFile, TItem> : TranslationEditorViewModelBase
    where TFile : LocalizationFileBase, ILocalizationContainer<TItem>
    where TItem : LocalizationItemBase
{
    [ObservableProperty] protected TItem? _currentItem;
    [ObservableProperty] protected TItem? _referenceItem;
    public TFile? EditableFile { get; protected set; }
    public TFile? ReferenceFile { get; protected set; }


    public bool IsFileLoaded => EditableFile != null && EditableFile.DataList.Count > 0;

    public override int ItemCount { get; protected set; }

    public virtual void LoadEditableFile(TFile file)
    {
        EditableFile = file;
        CurrentIndex = 0;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
    }

    public virtual void LoadReferenceFile(TFile file)
    {
        ReferenceFile = file;
        UpdateReferenceItem();
    }


    public override void StepIndex(int step)
    {
        if (EditableFile == null)
            return;

        var maxIndex = EditableFile.DataList.Count - 1;

        var tempIndex = CurrentIndex + step;
        if (tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if (tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        OnIndexChanged();
    }

    public override void OnIndexChanged()
    {
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    private void OnNavigationTextChanged(string value)
    {
        if (int.TryParse(value, out var userIndex) && userIndex > 0) GoCustom();
    }

    public virtual void GoCustom()
    {
        var index = int.Parse(NavigationText ?? throw new InvalidOperationException()) - 1;
        var sanitizedStep = index;
        if (index < 0) index = 0;
        if (index > EditableFile.DataList.Count - 1) index = EditableFile.DataList.Count - 1;

        if (CurrentIndex != index)
        {
            CurrentIndex = index;
            OnIndexChanged();
        }
    }


    protected virtual void UpdateCurrentItem()
    {
        if (EditableFile != null && EditableFile.DataList.Count > 0)
            CurrentItem = EditableFile.DataList[CurrentIndex];
    }

    protected virtual void UpdateReferenceItem()
    {
        if (ReferenceFile != null && CurrentItem != null)
            ReferenceItem = ReferenceFile.DataList.FirstOrDefault(x => x.Id == CurrentItem.Id);
        else
            ReferenceItem = default;
    }

    protected virtual void UpdateNavigation()
    {
        NavigationText = $"{CurrentIndex + 1}";
        NavigationCountText = $"{EditableFile?.DataList.Count ?? 0}";
    }

    public override void SetFileToEdit(LocalizationFileBase file)
    {
        if (file is not TFile typedFile)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(
                new ArgumentException("Приложение попыталось открыть редактор для неверного файла!"));
            return;
        }

        EditableFile = typedFile;
        OnIndexChanged();
    }

    public override void SetReferenceFile(LocalizationFileBase file)
    {
        if (file is not TFile typedFile)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(
                new ArgumentException("Приложение попыталось открыть редактор для неверного файла!"));
            return;
        }

        ReferenceFile = typedFile;
        OnIndexChanged();
    }

    public override void AskEditorToSave(LocalizationManager localizationManager)
    {
        if (EditableFile == null)
            return;
        LocalizationManager.SaveObjectToFile(EditableFile);
    }
}