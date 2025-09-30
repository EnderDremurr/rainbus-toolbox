using Avalonia;
using Avalonia.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using RainbusToolbox.Services;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Utilities
{
    public static class ContextMenuHelper
    {
        public static readonly AttachedProperty<bool> EnableTextPreviewProperty =
            AvaloniaProperty.RegisterAttached<Control, bool>("EnableTextPreview", typeof(ContextMenuHelper));
            
        public static void SetEnableTextPreview(Control element, bool value) =>
            element.SetValue(EnableTextPreviewProperty, value);
            
        public static bool GetEnableTextPreview(Control element) =>
            element.GetValue(EnableTextPreviewProperty);
            
        static ContextMenuHelper()
        {
            EnableTextPreviewProperty.Changed.Subscribe(args =>
            {
                if (args.Sender is TextBox textBox && args.NewValue.Equals(true))
                {
                    SetupContextMenu(textBox);
                }
            });
        }
        
        private static void SetupContextMenu(TextBox textBox)
        {
            var contextMenu = new ContextMenu();

            // Preview menu item
            var previewItem = new MenuItem { Header = "Превью" };
            previewItem.Click += (s, e) =>
            {
                var text = textBox.Text ?? string.Empty;
                var previewWindow = new RichTextPreviewWindow();
                previewWindow.SetTextToDisplay(text);
                previewWindow.Show();
            };
            contextMenu.Items.Add(previewItem);

            // Process with Angela menu item
            var angelaItem = new MenuItem { Header = "Process with Angela" };
            angelaItem.Click += async (s, e) =>
            {
                await ProcessTextWithAngela(textBox);
            };
            contextMenu.Items.Add(angelaItem);

            textBox.ContextMenu = contextMenu;
        }

        private async static Task<string> ProcessTextWithAngela(object parameter)
        {
            string text = string.Empty;
            TextBox textBox = null;
            
            Console.WriteLine("Received Angela command.");
            
            if (parameter is TextBox tb)
            {
                textBox = tb;
                text = textBox.Text ?? string.Empty;
            }
            else
            {
                var textProperty = parameter.GetType().GetProperty("Text");
                if (textProperty != null)
                {
                    text = textProperty.GetValue(parameter)?.ToString() ?? string.Empty;
                }
            }

            var angela = App.Current.ServiceProvider.GetService(typeof(Angela)) as Angela;
            var response = await angela?.ProcessText(string.IsNullOrEmpty(text) ? "" : text);

            if (textBox != null)
            {
                Console.WriteLine("Setting new text");
                textBox.Text = string.IsNullOrEmpty(response) ? text : response;
            }

            return response ?? text;
        }
    }
}
