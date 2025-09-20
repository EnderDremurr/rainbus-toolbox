using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

//Ego files are a bit different
[FilePattern("Skills_Ego*")]

public class SkillsEgoFile : LocalizationFileBase, ILocalizationContainer<SkillEgo>
{
    [JsonProperty("dataList")]
    public List<SkillEgo> DataList { get; set; }
}

public class SkillEgo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("levelList")]
    public List<SkillEgoLevel> LevelList { get; set; }
}

public class SkillEgoLevel
{
    [JsonProperty("abName")]
    public string AbnormalityName { get; set; } = string.Empty;
    
    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonProperty("coinlist")]
    public List<CoinListItem> CoinList { get; set; } = new List<CoinListItem>();
}