using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class RailwayDungeonStationNameLocalizationFile : LocalizationFileBase,
    ILocalizationContainer<RailwayDungeonStationNameList>
{
    [JsonProperty("dataList")]
    public List<RailwayDungeonStationNameList> DataList { get; set; } = [];
}
