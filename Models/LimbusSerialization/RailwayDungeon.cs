using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class RailwayDungeon : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("longName")]
    public string? LongName { get; set; }
}
