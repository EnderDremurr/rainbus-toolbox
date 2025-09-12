using System;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace RainbusToolbox.Views
{
    public partial class ReleaseTab : UserControl
    {
        public ReleaseTab()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Make entire title bar draggable
        private void VersionTextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = textBox.Text ?? string.Empty;
                if (text.EndsWith("."))
                {
                    textBox.Text = text.TrimEnd('.');
                }
            }
        }

        private void VersionTextBox_OnTextChanging(object sender, TextChangingEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Use Dispatcher to validate after the text change is applied
                Dispatcher.UIThread.Post(() =>
                {
                    string currentText = textBox.Text ?? string.Empty;
                    string validText = ValidateAndCleanVersionString(currentText);

                    if (currentText != validText)
                    {
                        // Temporarily disable the event to prevent recursion
                        textBox.TextChanging -= VersionTextBox_OnTextChanging;

                        // Store cursor position
                        int cursorPos = Math.Min(textBox.SelectionStart, validText.Length);

                        // Set the cleaned text
                        textBox.Text = validText;

                        // Restore cursor position
                        textBox.SelectionStart = cursorPos;
                        textBox.SelectionEnd = cursorPos;

                        // Re-enable the event
                        textBox.TextChanging += VersionTextBox_OnTextChanging;
                    }
                }, DispatcherPriority.Background);
            }
        }

        private string ValidateAndCleanVersionString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder validString = new StringBuilder();
            bool lastWasDot = false;

            // Process each character
            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    validString.Append(c);
                    lastWasDot = false;
                }
                else if (c == '.' && !lastWasDot && validString.Length > 0)
                {
                    validString.Append(c);
                    lastWasDot = true;
                }
            }

            return validString.ToString();
        }
    }
}
