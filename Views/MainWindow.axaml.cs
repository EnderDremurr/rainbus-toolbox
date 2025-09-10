using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace RainbusToolbox.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
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
                this.BeginMoveDrag(e);
            }
        }
        
        private void VersionTextBox_OnTextInput(object sender, Avalonia.Input.TextInputEventArgs e)
        {
            // Allow only digits and dot
            if (!System.Text.RegularExpressions.Regex.IsMatch(e.Text, @"^[0-9.]$"))
            {
                e.Handled = true;
            }
        }

    }
}