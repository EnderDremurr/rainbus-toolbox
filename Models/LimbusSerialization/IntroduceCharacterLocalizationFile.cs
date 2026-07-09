using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class IntroduceCharacterLocalizationFile : LocalizationFileBase, ILocalizationContainer<IntroductuceCharacter>
{
    [JsonProperty("dataList")]
    public List<IntroductuceCharacter> DataList { get; set; } = [];
}
