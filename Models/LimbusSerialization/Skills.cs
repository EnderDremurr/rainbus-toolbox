namespace RainbusToolbox.Utilities.Data;

using System.Collections.Generic;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

[FilePattern("Skills*")]
public class SkillsFile : LocalizationFileBase, ILocalizationContainer<Skill>
{
    [JsonProperty("dataList")]
    public List<Skill> DataList { get; set; }
}

public class Skill
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("levelList")]
    public List<SkillLevel> LevelList { get; set; }
}

public class SkillLevel
{
    [JsonProperty("level")]
    public int Level { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;

    [JsonProperty("coinlist")]
    public List<CoinListItem> CoinList { get; set; } = new List<CoinListItem>();
}

public class CoinListItem
{
    [JsonProperty("coindescs")]
    public List<CoinDesc> CoinDescs { get; set; } = new List<CoinDesc>();
}

public class CoinDesc
{
    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;
}


