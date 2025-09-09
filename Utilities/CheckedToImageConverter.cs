using System;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Platform;

namespace RainbusTools
{
    public class CheckedToImageConverter : IValueConverter
    {
        private static readonly Bitmap Checked = new Bitmap(AssetLoader.Open(new Uri("avares://RainbusTools/Assets/Checked.png")));
        private static readonly Bitmap Unchecked = new Bitmap(AssetLoader.Open(new Uri("avares://RainbusTools/Assets/Unchecked.png")));

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b && b)
                return Checked;
            return Unchecked;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
            => throw new NotImplementedException();
    }
}