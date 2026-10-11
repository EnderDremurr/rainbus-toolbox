using Newtonsoft.Json;

namespace RainbusToolbox.Services.ExternalServices;

internal static class LocalizationJsonSettings
{
    public static readonly JsonSerializerSettings Default = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    public static readonly JsonSerializerSettings Unidentified = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None
    };
}
