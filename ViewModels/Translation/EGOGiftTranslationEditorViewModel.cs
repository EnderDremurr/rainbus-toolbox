using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class EGOGiftTranslationEditorViewModel : ObservableObject
{
    private EGOGiftFile? _editableFile;
    private EGOGiftFile? _referenceFile;
    private int _currentIndex = 0;

    [ObservableProperty] private bool _canGoPrevious;
    [ObservableProperty] private bool _canGoNext;
    [ObservableProperty] private string _navigationText = "";
    [ObservableProperty] private GenericIdNameDesc? _currentItem;
    [ObservableProperty] private GenericIdNameDesc? _referenceItem;

    public bool IsEGOGiftLoaded => _editableFile != null && _editableFile.DataList.Count > 0;

    public void LoadEditableFile(EGOGiftFile file)
    {
        _editableFile = file;
        _currentIndex = 0;
        UpdateCurrentItem();
        UpdateReferenceItem();
        UpdateNavigation();
        OnPropertyChanged(nameof(IsEGOGiftLoaded));
    }

    public void LoadReferenceFile(EGOGiftFile file)
    {
        _referenceFile = file;
        UpdateReferenceItem();
        OnPropertyChanged(nameof(ReferenceItem)); // Ensure UI updates
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

    private void UpdateReferenceItem()
    {
        if (_referenceFile != null && _currentIndex < _referenceFile.DataList.Count)
            ReferenceItem = _referenceFile.DataList[_currentIndex];
        else
            ReferenceItem = new GenericIdNameDesc { Name = "(no reference)", Desc = "" };
    }

    private void UpdateNavigation()
    {
        CanGoPrevious = _currentIndex > 0;
        CanGoNext = _editableFile != null && _currentIndex < _editableFile.DataList.Count - 1;
        NavigationText = $"{_currentIndex + 1} / {_editableFile?.DataList.Count ?? 0}";
    }
}