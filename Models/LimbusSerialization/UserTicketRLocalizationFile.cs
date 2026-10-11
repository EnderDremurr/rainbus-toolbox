using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class UserTicketRLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; } = [];
}
