using Avalonia.Controls;
using Avalonia.Interactivity;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class EGOGiftTranslationEditor : UserControl, IFileEditor
{
    public EGOGiftTranslationEditorViewModel VM => (EGOGiftTranslationEditorViewModel)DataContext!;

    public EGOGiftTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new EGOGiftTranslationEditorViewModel();
    }

    private void OnPreviousClick(object? sender, RoutedEventArgs e) => VM.GoPrevious();

    private void OnNextClick(object? sender, RoutedEventArgs e) => VM.GoNext();

    public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((EGOGiftFile)file);

    public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((EGOGiftFile)file);
    public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
}