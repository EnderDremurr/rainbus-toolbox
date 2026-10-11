using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ItemDesc : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("flavor")]
    public string? Flavor { get; set; }
}