using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class GenericIdContent
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("content")]
    public string? Content { get; set; }
}

public class GenericIdNameDesc
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class GenericIdDesc
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class GenericIdDescRawDesc
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string? Desc { get; set; }
    [JsonProperty("rawDesc")]
    public string? RawDesc { get; set; }
}

public class GenericIdDescription
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("description")]
    public string? Description { get; set; }
}

public class GenericIdName
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("name")]
    public string? Name { get; set; }
}

public class GenericIdTitleDesc
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string? Title { get; set; }

    [JsonProperty("desc")]
    public string? Desc { get; set; }
}

public class GenericIdTitle
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("title")]
    public string? Title { get; set; }
}

public class GenericIdDescDlg
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string? Description { get; set; }

    [JsonProperty("dlg")]
    public string? Dialogue { get; set; }
}
