using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ActionEvent : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("options")]
    public List<EventOption>? Options { get; set; }
}