using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class DungeonNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageNode>
{
    [JsonProperty("dataList")]
    public List<StageNode> DataList { get; set; } = [];
}