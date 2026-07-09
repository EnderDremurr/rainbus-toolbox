using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class StageChapterLocalizationFile : LocalizationFileBase, ILocalizationContainer<StageChapter>
{
    [JsonProperty("dataList")]
    public List<StageChapter> DataList { get; set; } = [];
}
