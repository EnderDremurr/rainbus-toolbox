using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class AbnormalityGuideContentLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityGuide>
{
    [JsonProperty("dataList")]
    public List<AbnormalityGuide> DataList { get; set; } = [];
}