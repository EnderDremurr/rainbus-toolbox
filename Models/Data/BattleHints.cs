using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class BattleHint
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
}

public class BattleHints
{
    [JsonProperty("dataList")]
    public List<BattleHint> DataList { get; set; } = new List<BattleHint>();
}