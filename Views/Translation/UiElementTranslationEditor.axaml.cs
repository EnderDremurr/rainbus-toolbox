using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;


namespace RainbusToolbox.Views;

public partial class UiElementTranslationEditor : UserControl, IFileEditor
{
    public UiElementTranslationEditorViewModel VM => (UiElementTranslationEditorViewModel)DataContext!;

    public UiElementTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new UiElementTranslationEditorViewModel();
    }

    public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((UiLocalizationFile)file);

    public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((UiLocalizationFile)file);
    public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
}