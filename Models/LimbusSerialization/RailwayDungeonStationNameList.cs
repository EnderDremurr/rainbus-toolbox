using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class RailwayDungeonStationNameList : LocalizationItemBase
{
    [JsonProperty("nameList")]
    public List<RailwayDungeonStationNameList>? NameList { get; set; }
}