using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class EgoGiftSimpleDesc : LocalizationItemBase
{
    [JsonProperty("simpleDesc")]
    public string? SimpleDesc { get; set; }
}
