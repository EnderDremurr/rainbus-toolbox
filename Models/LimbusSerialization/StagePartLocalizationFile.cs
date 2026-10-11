using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class StagePartLocalizationFile : LocalizationFileBase, ILocalizationContainer<StagePart>
{
    [JsonProperty("dataList")]
    public List<StagePart> DataList { get; set; } = [];
}