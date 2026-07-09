using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class PersonalityDataEntry : LocalizationItemBase
{
    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("nameWithTitle")]
    public string? NameWithTitle { get; set; }

    [JsonProperty("desc")]
    public string? Description { get; set; }
}
