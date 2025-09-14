using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Models;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddSingleton<PersistentDataManager, PersistentDataManager>();
        collection.AddSingleton<DiscordManager, DiscordManager>();
        collection.AddSingleton<GithubManager, GithubManager>();
    }
}