using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

//PersonalityVoiceDlg/Voice_* (Character personality voice lines)+

[FilePattern("Voice*")]
public class PersonalityVoiceFile : LocalizationFileBase, ILocalizationContainer<PersonalityVoiceEntry>
{
    [JsonProperty("dataList")]
    public List<PersonalityVoiceEntry> DataList { get; set; }
}

public class PersonalityVoiceEntry
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("desc")]
    public string Description { get; set; }

    [JsonProperty("dlg")]
    public string Dialogue { get; set; }
}
//TODO: Implement editor