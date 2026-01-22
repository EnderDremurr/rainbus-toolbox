using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class EGOVoiceTranslationEditor : UserControl, IFileEditor
    {
        public EGOVoiceTranslationEditorViewModel VM => (EGOVoiceTranslationEditorViewModel)DataContext!;

        public EGOVoiceTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new EGOVoiceTranslationEditorViewModel();
        }

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((EgoVoiceLocalizationFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((EgoVoiceLocalizationFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }
}