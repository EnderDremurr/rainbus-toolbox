using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class ThreadDungeonLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; } = [];
}
