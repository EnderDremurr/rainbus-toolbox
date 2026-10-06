using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class PersonalityVoiceTranslationEditor : UserControl, IFileEditor
{
    public PersonalityVoiceTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new PersonalityVoiceTranslationEditorViewModel();
    }

    public PersonalityVoiceTranslationEditorViewModel VM => (PersonalityVoiceTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((PersonalityVoiceLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((PersonalityVoiceLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}