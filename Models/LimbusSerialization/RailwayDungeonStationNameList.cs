using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class RailwayDungeonStationNameList : LocalizationItemBase
{
    [JsonProperty("nameList")]
    public List<RailwayDungeonStationNameList>? NameList { get; set; }
}
