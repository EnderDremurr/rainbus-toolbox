using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public abstract class LocalizationItemBase
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
}