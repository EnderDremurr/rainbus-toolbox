using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RainbusToolbox.ViewModels.Tabs;

namespace RainbusToolbox.Views;

public partial class MainWindow : Window
{
    private Image? _bg;
    private TabControl? _mainTabs;

    public MainWindow()
    {
        InitializeComponent();

        this.GetObservable(WindowStateProperty).Subscribe(state =>
        {
            var overlay = this.FindControl<Grid>("ResizeOverlay");
            if (overlay != null)
                overlay.IsHitTestVisible = state != WindowState.FullScreen;
        });

        Opened += (_, _) =>
        {
            _bg = this.FindControl<Image>("Bg");
            _mainTabs = this.FindControl<TabControl>("MainTabs");

            if (_bg != null)
            {
                var uri = new Uri("avares://RainbusToolbox/Assets/Backgrounds/TranslationBG.png");
                using var stream = AssetLoader.Open(uri);
                _bg.Source = new Bitmap(stream);
            }

            if (_mainTabs != null)
                _mainTabs.SelectionChanged += MainTabs_OnSelectionChanged;
        };
    }

    private void MainTabs_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_bg == null || _mainTabs == null)
            return;

        if (!Equals(e.Source, _mainTabs))
            return;

        var uri = _mainTabs!.SelectedIndex switch
        {
            0 => "avares://RainbusToolbox/Assets/Backgrounds/TranslationBG.png",
            4 => "avares://RainbusToolbox/Assets/Backgrounds/ReleaseBG.png",
            1 => "avares://RainbusToolbox/Assets/Backgrounds/FilesBG.png",
            _ => "avares://RainbusToolbox/Assets/Backgrounds/TranslationBG.png"
        };

        using var stream = AssetLoader.Open(new Uri(uri));
        _bg.Source = new Bitmap(stream);

        var content = (_mainTabs.SelectedItem as TabItem)?.Content as Control;
        if (content?.DataContext is not IMainWindowTab tab)
            return;

        try
        {
            tab.OnTabOpened();
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex);
        }
    }


    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Make entire title bar draggable
    private void TitleBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private void ResizeLeft(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.West, e);
    }

    private void ResizeRight(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.East, e);
    }

    private void ResizeTop(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.North, e);
    }

    private void ResizeBottom(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.South, e);
    }

    private void ResizeTopLeft(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.NorthWest, e);
    }

    private void ResizeTopRight(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.NorthEast, e);
    }

    private void ResizeBottomLeft(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.SouthWest, e);
    }

    private void ResizeBottomRight(object? sender, PointerPressedEventArgs e)
    {
        BeginResizeDrag(WindowEdge.SouthEast, e);
    }
}