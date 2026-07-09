using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class StagePart : LocalizationItemBase
{
    [JsonProperty("parttitle")]
    public string? PartTitle { get; set; }
}
