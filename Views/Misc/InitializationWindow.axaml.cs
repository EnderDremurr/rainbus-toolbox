using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Views;

// TODO: SUPER TODO, THIS SHIT SHOULD BE IN THE VIEW MODEL, NOT THE VIEW!!!!
public partial class InitializationWindow : Window
{
    public InitializationWindow(InitializationWindowViewModel viewModel)
    {
        InitializeComponent();
        SetupBackgroundSize();

        Closing += (_, __) => viewModel.SaveCommand.Execute(null);

        viewModel.RequestFolderPicker += async title =>
        {
            var pickedFolders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false
            });
            return pickedFolders.Count > 0 ? pickedFolders[0] : null;
        };

        viewModel.RequestGithubAuthPopup += async userCode =>
        {
            var clipboard = Clipboard;

            await PopUpWindow.ShowAsync(this, "Нужна авторизация",
                $"Проге нужен токен с GitHub.\nВведи этот код на открытой странице:\n\n{userCode}\n\nЗатем нажми ОК",
                false,
                "",
                null,
                new PopupButton
                {
                    Label = "Скопировать код",
                    ResultValue = "copy",
                    KeepOpen = true,
                    OnClick = () => clipboard?.SetTextAsync(userCode)
                },
                new PopupButton { Label = "OK", ResultValue = "ok" }
            );
        };
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

    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
}