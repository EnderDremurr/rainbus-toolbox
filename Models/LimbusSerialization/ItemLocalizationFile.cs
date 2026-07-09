using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class ItemLocalizationFile : LocalizationFileBase, ILocalizationContainer<ItemDesc>
{
    [JsonProperty("dataList")]
    public List<ItemDesc> DataList { get; set; } = [];
}
