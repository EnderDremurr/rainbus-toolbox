using Avalonia;
using Avalonia.Controls;

namespace RainbusToolbox.Models;

public static class ButtonHoverProperties
{
    public static readonly AttachedProperty<string> TagNormalProperty =
        AvaloniaProperty.RegisterAttached<Button, Button, string>(
            "TagNormal");

    public static readonly AttachedProperty<string> TagHoverProperty =
        AvaloniaProperty.RegisterAttached<Button, Button, string>(
            "TagHover");

    public static string GetTagNormal(Button button)
    {
        return button.GetValue(TagNormalProperty);
    }

    public static void SetTagNormal(Button button, string value)
    {
        button.SetValue(TagNormalProperty, value);
    }

    public static string GetTagHover(Button button)
    {
        return button.GetValue(TagHoverProperty);
    }

    public static void SetTagHover(Button button, string value)
    {
        button.SetValue(TagHoverProperty, value);
    }
}