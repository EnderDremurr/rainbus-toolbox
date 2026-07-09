using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class DanteAbilityLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescRawDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescRawDesc> DataList { get; set; } = [];
}
