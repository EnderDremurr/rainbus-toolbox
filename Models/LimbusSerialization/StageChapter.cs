using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class StageChapter : LocalizationItemBase
{
    [JsonProperty("company")]
    public string? Company { get; set; }

    [JsonProperty("area")]
    public string? Area { get; set; }

    [JsonProperty("chapter")]
    public string? Chapter { get; set; }

    [JsonProperty("chapterNumber")]
    public string? ChapterNumber { get; set; }

    [JsonProperty("chaptertitle")]
    public string? ChapterTitle { get; set; }

    [JsonProperty("timeline")]
    public string? Timeline { get; set; }
}
