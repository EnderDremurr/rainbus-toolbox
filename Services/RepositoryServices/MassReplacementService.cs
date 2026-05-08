using System.Collections.Generic;
using System.IO;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Services.RepositoryServices;

public class MassReplacementService(RepositoryManager repositoryManager)
{
    // i guess it'll be faster to do "open file, check all regexes", rather than "check each file for each regex"
    // also don't forget about whitelists (if empty then there's no whitelist)

    public void RunAllRegexesForAllFiles()
    {
    }

    public void RunOneRegexForAllFiles(ReplacementEntry entry)
    {
    }

    private List<string> GetWhitelistedFiles(List<string> whitelist)
    {
        var result = new List<string>();
        var localizationFiles = repositoryManager.PathToLocalization;

        foreach (var entry in whitelist)
        {
            // check for wildcards (*)
            // check for folders/directories
            // check for folder + wildcards ( like Announcer/Skibidi*.json )

            // whitelists should be relative to localization root
            if (Path.IsPathRooted(entry))
                continue;

            if (entry.Last() == '/')
            {
                var fullPath = Path.Combine(localizationFiles, entry);
                // this means it's an entire folder
                if (!Directory.Exists(fullPath)) continue;
                result.AddRange(Directory.GetFiles(fullPath));
            }
            else if (entry.Contains('*'))
            {
                // wildcard, check for subfolders 
                if (entry.Contains('/'))
                {
                    // has subfolders
                    var wildcard = entry.Split('/').Last();
                    var fullPath = Path.Combine(localizationFiles, entry.Replace(wildcard, ""));
                    if (!Directory.Exists(fullPath)) continue;
                    result.AddRange(Directory.GetFiles(fullPath, wildcard));
                }
                else
                {
                    result.AddRange(Directory.GetFiles(localizationFiles, entry));
                }
            }
            else
            {
                // otherwise it's just a filename
                var fullPath = Path.Combine(localizationFiles, entry);
                if (!Path.Exists(fullPath)) continue;
                result.Add(fullPath);
            }
        }

        return result;
    }
}