using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class StoryTranslationEditor : UserControl, IFileEditor
{
    public StoryTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new StoryTranslationEditorViewModel();
    }

    public StoryTranslationEditorViewModel VM => (StoryTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((StoryDataFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((StoryDataFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}