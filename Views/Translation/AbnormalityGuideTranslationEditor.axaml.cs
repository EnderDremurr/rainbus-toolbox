using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class AbnormalityGuideTranslationEditor : UserControl, IFileEditor
    {
        public AbnormalityGuideTranslationEditorViewModel VM => (AbnormalityGuideTranslationEditorViewModel)DataContext!;

        public AbnormalityGuideTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new AbnormalityGuideTranslationEditorViewModel();
        }

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((AbnormalityGuideFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((AbnormalityGuideFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }
}