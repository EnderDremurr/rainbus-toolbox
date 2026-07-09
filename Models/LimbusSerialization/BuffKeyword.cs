using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class BuffKeyword : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("summary")]
    public string? Summary { get; set; }

    [JsonProperty("flavor")]
    public string? Flavor { get; set; }
}
