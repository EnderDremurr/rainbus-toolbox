using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class AbnormalityEventChoice : LocalizationItemBase
{
    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("eventDesc")]
    public string? EventDesc { get; set; }

    [JsonProperty("prevDesc")]
    public string? PrevDesc { get; set; }

    [JsonProperty("behaveDesc")]
    public string? BehaveDesc { get; set; }

    [JsonProperty("successDesc")]
    public List<string>? SuccessDesc { get; set; }

    [JsonProperty("failureDesc")]
    public List<string>? FailureDesc { get; set; }
}
