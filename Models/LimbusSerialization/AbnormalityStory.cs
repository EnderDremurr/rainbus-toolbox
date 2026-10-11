using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class AbnormalityStory
{
    [JsonProperty("level")]
    public string? Level { get; set; }

    [JsonProperty("story")]
    public string? Story { get; set; } = string.Empty;
}