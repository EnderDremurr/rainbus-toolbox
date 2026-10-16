using Avalonia.Controls;
using RegexEditorViewModel = RainbusToolbox.ViewModels.Misc.RegexEditorViewModel;

namespace RainbusToolbox.Views.Misc;

public partial class RegexEditor : UserControl
{
    public RegexEditor()
    {
        InitializeComponent();
    }

    public RegexEditor(RegexEditorViewModel vm) : this()
    {
        DataContext = vm;
    }
}