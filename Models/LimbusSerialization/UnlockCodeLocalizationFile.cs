using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class UnlockCodeLocalizationFile : LocalizationFileBase, ILocalizationContainer<UnlockCode>
{
    [JsonProperty("dataList")]
    public List<UnlockCode> DataList { get; set; } = [];
}
