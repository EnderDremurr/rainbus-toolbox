using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class PanicInfoLocalizationFile : LocalizationFileBase, ILocalizationContainer<PanicInfo>
{
    [JsonProperty("dataList")]
    public List<PanicInfo> DataList { get; set; } = [];
}
