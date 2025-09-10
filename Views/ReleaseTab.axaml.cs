using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace RainbusTools.Views;

public partial class ReleaseTab : UserControl
{
    public ReleaseTab()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Make entire title bar draggable
        
    private void VersionTextBox_OnTextInput(object sender, Avalonia.Input.TextInputEventArgs e)
    {
        // Allow only digits and dot
        if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9.]$"))
        {
            e.Handled = true;
        }
    }
}