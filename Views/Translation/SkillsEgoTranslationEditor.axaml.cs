using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using RainbusToolbox.Models.Data;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.Services;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Views;

public partial class SkillsEgoTranslationEditor : UserControl, IFileEditor
{
    public SkillsEgoTranslationEditorViewModel VM => (SkillsEgoTranslationEditorViewModel)DataContext!;

    public SkillsEgoTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new SkillsEgoTranslationEditorViewModel();
    }

    private void OnPreviousSkillClick(object? sender, RoutedEventArgs e) => VM.GoPreviousSkill();
    private void OnNextSkillClick(object? sender, RoutedEventArgs e) => VM.GoNextSkill();
    private void OnPreviousLevelClick(object? sender, RoutedEventArgs e) => VM.GoPreviousLevel();
    private void OnNextLevelClick(object? sender, RoutedEventArgs e) => VM.GoNextLevel();

    public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((SkillsEgoFile)file);
    public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((SkillsEgoFile)file);

    public void OnPreviewCurrentDescClick(object? sender, RoutedEventArgs e)
    {
        var previewWindow = new RichTextPreviewWindow();
        previewWindow.SetTextToDisplay(VM.CurrentLevel.Desc);
        previewWindow.Show();
    }

    public void OnPreviewCurrentCoinClick(object? sender, RoutedEventArgs e)
    {
        TextBox textBox;
        if (sender is Button button)
        {
            var parentGrid = button.Parent as Grid;
            textBox = parentGrid?.Children.OfType<TextBox>().FirstOrDefault();
        }
        else
        {
            return;
        }
        var previewWindow = new RichTextPreviewWindow();
        previewWindow.SetTextToDisplay(textBox.Text);
        previewWindow.Show();
        
        
    }
}