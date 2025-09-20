using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

[FilePattern("Announcer*")]
public class BattleAnnouncerFile : LocalizationFileBase, ILocalizationContainer<BattleAnnouncerEntry>
{
    [JsonProperty("dataList")]
    public List<BattleAnnouncerEntry> DataList { get; set; }
}

public class BattleAnnouncerEntry
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("dlg")]
    public string Dialogue { get; set; }
}