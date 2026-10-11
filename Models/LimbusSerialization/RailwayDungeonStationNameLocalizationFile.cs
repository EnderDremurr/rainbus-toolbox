using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class RailwayDungeonStationNameLocalizationFile : LocalizationFileBase,
    ILocalizationContainer<RailwayDungeonStationNameList>
{
    [JsonProperty("dataList")]
    public List<RailwayDungeonStationNameList> DataList { get; set; } = [];
}
