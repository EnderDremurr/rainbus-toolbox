using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class PersonalityLocalizationFile : LocalizationFileBase, ILocalizationContainer<PersonalityDataEntry>
{
    [JsonProperty("dataList")]
    public List<PersonalityDataEntry> DataList { get; set; } = [];
}
