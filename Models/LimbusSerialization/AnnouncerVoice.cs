using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class AnnouncerVoice : LocalizationItemBase
{
    [JsonProperty("dlg")]
    public string? Dialogue { get; set; }
}