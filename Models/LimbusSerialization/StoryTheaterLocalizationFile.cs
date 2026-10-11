using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class StoryTheaterLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitleDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitleDesc> DataList { get; set; } = [];
}