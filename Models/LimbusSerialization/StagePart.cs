using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class StagePart : LocalizationItemBase
{
    [JsonProperty("parttitle")]
    public string? PartTitle { get; set; }
}
