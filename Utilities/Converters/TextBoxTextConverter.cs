using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

public class TextBoxTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Debug what you're actually getting
        System.Diagnostics.Debug.WriteLine($"Converter received: {value?.GetType().Name}");
       
        if (value is TextBox textBox)
        {
            System.Diagnostics.Debug.WriteLine($"TextBox.Text: '{textBox.Text}'");
            return textBox.Text ?? "NULL_TEXT";
        }
       
        System.Diagnostics.Debug.WriteLine("Value is not a TextBox");
        return "NOT_TEXTBOX";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}