using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class RailwayDungeonLocalizationFile : LocalizationFileBase, ILocalizationContainer<RailwayDungeon>
{
    [JsonProperty("dataList")]
    public List<RailwayDungeon> DataList { get; set; } = [];
}
