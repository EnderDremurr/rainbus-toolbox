using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class AbnormalityCharDlg : LocalizationItemBase
{
    [JsonProperty("personalityid")]
    public string? PersonalityId { get; set; }

    [JsonProperty("voicefile")]
    public string? VoiceFile { get; set; }

    [JsonProperty("teller")]
    public string? Teller { get; set; }

    [JsonProperty("dialog")]
    public string? Dialog { get; set; }

    [JsonProperty("usage")]
    public string? Usage { get; set; }
}
