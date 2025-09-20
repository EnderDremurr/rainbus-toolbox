using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
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

        private void OnPreviousClick(object? sender, RoutedEventArgs e) => VM.GoPrevious();

        private void OnNextClick(object? sender, RoutedEventArgs e) => VM.GoNext();

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((PersonalityVoiceFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((PersonalityVoiceFile)file);
    }
}