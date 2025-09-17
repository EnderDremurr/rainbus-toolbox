using System.Collections.Generic;
using Newtonsoft.Json;
using RainbusToolbox.Utilities.Data;

[FilePattern("PanicInfo*")]
public class PanicInfoFile : LocalizationFileBase, ILocalizationContainer<PanicInfo>
{
    [JsonProperty("dataList")]
    public List<PanicInfo> DataList { get; set; }
}

public class PanicInfo
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("panicName")]
    public string PanicName { get; set; } = string.Empty;

    [JsonProperty("lowMoraleDescription")]
    public string LowMoraleDescription { get; set; } = string.Empty;

    [JsonProperty("panicDescription")]
    public string PanicDescription { get; set; } = string.Empty;
}