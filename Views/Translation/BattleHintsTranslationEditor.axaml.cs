using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views;

public partial class BattleHintsTranslationEditor : UserControl, IFileEditor
{
    public BattleHintsEditorViewModel VM => (BattleHintsEditorViewModel)DataContext!;

    public BattleHintsTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new BattleHintsEditorViewModel();
    }
    public void SetFileToEdit(LocalizationFileBase file) => VM.LoadEditableFile((BattleHintsFile)file);

    public void SetReferenceFile(LocalizationFileBase file) => VM.LoadReferenceFile((BattleHintsFile)file);
}
