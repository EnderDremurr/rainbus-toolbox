using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class AnnouncerVoice : LocalizationItemBase
{
    [JsonProperty("dlg")]
    public string? Dialogue { get; set; }
}
