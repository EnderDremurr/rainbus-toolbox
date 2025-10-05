using Avalonia.Controls;
using Avalonia.Interactivity;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class StoryTranslationEditor : UserControl, IFileEditor
{
    public StoryTranslationEditorViewModel VM => (StoryTranslationEditorViewModel)DataContext!;

    public StoryTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new StoryTranslationEditorViewModel();
    }

    public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((StoryDataFile)file);

    public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((StoryDataFile)file);
    public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
}