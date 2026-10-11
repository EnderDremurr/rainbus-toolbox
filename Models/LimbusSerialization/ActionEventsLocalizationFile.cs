using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ActionEventsLocalizationFile : LocalizationFileBase, ILocalizationContainer<ActionEvent>
{
    [JsonProperty("dataList")]
    public List<ActionEvent> DataList { get; set; } = [];
}