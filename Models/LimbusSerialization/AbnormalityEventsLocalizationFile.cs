using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class AbnormalityEventsLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityEventChoice>
{
    [JsonProperty("dataList")]
    public List<AbnormalityEventChoice> DataList { get; set; } = [];
}
