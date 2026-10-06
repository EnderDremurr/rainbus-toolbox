using System.IO;
using System.IO.Compression;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services.RepositoryServices;

namespace RainbusToolbox.Utilities;

public static class LocalizationPackager
{
    public static async Task<string> PackageLocalizationAsync(string version, LocalizationManager localizationManager,
        GitManager gitManager)
    {
        await gitManager.SynchronizeWithOriginAsync(); // i dunno if this should happen here? but i'll leave it for now

        var repoPath = localizationManager.RepositoryRoot;
        var zipFileName = $"{gitManager.GetCurrentRepoDisplayName()} v{version}.zip";
        var zipPath = Path.Combine(localizationManager.PathToDistribution, zipFileName);

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        var localizePath = Path.Combine(repoPath, localizationManager.LocalizationFolder);

        if (!Directory.Exists(localizePath))
            return zipPath;

        // TODO: after i'm done with new keyword system, the conversion should be ran here and for .dist version, before packaging

        foreach (var file in Directory.GetFiles(localizePath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(localizePath, file);
            zip.CreateEntryFromFile(file, relativePath);
        }

        return zipPath;
    }
}