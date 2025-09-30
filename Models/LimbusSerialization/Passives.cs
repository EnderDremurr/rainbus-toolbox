using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

//Passives* (Passive abilities)
[FilePattern("Passive*")]
public class PassivesFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; }
}
//TODO: Implement editor