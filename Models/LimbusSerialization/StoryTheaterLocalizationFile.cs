using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class StoryTheaterLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitleDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitleDesc> DataList { get; set; } = [];
}
