using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace RainbusTools.Views
{
    public partial class InputDialog : Window
    {
        public string CommitMessage { get; private set; }

        public InputDialog()
        {
            InitializeComponent();
            this.AttachDevTools();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            CommitMessage = this.FindControl<TextBox>("CommitMessageTextBox").Text;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CommitMessage = null;
            this.Close();
        }
    }
}