using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class BattleAnnouncerTranslationEditor : UserControl, IFileEditor
{
    public BattleAnnouncerTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new BattleAnnouncerTranslationEditorViewModel();
    }

    public BattleAnnouncerTranslationEditorViewModel VM => (BattleAnnouncerTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((AnnouncerVoiceLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((AnnouncerVoiceLocalizationFile)file);
    }

    public void AskEditorToSave(RepositoryManager repositoryManager)
    {
        VM.SaveCurrentFile(repositoryManager);
    }
}