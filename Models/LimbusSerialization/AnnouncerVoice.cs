using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class AnnouncerVoice : LocalizationItemBase
{
    [JsonProperty("dlg")]
    public string? Dialogue { get; set; }
}
