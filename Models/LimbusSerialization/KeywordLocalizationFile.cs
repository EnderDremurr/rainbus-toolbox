using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class KeywordLocalizationFile : LocalizationFileBase, ILocalizationContainer<BuffKeyword>
{
    [JsonProperty("dataList")]
    public List<BuffKeyword> DataList { get; set; } = [];
}
