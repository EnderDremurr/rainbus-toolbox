using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class DanteNoteLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdTitleDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdTitleDesc> DataList { get; set; } = [];
}
