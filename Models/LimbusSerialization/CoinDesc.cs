using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class CoinDesc
{
    [JsonProperty("desc")]
    public string? Desc { get; set; }
}
