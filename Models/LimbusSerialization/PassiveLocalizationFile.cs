using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class PassiveLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDescFlavor>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDescFlavor> DataList { get; set; } = [];
}