using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class AbnormalityGuideContentLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityGuide>
{
    [JsonProperty("dataList")]
    public List<AbnormalityGuide> DataList { get; set; } = [];
}
