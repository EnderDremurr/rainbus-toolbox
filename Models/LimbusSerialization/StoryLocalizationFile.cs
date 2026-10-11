using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class StoryLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = [];
}
