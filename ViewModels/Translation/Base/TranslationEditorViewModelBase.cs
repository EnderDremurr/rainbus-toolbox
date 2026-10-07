using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public abstract partial class TranslationEditorViewModelBase : ObservableObject, IFileEditor
{
    [ObservableProperty] protected string _navigationCountText = "";
    [ObservableProperty] protected string _navigationText = "";

    public int CurrentIndex { get; protected set; }
    public abstract int ItemCount { get; protected set; }

    public abstract void SetFileToEdit(LocalizationFileBase file);

    public abstract void SetReferenceFile(LocalizationFileBase file);

    public abstract void AskEditorToSave(LocalizationManager localizationManager);


    public abstract void OnIndexChanged();

    [RelayCommand]
    public abstract void StepIndex(int step);
}