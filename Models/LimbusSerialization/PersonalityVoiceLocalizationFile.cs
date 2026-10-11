using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class PersonalityVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescDlg>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescDlg> DataList { get; set; } = [];
}
