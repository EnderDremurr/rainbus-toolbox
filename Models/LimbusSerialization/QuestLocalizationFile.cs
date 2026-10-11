using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class QuestLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = [];
}
