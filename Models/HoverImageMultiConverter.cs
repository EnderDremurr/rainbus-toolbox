using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace RainbusTools.Models
{
    public class HoverImageMultiConverter : IMultiValueConverter
    {
        // values[0] = IsPointerOver (bool)
        // values[1] = Normal image URI (string)
        // values[2] = Hover image URI (string)
        
        public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count != 3)
                return null;

            bool isPointerOver = values[0] is bool b && b;
            string normalPath = values[1] as string;
            string hoverPath = values[2] as string;

            string pathToUse = isPointerOver ? hoverPath : normalPath;

            if (string.IsNullOrEmpty(pathToUse))
                return null;

            try
            {
                return new Bitmap(pathToUse); // convert string to IImage
            }
            catch
            {
                return null;
            }
        }
        

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}