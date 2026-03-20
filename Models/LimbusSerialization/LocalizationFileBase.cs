using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public abstract class LocalizationFileBase
{
    // Protected constructor for inheritance
    protected LocalizationFileBase(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        FullPath = filePath;
        PathTo = Path.GetDirectoryName(filePath) ?? string.Empty;
        FileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
    }

    // Parameterless constructor for JSON deserialization
    protected LocalizationFileBase()
    {
        // Will be populated later by deserializer
    }

    [JsonIgnore]
    public string PathTo { get; set; }

    [JsonIgnore]
    public string FileName { get; set; }

    [JsonIgnore]
    public string FullPath { get; set; }

    // Method to set path info after JSON deserialization
    internal void SetPathInfo(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

        FullPath = filePath;
        PathTo = Path.GetDirectoryName(filePath) ?? string.Empty;
        FileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
    }
}

public interface ILocalizationContainer<TItem>
{
    [JsonProperty("dataList")]
    public List<TItem> DataList { get; set; }
}