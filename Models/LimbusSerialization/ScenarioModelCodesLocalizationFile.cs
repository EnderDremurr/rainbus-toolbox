using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ScenarioModelCodesLocalizationFile : LocalizationFileBase, ILocalizationContainer<ScenarioModelCode>
{
    [JsonProperty("dataList")]
    public List<ScenarioModelCode> DataList { get; set; } = [];
}