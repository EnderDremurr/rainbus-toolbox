using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ThreadDungeonLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; } = [];
}