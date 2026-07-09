using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class RailwayDungeon : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("longName")]
    public string? LongName { get; set; }
}
