using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class PassiveLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDescFlavor>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDescFlavor> DataList { get; set; } = [];
}