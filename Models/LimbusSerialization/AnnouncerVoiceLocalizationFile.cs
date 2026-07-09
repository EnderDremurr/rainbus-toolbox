using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class AnnouncerVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<AnnouncerVoice>
{
    [JsonProperty("dataList")]
    public List<AnnouncerVoice> DataList { get; set; } = [];
}
