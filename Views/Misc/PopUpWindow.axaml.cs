using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views.Misc;

public partial class PopUpWindow : Window
{
    public PopUpWindow()
    {
        InitializeComponent();
    }

    // Toolbar drag
    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    public static async Task<PopUpWindowViewModel> ShowAsync(
        Window parent,
        string title,
        string message,
        bool hasInput = false,
        string inputPlaceholder = "",
        string? initialValue = null,
        params PopupButton[] buttons)
    {
        var popup = new PopUpWindow();
        var vm = new PopUpWindowViewModel
        {
            Title = title,
            Message = message,
            HasInput = hasInput,
            InputPlaceholder = inputPlaceholder,
            InputValue = initialValue ?? string.Empty
        };

        foreach (var btn in buttons)
        {
            btn.ClickCommand = new RelayCommand(() =>
            {
                vm.Result = btn.ResultValue;
                btn.OnClick?.Invoke();
                if (!btn.KeepOpen)
                    popup.Close();
            });
            vm.Buttons.Add(btn);
        }

        popup.DataContext = vm;
        await popup.ShowDialog(parent);
        return vm;
    }

    public static Task<PopUpWindowViewModel> ShowAsync(
        Window parent,
        string title,
        string message)
    {
        return ShowAsync(parent, title, message, false, "", null,
            new PopupButton { Label = "OK", ResultValue = "ok" }
        );
    }
}