using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class EGOGiftTranslationEditor : UserControl, IFileEditor
{
    public EGOGiftTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new EGOGiftTranslationEditorViewModel();
    }

    public EGOGiftTranslationEditorViewModel VM => (EGOGiftTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((EgoGiftsLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((EgoGiftsLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}