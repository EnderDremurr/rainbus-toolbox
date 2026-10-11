using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class UnlockCode : LocalizationItemBase
{
    [JsonProperty("openCondition")]
    public string? OpenCondition { get; set; } = string.Empty;
}
