using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class UnlockCode : LocalizationItemBase
{
    [JsonProperty("openCondition")]
    public string? OpenCondition { get; set; } = string.Empty;
}