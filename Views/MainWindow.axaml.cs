using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RainbusToolbox.ViewModels;

namespace RainbusToolbox.Views
{
    public partial class MainWindow : Window
    {
        private Image? _bg;
        private TabControl? _mainTabs;

        public MainWindow()
        {
            InitializeComponent();

            Opened += (_, _) =>
            {
                _bg = this.FindControl<Image>("Bg");
                _mainTabs = this.FindControl<TabControl>("MainTabs");

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
            
            string uri = _mainTabs!.SelectedIndex switch
            {
                0 => "avares://RainbusToolbox/Assets/TranslationBG.png",
                1 => "avares://RainbusToolbox/Assets/ReleaseBG.png",
                2 => "avares://RainbusToolbox/Assets/FilesBG.png",
                _ => "avares://RainbusToolbox/Assets/TranslationBG.png"
            };

            using var stream = AssetLoader.Open(new Uri(uri));
            _bg.Source = new Bitmap(stream);
            
            switch (_mainTabs.SelectedIndex)
            {
                case 0:
                    var t0 = (_mainTabs.Items[0] as TabItem)?.Content as TranslationTab;
                    (t0?.DataContext as TranslationTabViewModel)?.OnTabOpened();
                    break;
                case 1:
                    var t1 = (_mainTabs.Items[1] as TabItem)?.Content as ReleaseTab;
                    (t1?.DataContext as ReleaseTabViewModel)?.OnTabOpened();
                    break;
                case 2:
                    var t2 = (_mainTabs.Items[2] as TabItem)?.Content as FilesTab;
                    (t2?.DataContext as FilesTabViewModel)?.OnTabOpened();
                    break;
            }
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