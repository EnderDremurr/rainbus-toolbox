using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

[FilePattern("/*")]
public class UnidentifiedFile : LocalizationFileBase, ILocalizationContainer<string>
{
    [JsonProperty("dataList")]
    public List<string> DataList { get; set; } = new List<string>();
}