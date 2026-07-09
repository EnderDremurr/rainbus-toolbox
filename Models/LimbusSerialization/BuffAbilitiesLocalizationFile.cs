using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class BuffAbilitiesLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdDesc> DataList { get; set; } = [];
}
