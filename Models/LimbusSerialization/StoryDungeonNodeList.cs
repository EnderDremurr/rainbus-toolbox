using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class StoryDungeonNodeList : LocalizationItemBase
{
    [JsonProperty("stageList")]
    public List<StageNode>? StageList { get; set; }
}