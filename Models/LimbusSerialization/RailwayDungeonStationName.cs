using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class RailwayDungeonStationName : LocalizationItemBase
{
    [JsonProperty("content")]
    public string? Content { get; set; } = string.Empty;

    [JsonProperty("shortName")]
    public string? ShortName { get; set; } = string.Empty;
}
