using Avalonia.Controls;
using Avalonia.Interactivity;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class SkillsEgoTranslationEditor : UserControl, IFileEditor
{
    public SkillsEgoTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new SkillsEgoTranslationEditorViewModel();
    }

    public SkillsEgoTranslationEditorViewModel VM => (SkillsEgoTranslationEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((SkillLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((SkillLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }

    private void OnPreviousLevelClick(object? sender, RoutedEventArgs e)
    {
        VM.GoPreviousLevel();
    }

    private void OnNextLevelClick(object? sender, RoutedEventArgs e)
    {
        VM.GoNextLevel();
    }
}