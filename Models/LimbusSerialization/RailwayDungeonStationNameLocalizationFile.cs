using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class RailwayDungeonStationNameLocalizationFile : LocalizationFileBase,
    ILocalizationContainer<RailwayDungeonStationNameList>
{
    [JsonProperty("dataList")]
    public List<RailwayDungeonStationNameList> DataList { get; set; } = [];
}