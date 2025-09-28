using Avalonia.Controls;
using Avalonia.Interactivity;
using System.ComponentModel;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class BattleAnnouncerTranslationEditor : UserControl, IFileEditor
    {
        public BattleAnnouncerTranslationEditorViewModel VM => (BattleAnnouncerTranslationEditorViewModel)DataContext!;

        public BattleAnnouncerTranslationEditor()
        {
            InitializeComponent();
            DataContext ??= new BattleAnnouncerTranslationEditorViewModel();
        }

        private void OnPreviousClick(object? sender, RoutedEventArgs e) => VM.GoPrevious();

        private void OnNextClick(object? sender, RoutedEventArgs e) => VM.GoNext();

        public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((BattleAnnouncerFile)file);

        public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((BattleAnnouncerFile)file);
        public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
        
        private void OnBackgroundSizeChanged(object? sender, Avalonia.Controls.SizeChangedEventArgs e)
        {
            if (DataContext is BattleAnnouncerTranslationEditorViewModel vm)
            {
                // Use the new rendered size reported by the Image control.
                vm.CurrentImageWidth = e.NewSize.Width;
                vm.CurrentImageHeight = e.NewSize.Height;
            }
        }
    }
}