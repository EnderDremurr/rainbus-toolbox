using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

//BattleKeywords* (Battle terminology)
public class BattleKeywordFile
{
    [JsonProperty("dataList")]
    public List<BattleKeyword> DataList { get; set; }
}

public class BattleKeyword
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Description { get; set; } = string.Empty;

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("undefined")]
    public string Undefined { get; set; } = "-";
}
//TODO: Implement editor