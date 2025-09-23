using System;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Utilities
{
    public static class GlobalCommandHandler
    {
        public static ICommand CreatePreviewCommand { get; } =
            new RelayCommand<string>(CreatePreviewOfText);

        private static void CreatePreviewOfText(object parameter)
        {
            string debugInfo = $"Parameter type: {parameter?.GetType()?.Name ?? "null"}\n";
            debugInfo += $"Parameter value: {parameter}\n";

            string text = string.Empty;

            if (parameter is TextBox textBox)
            {
                text = textBox.Text ?? string.Empty;
                debugInfo += $"TextBox text: '{text}'\n";
            }
            else if (parameter != null)
            {
                // Try to get Text property via reflection as fallback
                var textProperty = parameter.GetType().GetProperty("Text");
                if (textProperty != null)
                {
                    text = textProperty.GetValue(parameter)?.ToString() ?? string.Empty;
                    debugInfo += $"Text via reflection: '{text}'\n";
                }
                debugInfo += $"Available properties: {string.Join(", ", parameter.GetType().GetProperties().Select(p => p.Name))}\n";
            }
            else
            {
                debugInfo += "Parameter is null!\n";
            }

            var previewWindow = new RichTextPreviewWindow();
            previewWindow.SetTextToDisplay($"DEBUG INFO:\n{debugInfo}\n\nACTUAL TEXT:\n{text}");
            previewWindow.Show();
        }
    }

    // A very lightweight ICommand implementation
    public class RelayCommand<T>(Action<T> execute) : ICommand
    {
        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => execute((T)parameter!);

        public event EventHandler? CanExecuteChanged;
    }
}