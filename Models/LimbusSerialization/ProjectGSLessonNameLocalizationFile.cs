using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ProjectGSLessonNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<ProjectGSLessonName>
{
    [JsonProperty("dataList")]
    public List<ProjectGSLessonName> DataList { get; set; } = [];
}