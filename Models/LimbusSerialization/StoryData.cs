using System.Collections.Generic;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Utilities.Data;

[FilePattern("StoryData/*")]
public class StoryDataFile : LocalizationFileBase, ILocalizationContainer<StoryDataItem>
{
    [JsonProperty("dataList")]
    public List<StoryDataItem> DataList { get; set; }
}

public class StoryDataItem
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("model")]
    public string? Model { get; set; } // Optional

    [JsonProperty("teller")]
    public string? Teller { get; set; } // Optional

    [JsonProperty("title")]
    public string? Title { get; set; } // Optional

    [JsonProperty("place")]
    public string? Place { get; set; } // Optional

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty; // Always present
}