using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class Skill : LocalizationItemBase
{
    [JsonProperty("levelList")]
    public List<SkillLevel> LevelList { get; set; } = [];
}
