using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class TranslationEditorViewModel<TFile, TItem> : ObservableObject where TFile : LocalizationFileBase, ILocalizationContainer<TItem>
{
    public TFile? EditableFile { get; protected set; }
    public TFile? ReferenceFile { get; protected set; }
    public int CurrentIndex { get; protected set; } = 0;

    [ObservableProperty] protected bool _canGoPrevious;
    [ObservableProperty] protected bool _canGoNext;
    [ObservableProperty] protected string _navigationText = "";
    [ObservableProperty] protected TItem? _currentItem;
    [ObservableProperty] protected TItem? _referenceItem;

    public bool IsFileLoaded => EditableFile != null && EditableFile.DataList.Count > 0;

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

    public virtual void GoPrevious()
    {
        if (CurrentIndex <= 0) return;
        CurrentIndex--;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    public virtual void GoNext()
    {
        if (EditableFile == null || CurrentIndex >= EditableFile.DataList.Count - 1) return;
        CurrentIndex++;
        UpdateCurrentItem();
        UpdateReferenceItem(); 
        UpdateNavigation();
    }

    protected virtual void UpdateCurrentItem()
    {
        if (EditableFile != null && EditableFile.DataList.Count > 0)
            CurrentItem = EditableFile.DataList[CurrentIndex];
    }

    // Add this new method
    protected virtual void UpdateReferenceItem()
    {
        if (ReferenceFile != null && ReferenceFile.DataList.Count > CurrentIndex)
            ReferenceItem = ReferenceFile.DataList[CurrentIndex];
        else
            ReferenceItem = default;
    }

    protected virtual void UpdateNavigation()
    {
        CanGoPrevious = CurrentIndex > 0;
        CanGoNext = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 1;
        NavigationText = $"{CurrentIndex + 1} / {EditableFile?.DataList.Count ?? 0}";
    }
}