using System.IO;
using System.IO.Compression;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Services;

public static class LocalizationPackager
{
    public static string PackageLocalization(string version, RepositoryManager repositoryManager)
    {
        repositoryManager.SynchronizeWithOrigin();

        var repoPath = repositoryManager.Repository.Info.WorkingDirectory;
        var zipFileName = $"RCR v{version}.zip";
        var zipPath = Path.Combine(repositoryManager.PathToDistribution, zipFileName);

        if (File.Exists(zipPath))
            File.Delete(zipPath);

        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var localizePath = Path.Combine(repoPath, repositoryManager.LocalizationFolder);
            if (Directory.Exists(localizePath))
            {
                foreach (var file in Directory.GetFiles(localizePath, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(localizePath, file);
                    zip.CreateEntryFromFile(file, relativePath);
                }
            }
        }

        return zipPath;
    }
}