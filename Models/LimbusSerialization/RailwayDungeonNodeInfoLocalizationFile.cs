using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class RailwayDungeonNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageNode>
{
    [JsonProperty("dataList")]
    public List<StageNode> DataList { get; set; } = [];
}
