using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
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

        private void OnPreviousClick(object? sender, RoutedEventArgs e) => VM.GoPrevious();

        private void OnNextClick(object? sender, RoutedEventArgs e) => VM.GoNext();

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((PassivesFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((PassivesFile)file);
    }
}