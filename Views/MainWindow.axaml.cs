using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RainbusToolbox.Views
{
    public partial class MainWindow : Window
    {
        private Image? _bg;
        private TabControl? _mainTabs;

        public MainWindow()
        {
            InitializeComponent();

            this.Opened += (_, _) =>
            {
                _bg = this.FindControl<Image>("Bg");
                _mainTabs = this.FindControl<TabControl>("MainTabs");
            
                Console.WriteLine($"Found Bg: {_bg != null}");
                Console.WriteLine($"Found MainTabs: {_mainTabs != null}");

                if (_bg != null)
                {
                    var uri = new Uri("avares://RainbusToolbox/Assets/TranslationBG.png");
                    using var stream = AssetLoader.Open(uri);
                    _bg.Source = new Bitmap(stream);
                }

                if (_mainTabs != null)
                {
                    _mainTabs.SelectionChanged += MainTabs_OnSelectionChanged;
                }
            };
        }

        private void MainTabs_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_bg == null || _mainTabs == null)
                return;

            string uri = _mainTabs.SelectedIndex switch
            {
                0 => "avares://RainbusToolbox/Assets/TranslationBG.png",
                1 => "avares://RainbusToolbox/Assets/ReleaseBG.png",
                2 => "avares://RainbusToolbox/Assets/FilesBG.png",
                _ => "avares://RainbusToolbox/Assets/TranslationBG.png"
            };

            using var stream = AssetLoader.Open(new Uri(uri));
            _bg.Source = new Bitmap(stream);
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }

        // Make entire title bar draggable
        private void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                BeginMoveDrag(e);
            }
        }
        

    }
}