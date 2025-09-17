using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.ViewModels;

public partial class PanicTranslationEditorViewModel : ObservableObject
{
    private PanicInfoFile? _editableFile;
    private PanicInfoFile? _referenceFile;
    private int _currentIndex = 0;

    [ObservableProperty] private bool _canGoPrevious;
    [ObservableProperty] private bool _canGoNext;
    [ObservableProperty] private string _navigationText = "";
    [ObservableProperty] private PanicInfo? _currentItem;
    [ObservableProperty] private PanicInfo? _referenceItem;

    public bool IsStoryLoaded => _editableFile != null && _editableFile.DataList.Count > 0;

    public void LoadEditableFile(PanicInfoFile file)
    {
        _editableFile = file;
        _currentIndex = 0;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsStoryLoaded));
    }

    public void LoadReferenceFile(PanicInfoFile file)
    {
        _referenceFile = file;
        UpdateReferenceItem();
    }

    public void GoPrevious()
    {
        if (_currentIndex <= 0) return;
        _currentIndex--;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
    }

    public void GoNext()
    {
        if (_editableFile == null || _currentIndex >= _editableFile.DataList.Count - 1) return;
        _currentIndex++;
        UpdateCurrentItem();
        UpdateReferenceItem(); 
        UpdateNavigation();
    }

    private void UpdateCurrentItem()
    {
        if (_editableFile != null && _editableFile.DataList.Count > 0)
            CurrentItem = _editableFile.DataList[_currentIndex];
    }

    // Add this new method
    private void UpdateReferenceItem()
    {
        if (_referenceFile != null && _referenceFile.DataList.Count > _currentIndex)
            ReferenceItem = _referenceFile.DataList[_currentIndex];
        else
            ReferenceItem = null;
    }

    private void UpdateNavigation()
    {
        CanGoPrevious = _currentIndex > 0;
        CanGoNext = _editableFile != null && _currentIndex < _editableFile.DataList.Count - 1;
        NavigationText = $"{_currentIndex + 1} / {_editableFile?.DataList.Count ?? 0}";
    }
}