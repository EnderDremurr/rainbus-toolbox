using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class Skill : LocalizationItemBase
{
    [JsonProperty("levelList")]
    public List<SkillLevel> LevelList { get; set; } = [];
}