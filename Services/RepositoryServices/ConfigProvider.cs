using System.IO;
using Avalonia.Platform;
using RainbusToolbox.Models.Managers;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RainbusToolbox.Services.RepositoryServices;

public sealed class
    ConfigProvider(RepositoryManager repositoryManager) // repo manager is required for future override handling
{
    private const string BaseAvaresPath = "avares://RainbusToolbox/Assets/Configs/";

    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(PascalCaseNamingConvention.Instance)
        .Build();

    public T GetYamlConfig<T>(string configFileName) where T : class
    {
        // for now forget about repo overrides, i'll add that later

        if (configFileName.Contains('.'))
        {
            var split = configFileName.Split('.');

            if (split.Length > 2) // means there is several dots - this should never happen
                throw new ArgumentException(
                    $"Provided config file name is invalid! File: {configFileName}");

            if (split[1] != "yaml" && split[1] != "yml")
                throw new ArgumentException($"The file provided is not of YAML format. File: {configFileName}");

            configFileName = split.First(); // remove the extension from filename
        } // helper enforces .yaml naming, so if i forget and add a type in parameter, this should sanitize it or throw

        try
        {
            var configFileStream = AssetLoader.Open(new Uri(BaseAvaresPath + configFileName + ".yaml"));
            using var streamReader = new StreamReader(configFileStream);

            var config = _deserializer.Deserialize<T>(streamReader);

            if (config == null)
                throw new InvalidOperationException(
                    "Config file is empty!");

            return config;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to load config \"{configFileName}\": {e.Message}", e);
        }
    }
}