using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace RainbusToolbox.Models;

public class HoverImageConverter : IValueConverter
{
    public string Normal { get; set; } = "";
    public string Hover { get; set; } = "";

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isHovered && isHovered)
            return Hover;
        return Normal;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}