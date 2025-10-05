using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class PassiveTranslationEditor : UserControl, IFileEditor
    {
        public PassiveTranslationEditorViewModel VM => (PassiveTranslationEditorViewModel)DataContext!;

        public PassiveTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new PassiveTranslationEditorViewModel();
        }

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((PassivesFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((PassivesFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }
}