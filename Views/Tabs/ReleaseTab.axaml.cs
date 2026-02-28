using System;
using System.Linq;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RainbusToolbox.ViewModels;

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
