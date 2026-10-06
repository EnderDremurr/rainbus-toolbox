using Avalonia.Controls;
using RainbusToolbox.Utilities;

namespace RainbusToolbox.Views.Misc;

public partial class RichTextPreviewWindow : Window
{
    public RichTextPreviewWindow()
    {
        InitializeComponent();
    }

    public void SetTextToDisplay(string text)
    {
        var inlines = TextMarkupProcessor.ConvertRawToRich(text);

        foreach (var inline in inlines) TextBlock.Inlines?.Add(inline);
    }
}