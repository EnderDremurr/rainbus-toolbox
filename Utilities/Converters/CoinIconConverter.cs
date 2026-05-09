using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RainbusToolbox.Utilities.Converters;

public class CoinIconConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int index) return null!;

        var path = index is >= 1 and <= 10
            ? $"avares://RainbusToolbox/Assets/Icons/CoinEffect{index}.png"
            : "avares://RainbusToolbox/Assets/Icons/Special_Coin.png";

        return new Bitmap(AssetLoader.Open(new Uri(path)));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}