using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using InitializationWindowViewModel = RainbusToolbox.ViewModels.Misc.InitializationWindowViewModel;

namespace RainbusToolbox.Views.Misc;

public partial class InitializationWindow : Window
{
    public InitializationWindow(InitializationWindowViewModel viewModel)
    {
        InitializeComponent();
        SetupBackgroundSize();

        Closing += (_, __) => viewModel.SaveCommand.Execute(null);
    }

    private void SetupBackgroundSize()
    {
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://RainbusToolbox/Assets/Backgrounds/Init.png")));
        Width = bitmap.PixelSize.Width / 1.5f;
        Height = bitmap.PixelSize.Height / 1.5f;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}