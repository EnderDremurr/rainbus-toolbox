using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

[FilePattern("Bufs*")] // bufs with one f!!! (its like that in game)
public class BuffsFile : LocalizationFileBase, ILocalizationContainer<Buff>
{
    [JsonProperty("dataList")]
    public List<Buff> DataList { get; set; }
}

public class Buff
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonProperty("undefined")]
    public string Undefined { get; set; } = string.Empty;
}