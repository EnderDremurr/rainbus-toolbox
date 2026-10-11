using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class NormalBattleHintLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdContent>
{
    [JsonProperty("dataList")]
    public List<GenericIdContent> DataList { get; set; } = [];
}
