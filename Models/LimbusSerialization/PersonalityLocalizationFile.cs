using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class PersonalityLocalizationFile : LocalizationFileBase, ILocalizationContainer<PersonalityDataEntry>
{
    [JsonProperty("dataList")]
    public List<PersonalityDataEntry> DataList { get; set; } = [];
}
