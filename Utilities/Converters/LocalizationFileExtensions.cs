using System.Text.RegularExpressions;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Utilities.Converters;

public static class LocalizationFileExtensions
{
    public static string GetSanityName(this LocalizationFileBase file)
    {
        var typeName = file.GetType().Name;
        
        var stripped = typeName.Replace("LocalizationFile", "");
        
        return Regex.Replace(stripped, "(?<!^)([A-Z])", " $1").Trim();
    }
}