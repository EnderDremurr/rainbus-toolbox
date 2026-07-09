using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class ScenarioModelCodesLocalizationFile : LocalizationFileBase, ILocalizationContainer<ScenarioModelCode>
{
    [JsonProperty("dataList")]
    public List<ScenarioModelCode> DataList { get; set; } = [];
}
