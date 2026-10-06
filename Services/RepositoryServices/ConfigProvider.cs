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
        if (configFileName.Contains('.'))
        {
            var split = configFileName.Split('.');

            if (split.Length > 2) // means there is several dots - this should never happen
                throw new ArgumentException(
                    $"Provided config file name is invalid! File: {configFileName}");

            if (split[1] != "yaml" && split[1] != "yml")
                throw new ArgumentException($"The file provided is not of YAML format. File: {configFileName}");

            configFileName = split.First(); // remove the extension from filename
            configFileName += ".yaml";
        } // helper enforces .yaml naming, so if i forget and add a type in parameter, this should sanitize it or throw


        return GetOverride<T>(configFileName) ?? GetBuiltin<T>(configFileName);
    }

    private T GetBuiltin<T>(string sanitizedFileNameWithExtension)
        where T : class // this can't return null, as null in builtin config is a fatal error
    {
        try
        {
            var configFileStream = AssetLoader.Open(new Uri(BaseAvaresPath + sanitizedFileNameWithExtension));
            using var streamReader = new StreamReader(configFileStream);
            var config = _deserializer.Deserialize<T?>(streamReader);

            if (config == null)
                throw new InvalidOperationException(
                    "Config file is empty!");

            return config;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Failed to load config \"{sanitizedFileNameWithExtension}\": {e.Message}", e);
        }
    }

    private T? GetOverride<T>(string sanitizedFileNameWithExtension) where T : class
    {
        if (string.IsNullOrEmpty(repositoryManager.RepositoryRoot))
            throw new NullReferenceException("Path to localization repo is null! This is a dev error!");


        var possibleOverridePath = Path.Combine(repositoryManager.PathToLocalization, sanitizedFileNameWithExtension);
        var hasOverride = File.Exists(possibleOverridePath);

        if (!hasOverride)
            return null; // return null without warnings, this just means there's no override

        try
        {
            var configFileStream = File.OpenRead(possibleOverridePath);
            using var streamReader = new StreamReader(configFileStream);
            var config = _deserializer.Deserialize<T?>(streamReader);

            return config;
        }
        catch (Exception e)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(new InvalidOperationException(
                $"Версия конфига \"{sanitizedFileNameWithExtension}\" в репозитории невалидна, приложение будет использовать встроенный конфиг!",
                e));
            return null;
        }
    }
}