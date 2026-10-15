using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RainbusToolbox.ViewModels.Misc;

namespace RainbusToolbox.Views.Misc;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsWindowViewModel vm) : this()
    {
        DataContext = vm;
    }

    // Toolbar drag
    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    // Close button
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}