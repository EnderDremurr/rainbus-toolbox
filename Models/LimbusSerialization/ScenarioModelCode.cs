using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ScenarioModelCode : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; } = string.Empty;

    [JsonProperty("nickName")]
    public string? NickName { get; set; } = string.Empty;
}