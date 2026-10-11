using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class StagePart : LocalizationItemBase
{
    [JsonProperty("parttitle")]
    public string? PartTitle { get; set; }
}