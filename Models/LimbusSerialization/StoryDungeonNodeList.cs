using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class StoryDungeonNodeList : LocalizationItemBase
{
    [JsonProperty("stageList")]
    public List<StageNode>? StageList { get; set; }
}
