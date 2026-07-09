using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class AbnormalityGuide : LocalizationItemBase
{
    [JsonProperty("codeName")]
    public string? CodeName { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("clue")]
    public string? Clue { get; set; }

    [JsonProperty("storyList")]
    public List<AbnormalityStory?> StoryList { get; set; } = [];
}
