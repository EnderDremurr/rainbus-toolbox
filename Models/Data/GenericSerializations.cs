using Newtonsoft.Json;

namespace RainbusToolbox.Utilities.Data;

public class GenericIdContent
{
    [JsonProperty("id")]
    public string Id { get; set; }
    [JsonProperty("content")]
    public string Content { get; set; }
}

public class GenericIdNameDesc
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("desc")]
    public string Desc { get; set; } = string.Empty;
}

public class GenericIdName
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
}