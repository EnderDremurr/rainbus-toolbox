using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class BufLocalizationFile : LocalizationFileBase, ILocalizationContainer<BuffKeyword>
{
    [JsonProperty("dataList")]
    public List<BuffKeyword> DataList { get; set; } = [];
}
