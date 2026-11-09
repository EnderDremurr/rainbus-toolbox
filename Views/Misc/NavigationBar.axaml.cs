using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace RainbusToolbox.Views.Misc;

public partial class NavigationBar : UserControl
{
    public NavigationBar()
    {
        AddHandler(PointerPressedEvent, OnAnyPointerPressed, RoutingStrategies.Tunnel);
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        TopLevel.GetTopLevel(this)?.AddHandler(PointerPressedEvent, OnAnyPointerPressed, RoutingStrategies.Tunnel);
    }

    private void OnAnyPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // don't kill focus when diddling textboxes
        if ((e.Source as Visual)?.FindAncestorOfType<TextBox>(true) != null)
            return; // also tbh this probably has a better way of doing but im stuwupid, maybe i'll know it later

        TopLevel.GetTopLevel(this)?.FocusManager?.ClearFocus(); // this shit is obsolete but lowkey works :sob:
    }
}