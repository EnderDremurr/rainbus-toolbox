using System;
using System.Windows.Input;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Utilities
{
    public static class GlobalCommandHandler
    {
        public static ICommand CreatePreviewCommand { get; } =
            new RelayCommand<string>(CreatePreviewOfText);

        private static void CreatePreviewOfText(string text)
        {
            var previewWindow = new RichTextPreviewWindow();
            previewWindow.SetTextToDisplay(text);
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