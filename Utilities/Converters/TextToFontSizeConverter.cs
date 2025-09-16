using System;
using System.Globalization;
using Avalonia.Data.Converters;

public class TextToFontSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string text) return 36.0;
        
        int length = text.Length;
        return length switch
        {
            <= 15 => 36.0,   // Short names - large font
            <= 25 => 30.0,   // Medium names - medium font  
            <= 35 => 24.0,   // Long names - smaller font
            <= 45 => 20.0,   // Very long names - small font
            _ => 16.0         // Extremely long names - tiny font
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}