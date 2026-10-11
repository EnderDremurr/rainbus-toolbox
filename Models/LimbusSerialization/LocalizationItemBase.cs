using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public abstract class LocalizationItemBase
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
}