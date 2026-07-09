using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class EventOption
{
    [JsonProperty("message")]
    public string? Message { get; set; }

    [JsonProperty("messageDesc")]
    public string? MessageDesc { get; set; }

    [JsonProperty("result")]
    public List<string>? Result { get; set; }
}
