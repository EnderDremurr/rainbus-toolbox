using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class UiLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = [];
}
