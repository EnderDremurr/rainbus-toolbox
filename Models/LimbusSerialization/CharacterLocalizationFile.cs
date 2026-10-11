using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class CharacterLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdName>
{
    [JsonProperty("dataList")]
    public List<GenericIdName> DataList { get; set; } = [];
}
