using System.ComponentModel;
using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class KeywordTranslationEditor : UserControl, INotifyPropertyChanged, IFileEditor
{
    public KeywordTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new KeywordTranslationEditorViewModel();
    }

    public KeywordTranslationEditorViewModel VM => (KeywordTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((KeywordLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((KeywordLocalizationFile)file);
    }

    public void AskEditorToSave(RepositoryManager repositoryManager)
    {
        VM.SaveCurrentFile(repositoryManager);
    }
}