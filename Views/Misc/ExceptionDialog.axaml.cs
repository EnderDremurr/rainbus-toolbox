using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace RainbusToolbox;

public partial class ExceptionDialog : Window
{
    private readonly string _errorText;

    public ExceptionDialog(string errorText)
    {
        InitializeComponent();
        _errorText = errorText;
        
        // Set the error text
        var errorTextBox = this.FindControl<TextBox>("ErrorTextBox");
        if (errorTextBox != null)
        {
            errorTextBox.Text = _errorText;
        }
    }

    private async void OnCopyButtonClick(object sender, RoutedEventArgs e)
    {
        try
        {
            var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(_errorText);
                
                // Provide visual feedback
                var button = sender as Button;
                if (button != null)
                {
                    var originalContent = button.Content;
                    button.Content = "Copied!";
                    button.IsEnabled = false;
                    
                    // Reset button after 2 seconds
                    _ = Task.Delay(2000).ContinueWith(_ =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            button.Content = originalContent;
                            button.IsEnabled = true;
                        });
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // If clipboard fails, show alternative
            var button = sender as Button;
            if (button != null)
            {
                button.Content = "Copy failed - select all text manually";
                
                // Select all text in the textbox for manual copying
                var errorTextBox = this.FindControl<TextBox>("ErrorTextBox");
                errorTextBox?.Focus();
                errorTextBox?.SelectAll();
            }
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // Allow Ctrl+C to copy the error text even when textbox doesn't have focus
        if (e.Key == Key.C && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            OnCopyButtonClick(this, new RoutedEventArgs());
            e.Handled = true;
            return;
        }
        
        // Allow Escape to close
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
            return;
        }

        base.OnKeyDown(e);
    }
}