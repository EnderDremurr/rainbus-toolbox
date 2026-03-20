using System.Text;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RainbusToolbox.Views;

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


    private string ValidateAndCleanVersionString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var validString = new StringBuilder();
        var lastWasDot = false;

        // Process each character
        foreach (var c in input)
            if (char.IsDigit(c))
            {
                validString.Append(c);
                lastWasDot = false;
            }
            else if (c == '.' && !lastWasDot && validString.Length > 0)
            {
                validString.Append(c);
                lastWasDot = true;
            }

        return validString.ToString();
    }
}