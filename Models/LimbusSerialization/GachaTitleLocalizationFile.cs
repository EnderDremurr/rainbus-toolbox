using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class GachaTitleLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = [];
}