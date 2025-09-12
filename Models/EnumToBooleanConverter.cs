using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace RainbusToolbox.Models
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (parameter is string enumString && value != null)
            {
                return value.ToString() == enumString;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string enumString)
            {
                // ConvertBack must return the enum value
                return Enum.Parse(targetType, enumString);
            }

            // Important: return BindingOperations.DoNothing only if unchecked
            return Avalonia.Data.BindingOperations.DoNothing;
        }
    }
}