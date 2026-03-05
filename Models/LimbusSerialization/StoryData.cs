using System.Collections.Generic;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Utilities.Data;

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
    public string? Model { get; set; }

    [JsonProperty("teller")]
    public string? Teller { get; set; }

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("place")]
    public string? Place { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty;
}