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
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute) => _execute = execute;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute((T)parameter);

        public event EventHandler CanExecuteChanged;
    }
}