using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class DungeonStartBuffsLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescription>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescription> DataList { get; set; } = [];
}