using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;


[FilePattern("Egos.json")]
public class EgoNames : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}