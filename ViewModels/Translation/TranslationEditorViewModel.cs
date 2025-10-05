using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class TranslationEditorViewModel<TFile, TItem> : ObservableObject where TFile : LocalizationFileBase, ILocalizationContainer<TItem>
{
    public TFile? EditableFile { get; protected set; }
    public TFile? ReferenceFile { get; protected set; }
    public int CurrentIndex { get; protected set; } = 0;

    
    [ObservableProperty] protected string _navigationText = "";
    [ObservableProperty] protected TItem? _currentItem;
    [ObservableProperty] protected TItem? _referenceItem;

    #region Navigation

    [ObservableProperty] protected bool _canGoPrevious;
    [ObservableProperty] protected bool _canGoNext;
    [ObservableProperty] protected bool _canGoPreviousFive;
    [ObservableProperty] protected bool _canGoNextFive;
    [ObservableProperty] protected bool _canGoPreviousTen;
    [ObservableProperty] protected bool _canGoNextTen;

    #endregion

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

    public virtual void GoPrevious(object stepObj)
    {
        var step = int.Parse(stepObj.ToString() ?? throw new InvalidOperationException());
        
        switch (step)
        {
            case 1:
                if(!CanGoPrevious)
                    return;
                CurrentIndex--;
                break;
            case 5:
                if(!CanGoPreviousFive)
                    return;
                CurrentIndex -= 5;
                break;
            case 10:
                if(!CanGoPreviousTen)
                    return;
                CurrentIndex -= 10;
                break;
        }
        
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    public virtual void GoNext(object stepObj)
    {
        var step = int.Parse(stepObj.ToString() ?? throw new InvalidOperationException());
        
        switch (step)
        {
            case 1:
                if(!CanGoNext)
                    return;
                CurrentIndex++;
                break;
            case 5:
                if(!CanGoNextFive)
                    return;
                CurrentIndex += 5;
                break;
            case 10:
                if(!CanGoNextTen)
                    return;
                CurrentIndex += 10;
                break;
        }
        
        
        
        
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
        CanGoPrevious = EditableFile != null && EditableFile.DataList.Count > CurrentIndex;
        CanGoNext = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 1;
        
        CanGoPreviousFive = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 5;
        CanGoNextFive = EditableFile != null && EditableFile.DataList.Count > CurrentIndex + 5;
        
        CanGoPreviousTen = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 10;
        CanGoNextTen = EditableFile != null && CurrentIndex < EditableFile.DataList.Count - 10;
        
        NavigationText = $"{CurrentIndex + 1} / {EditableFile?.DataList.Count ?? 0}";
    }

    public virtual void SaveCurrentFile(RepositoryManager repositoryManager)
    {
        if(EditableFile == null)
            return;
        repositoryManager.SaveObjectToFile(EditableFile);
    }
}