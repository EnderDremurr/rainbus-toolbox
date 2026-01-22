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

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((PassiveLocalizationFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((PassiveLocalizationFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }
}