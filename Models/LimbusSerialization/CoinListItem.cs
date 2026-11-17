using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class CoinListItem
{
    [JsonProperty("coindescs")]
    public List<CoinDesc> CoinDescs { get; set; } = [];
}