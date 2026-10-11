using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class MentalCondition : LocalizationItemBase
{
    [JsonProperty("add")]
    public string? Add { get; set; } = string.Empty;

    [JsonProperty("min")]
    public string? Min { get; set; } = string.Empty;
}