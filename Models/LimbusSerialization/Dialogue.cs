using System.Collections.Generic;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

[FilePattern("AbDlg*")]
public class DialogueFile : LocalizationFileBase
{
    [JsonProperty("dataList")]
    public List<DialogueEntry> DataList { get; set; }
}

public class DialogueEntry : LocalizationFileBase
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("personalityid")]
    public int PersonalityId { get; set; }

    [JsonProperty("voicefile")]
    public int VoiceFile { get; set; }

    [JsonProperty("teller")]
    public string Teller { get; set; }

    [JsonProperty("dialog")]
    public string Dialog { get; set; }

    // Keeping as string since format is non-standard, can parse later if needed
    [JsonProperty("usage")]
    public string Usage { get; set; }
}