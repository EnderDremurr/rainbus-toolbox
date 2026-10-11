using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class UserBannerLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdNameDesc>
{
    [JsonProperty("dataList")]
    public List<GenericIdNameDesc> DataList { get; set; } = [];
}
