using Avalonia.Controls;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Views;

public partial class BattleHintsTranslationEditor : UserControl, IFileEditor
{
    public BattleHintsTranslationEditor()
    {
        InitializeComponent();
        DataContext ??= new BattleHintsEditorViewModel();
    }

    public BattleHintsEditorViewModel VM => (BattleHintsEditorViewModel)DataContext!;

    public void SetFileToEdit(LocalizationFileBase file)
    {
        VM.LoadEditableFile((NormalBattleHintLocalizationFile)file);
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        VM.LoadReferenceFile((NormalBattleHintLocalizationFile)file);
    }

    public void AskEditorToSave(LocalizationManager localizationManager)
    {
        VM.SaveCurrentFile(localizationManager);
    }
}