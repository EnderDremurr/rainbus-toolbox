using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class PassiveTranslationEditor : UserControl, IFileEditor
{
    public PassiveTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new PassiveTranslationEditorViewModel();
    }

    public PassiveTranslationEditorViewModel VM => (PassiveTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((PassiveLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((PassiveLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}