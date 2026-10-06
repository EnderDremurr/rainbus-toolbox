using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class EGOVoiceTranslationEditor : UserControl, IFileEditor
{
    public EGOVoiceTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new EGOVoiceTranslationEditorViewModel();
    }

    public EGOVoiceTranslationEditorViewModel VM => (EGOVoiceTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((EgoVoiceLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((EgoVoiceLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}