using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class SkillLocalizationFile : LocalizationFileBase, ILocalizationContainer<Skill>
{
    [JsonProperty("dataList")]
    public List<Skill> DataList { get; set; } = [];
}
