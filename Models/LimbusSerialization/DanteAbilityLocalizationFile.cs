using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class DanteAbilityLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescRawDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescRawDesc> DataList { get; set; } = [];
}