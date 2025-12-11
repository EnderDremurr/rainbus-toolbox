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
    [ObservableProperty] protected string _navigationCountText = "";
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

    public virtual void GoPrevious(object stepObj)
    {
        var step = int.Parse(stepObj.ToString() ?? throw new InvalidOperationException()) * -1;
        
        if(EditableFile == null)
            return;
        
        var maxIndex = EditableFile.DataList.Count - 1;

        var tempIndex = CurrentIndex + step;
        if(tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if(tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    public virtual void GoNext(object stepObj)
    {
        var step = int.Parse(stepObj.ToString() ?? throw new InvalidOperationException());
        
        if(EditableFile == null)
            return;
        
        var maxIndex = EditableFile.DataList.Count - 1;

        var tempIndex = CurrentIndex + step;
        if(tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if(tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        
        
        UpdateCurrentItem();
        UpdateReferenceItem(); 
        UpdateNavigation();
    }

    partial void OnNavigationTextChanged(string value)
    {
        if (int.TryParse(value, out var userIndex) && userIndex > 0)
        {
            GoCustom();
        }
    }
    public virtual void GoCustom()
    {
        var index = int.Parse(NavigationText.ToString() ?? throw new InvalidOperationException()) - 1;
        var sanitizedStep = index;
        if(index < 0) index = 0;
        if(index > EditableFile.DataList.Count - 1) index = EditableFile.DataList.Count;
        
        if (CurrentIndex != index)
        {
            CurrentIndex = index;
            UpdateCurrentItem();
            UpdateReferenceItem();
            UpdateNavigation();
        }
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
        NavigationText = $"{CurrentIndex + 1}";
        NavigationCountText = $" / {EditableFile?.DataList.Count ?? 0}";
    }

    public virtual void SaveCurrentFile(RepositoryManager repositoryManager)
    {
        if(EditableFile == null)
            return;
        repositoryManager.SaveObjectToFile(EditableFile);
    }
}