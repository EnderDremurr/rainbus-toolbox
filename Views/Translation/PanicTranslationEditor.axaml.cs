using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class PanicTranslationEditor : UserControl, IFileEditor
{
    public PanicTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new PanicTranslationEditorViewModel();
    }

    public PanicTranslationEditorViewModel VM => (PanicTranslationEditorViewModel)DataContext!;


    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((PanicInfoLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((PanicInfoLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}