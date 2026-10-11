using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class IntroductuceCharacter : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string? Desc { get; set; } = string.Empty;

    [JsonProperty("sentence")]
    public string? Sentence { get; set; } = string.Empty;
}