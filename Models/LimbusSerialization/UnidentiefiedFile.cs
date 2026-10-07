using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class UnknownFile : LocalizationFileBase, ILocalizationContainer<string>
{
    [JsonProperty("dataList")]
    public List<string> DataList { get; set; } = [];
}