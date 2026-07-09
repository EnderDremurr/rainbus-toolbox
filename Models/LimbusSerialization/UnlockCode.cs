using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class UnlockCode : LocalizationItemBase
{
    [JsonProperty("openCondition")]
    public string? OpenCondition { get; set; } = string.Empty;
}
