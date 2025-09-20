using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

//AbnormalityGuides* (Abnormality descriptions)
[FilePattern("AbnormalityGuides*")]
public class AbnormalityGuideFile : LocalizationFileBase, ILocalizationContainer<AbnormalityGuide>
{
    [JsonProperty("dataList")]
    public List<AbnormalityGuide> DataList { get; set; }
}

public class AbnormalityGuide
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("codeName")]
    public string CodeName { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("clue")]
    public string Clue { get; set; } = string.Empty;

    [JsonProperty("storyList")]
    public List<AbnormalityStory> StoryList { get; set; }
}

public class AbnormalityStory
{
    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("story")]
    public string Story { get; set; } = string.Empty;
}
//TODO: Implement editor