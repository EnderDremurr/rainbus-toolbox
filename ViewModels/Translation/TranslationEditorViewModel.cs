using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class TranslationEditorViewModel<TFile, TItem> : ObservableObject where TFile : LocalizationFileBase, ILocalizationContainer<TItem>
{
    protected TFile? _editableFile;
    protected TFile? _referenceFile;
    protected int _currentIndex = 0;

    [ObservableProperty] protected bool _canGoPrevious;
    [ObservableProperty] protected bool _canGoNext;
    [ObservableProperty] protected string _navigationText = "";
    [ObservableProperty] protected TItem? _currentItem;
    [ObservableProperty] protected TItem? _referenceItem;

    public bool IsFileLoaded => _editableFile != null && _editableFile.DataList.Count > 0;

    public virtual void LoadEditableFile(TFile file)
    {
        _editableFile = file;
        _currentIndex = 0;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsFileLoaded));
    }

    public virtual void LoadReferenceFile(TFile file)
    {
        _referenceFile = file;
        UpdateReferenceItem();
    }

    public virtual void GoPrevious()
    {
        if (_currentIndex <= 0) return;
        _currentIndex--;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    public virtual void GoNext()
    {
        if (_editableFile == null || _currentIndex >= _editableFile.DataList.Count - 1) return;
        _currentIndex++;
        UpdateCurrentItem();
        UpdateReferenceItem(); 
        UpdateNavigation();
    }

    protected virtual void UpdateCurrentItem()
    {
        if (_editableFile != null && _editableFile.DataList.Count > 0)
            CurrentItem = _editableFile.DataList[_currentIndex];
    }

    // Add this new method
    protected virtual void UpdateReferenceItem()
    {
        if (_referenceFile != null && _referenceFile.DataList.Count > _currentIndex)
            ReferenceItem = _referenceFile.DataList[_currentIndex];
        else
            ReferenceItem = default;
    }

    protected virtual void UpdateNavigation()
    {
        CanGoPrevious = _currentIndex > 0;
        CanGoNext = _editableFile != null && _currentIndex < _editableFile.DataList.Count - 1;
        NavigationText = $"{_currentIndex + 1} / {_editableFile?.DataList.Count ?? 0}";
    }
}