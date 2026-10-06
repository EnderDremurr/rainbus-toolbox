using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class UiElementTranslationEditor : UserControl, IFileEditor
{
    public UiElementTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new UiElementTranslationEditorViewModel();
    }

    public UiElementTranslationEditorViewModel VM => (UiElementTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((UiLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((UiLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}