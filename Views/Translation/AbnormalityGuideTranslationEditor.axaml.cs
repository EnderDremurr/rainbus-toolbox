using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class AbnormalityGuideTranslationEditor : UserControl, IFileEditor
{
    public AbnormalityGuideTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new AbnormalityGuideTranslationEditorViewModel();
    }

    public AbnormalityGuideTranslationEditorViewModel VM => (AbnormalityGuideTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((AbnormalityGuideContentLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((AbnormalityGuideContentLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}