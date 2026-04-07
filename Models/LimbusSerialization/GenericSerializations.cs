using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class GenericIdContent : LocalizationItemBase
{
    [JsonProperty("content")]
    public string? Content { get; set; }
}

public class GenericIdNameDesc : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class GenericIdDesc : LocalizationItemBase
{
    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class GenericIdDescRawDesc : LocalizationItemBase
{
    [JsonProperty("desc")]
    public string? Desc { get; set; }

    [JsonProperty("rawDesc")]
    public string? RawDesc { get; set; }
}

public class GenericIdDescription : LocalizationItemBase
{
    [JsonProperty("description")]
    public string? Description { get; set; }
}

public class GenericIdName : LocalizationItemBase
{
    [JsonProperty("name")]
    public string? Name { get; set; }
}

public class GenericIdTitleDesc : LocalizationItemBase
{
    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class GenericIdTitle : LocalizationItemBase
{
    [JsonProperty("title")]
    public string? Title { get; set; }
}

public class GenericIdDescDlg : LocalizationItemBase
{
    [JsonProperty("desc")]
    public string? Description { get; set; }

    [JsonProperty("dlg")]
    public string? Dialogue { get; set; }
}