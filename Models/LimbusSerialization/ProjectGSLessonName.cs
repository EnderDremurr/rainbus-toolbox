using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ProjectGSLessonName : LocalizationItemBase
{
    [JsonProperty("content")]
    public string? Content { get; set; }

    [JsonProperty("teacher")]
    public string? Teacher { get; set; }
}