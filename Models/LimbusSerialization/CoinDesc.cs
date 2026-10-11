using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class CoinDesc
{
    [JsonProperty("desc")]
    public string? Desc { get; set; }
}
