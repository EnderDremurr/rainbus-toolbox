using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class EgoGiftSimpleDesc : LocalizationItemBase
{
    [JsonProperty("simpleDesc")]
    public string? SimpleDesc { get; set; }
}
