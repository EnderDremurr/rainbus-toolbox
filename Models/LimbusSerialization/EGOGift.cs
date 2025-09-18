//EGOGift* (EGO gifts)

using System.Collections.Generic;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Utilities.Data;

[FilePattern("EGOGift*")]
public class EGOGiftFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}