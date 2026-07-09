using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class StageNode : LocalizationItemBase
{
    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("place")]
    public string? Place { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}
