using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class PersonalityVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<GenericIdDescDlg>
{
    [JsonProperty("dataList")]
    public List<GenericIdDescDlg> DataList { get; set; } = [];
}