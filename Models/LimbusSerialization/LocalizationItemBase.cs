using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public abstract class LocalizationItemBase
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
}