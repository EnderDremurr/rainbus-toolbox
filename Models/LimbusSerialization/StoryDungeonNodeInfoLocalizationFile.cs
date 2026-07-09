using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class StoryDungeonNodeInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<StoryDungeonNodeList>
{
    [JsonProperty("dataList")]
    public List<StoryDungeonNodeList> DataList { get; set; } = [];
}
