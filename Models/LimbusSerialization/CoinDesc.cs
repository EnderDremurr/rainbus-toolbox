using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class CoinDesc
{
    [JsonProperty("desc")]
    public string? Desc { get; set; }
}