using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class EgoGiftSimpleDesc : LocalizationItemBase
{
    [JsonProperty("simpleDesc")]
    public string? SimpleDesc { get; set; }
}