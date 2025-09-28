using Avalonia.Controls;
using Avalonia.Interactivity;
using RainbusToolbox.Models.Data;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Views;

public partial class SkillsTranslationEditor : UserControl, IFileEditor
{
    public SkillsTranslationEditorViewModel VM => (SkillsTranslationEditorViewModel)DataContext!;

    public SkillsTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new SkillsTranslationEditorViewModel();
    }

    private void OnPreviousSkillClick(object? sender, RoutedEventArgs e) => VM.GoPreviousSkill();
    private void OnNextSkillClick(object? sender, RoutedEventArgs e) => VM.GoNextSkill();
    private void OnPreviousLevelClick(object? sender, RoutedEventArgs e) => VM.GoPreviousLevel();
    private void OnNextLevelClick(object? sender, RoutedEventArgs e) => VM.GoNextLevel();

    public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((SkillsFile)file);
    public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((SkillsFile)file);
    public void AskEditorToSave(RepositoryManager repositoryManager) => VM.SaveCurrentFile(repositoryManager);
}
