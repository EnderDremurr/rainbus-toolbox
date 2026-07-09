using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class StagePartLocalizationFile : LocalizationFileBase, ILocalizationContainer<StagePart>
{
    [JsonProperty("dataList")]
    public List<StagePart> DataList { get; set; } = [];
}
