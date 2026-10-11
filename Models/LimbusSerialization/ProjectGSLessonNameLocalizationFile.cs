using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class ProjectGSLessonNameLocalizationFile : LocalizationFileBase, ILocalizationContainer<ProjectGSLessonName>
{
    [JsonProperty("dataList")]
    public List<ProjectGSLessonName> DataList { get; set; } = [];
}
