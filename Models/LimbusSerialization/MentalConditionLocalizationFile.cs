using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class MentalConditionLocalizationFile : LocalizationFileBase, ILocalizationContainer<MentalCondition>
{
    [JsonProperty("dataList")]
    public List<MentalCondition> DataList { get; set; } = [];
}