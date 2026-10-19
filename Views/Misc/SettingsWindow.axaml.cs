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

    private void OnIdTextChanging(object? sender, TextChangingEventArgs e)
    {
        var box = (TextBox)sender!;
        var text = box.Text ?? "";
        if (text.All(c => c is >= '0' and <= '9')) return;

        var caret = box.CaretIndex;
        box.Text = new string(text.Where(c => c is >= '0' and <= '9').ToArray());
        box.CaretIndex = Math.Min(caret, box.Text.Length);
    }
}