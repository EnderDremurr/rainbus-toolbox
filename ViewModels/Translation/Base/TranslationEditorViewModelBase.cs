using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models;
using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.Services.RepositoryServices;

namespace RainbusToolbox.ViewModels.Translation.Base;

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