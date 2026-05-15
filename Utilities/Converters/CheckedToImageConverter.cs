using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RainbusToolbox;

public class CheckedToImageConverter : IValueConverter
{
    private static readonly Bitmap Checked =
        new(AssetLoader.Open(new Uri("avares://RainbusToolbox/Assets/Buttons/Checked.png")));

    private static readonly Bitmap Unchecked =
        new(AssetLoader.Open(new Uri("avares://RainbusToolbox/Assets/Buttons/Unchecked.png")));

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true)
            return Checked;
        return Unchecked;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}