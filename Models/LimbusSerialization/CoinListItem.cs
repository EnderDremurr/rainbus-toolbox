using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class CoinListItem
{
    [JsonProperty("coindescs")]
    public List<CoinDesc> CoinDescs { get; set; } = [];
}
