using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class SkillLevel
{
    [JsonProperty("abName")]
    public string? AbnormalityName { get; set; }

    [JsonProperty("level")]
    public string? Level { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("flavor")]
    public string? Flavor { get; set; }

    [JsonProperty("coinlist")]
    public List<CoinListItem> CoinList { get; set; } = [];
}
