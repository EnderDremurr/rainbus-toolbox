using Avalonia;
using Avalonia.Controls;
using System;
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
            var menuItem = new MenuItem { Header = "Превью" };
            
            menuItem.Click += (s, e) => 
            {
                var text = textBox.Text ?? string.Empty;
                var previewWindow = new RichTextPreviewWindow();
                previewWindow.SetTextToDisplay(text);
                previewWindow.Show();
            };
            
            contextMenu.Items.Add(menuItem);
            textBox.ContextMenu = contextMenu;
        }
    }
}