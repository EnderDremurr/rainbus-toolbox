using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class AnnouncerVoiceLocalizationFile : LocalizationFileBase, ILocalizationContainer<AnnouncerVoice>
{
    [JsonProperty("dataList")]
    public List<AnnouncerVoice> DataList { get; set; } = [];
}