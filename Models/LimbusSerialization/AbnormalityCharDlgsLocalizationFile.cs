using System.Collections.Generic;
using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class AbnormalityCharDlgsLocalizationFile : LocalizationFileBase, ILocalizationContainer<AbnormalityCharDlg>
{
    [JsonProperty("dataList")]
    public List<AbnormalityCharDlg> DataList { get; set; } = [];
}
