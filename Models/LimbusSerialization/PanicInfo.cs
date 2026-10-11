using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class PanicInfo : LocalizationItemBase
{
    [JsonProperty("panicName")]
    public string PanicName { get; set; } = string.Empty;

    [JsonProperty("lowMoraleDescription")]
    public string LowMoraleDescription { get; set; } = string.Empty;

    [JsonProperty("panicDescription")]
    public string PanicDescription { get; set; } = string.Empty;
}