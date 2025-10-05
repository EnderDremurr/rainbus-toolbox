using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class PersonalityVoiceTranslationEditor : UserControl, IFileEditor
    {
        public PersonalityVoiceTranslationEditorViewModel VM => (PersonalityVoiceTranslationEditorViewModel)DataContext!;

        public PersonalityVoiceTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new PersonalityVoiceTranslationEditorViewModel();
        }

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((PersonalityVoiceFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((PersonalityVoiceFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
    }
}