using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Views.Translation;

public partial class GenericTranslationEditor : UserControl, IFileEditor
{
    public GenericTranslationEditor()
    {
        InitializeComponent();
    }

    public void SetFileToEdit(LocalizationFileBase file)
    {
        throw new System.NotImplementedException();
    }

    public void SetReferenceFile(LocalizationFileBase file)
    {
        throw new System.NotImplementedException();
    }
}