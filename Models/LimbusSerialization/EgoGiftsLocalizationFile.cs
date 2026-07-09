using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class EgoGiftsLocalizationFile : LocalizationFileBase, ILocalizationContainer<EgoGift>
{
    [JsonProperty("dataList")]
    public List<EgoGift> DataList { get; set; } = [];
}
