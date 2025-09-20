using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

//EGOVoiceDig/VoiceEGO* (EGO ability voice lines) +
[FilePattern("VoiceEGO*")]
public class EGOVoiceFile : LocalizationFileBase, ILocalizationContainer<EGOVoiceEntry>
{
    [JsonProperty("dataList")]
    public List<EGOVoiceEntry> DataList { get; set; }
}

public class EGOVoiceEntry
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    [JsonProperty("dlg")]
    public string Dialogue { get; set; }
}
//TODO: Implement editor