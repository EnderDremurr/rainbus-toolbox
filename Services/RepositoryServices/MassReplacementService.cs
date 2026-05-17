using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Services.RepositoryServices;

public class MassReplacementService(RepositoryManager repositoryManager)
{
    // also don't forget about whitelists (if empty then there's no whitelist)


    public async Task RunAllRegexesForAllFilesAsync(
        List<ReplacementEntry> entries,
        IProgress<(int Processed, int Total, string Label)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Log.Debug("Initializing regex replacement service for all entries");

        for (var i = 0; i < entries.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((i, entries.Count, $"Entry {i + 1}/{entries.Count}: {entries[i].Target}"));
            await RunOneRegexForAllFilesAsync(entries[i], progress, cancellationToken, entries.Count, i);
        }
    }

    public async Task RunOneRegexForAllFilesAsync(
        ReplacementEntry entry,
        IProgress<(int Processed, int Total, string Label)>? progress = null,
        CancellationToken cancellationToken = default,
        int totalEntries = 1,
        int currentEntryIndex = 0)
    {
        Log.Debug("Initializing regex replacement service for one entry");

        var filesToEdit = GetWhitelistedFiles(entry.FileWhiteList.Select(f => f.FilePath).ToList());
        var regex = BuildRegex(entry);
        var total = filesToEdit.Count;

        for (var i = 0; i < total; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var filePath = filesToEdit[i];
            progress?.Report((i + 1, total, $"[{currentEntryIndex + 1}/{totalEntries}] {Path.GetFileName(filePath)}"));

            await Task.Run(() =>
            {
                try
                {
                    var raw = File.ReadAllText(filePath);
                    var root = JsonConvert.DeserializeObject<JObject>(raw);
                    if (root == null) return;

                    var dataList = root["dataList"] as JArray;
                    if (dataList == null) return;

                    var dirty = false;

                    foreach (var item in dataList)
                    foreach (var prop in item.Children<JProperty>())
                    {
                        if (prop.Name == "id") continue;
                        if (prop.Value.Type != JTokenType.String) continue;

                        var original = prop.Value.Value<string>()!;
                        var replaced = regex.Replace(original,
                            m => ReplaceWithCasePreservation(m.Value, entry.Replacement, entry.PreserveCase));

                        if (replaced == original) continue;

                        prop.Value = replaced;
                        dirty = true;
                    }

                    if (dirty)
                        File.WriteAllText(filePath, JsonConvert.SerializeObject(root, Formatting.Indented));
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed processing {filePath}: {ex.Message}");
                }
            }, cancellationToken);
        }
    }

    private static Regex BuildRegex(ReplacementEntry entry)
    {
        var options = entry.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase;

        string pattern;

        if (entry.IsRegex)
        {
            pattern = entry.MatchWholeWord
                ? $@"\b(?:{entry.Target})\b"
                : entry.Target;
        }
        else
        {
            var escaped = Regex.Escape(entry.Target);
            pattern = entry.MatchWholeWord
                ? $@"\b{escaped}\b"
                : escaped;
        }

        return new Regex(pattern, options,
            TimeSpan.FromSeconds(5)); // timeout if regex is retarded
    }

    private static string ReplaceWithCasePreservation(string original, string replacement, bool preserveCase)
    {
        if (!preserveCase) return replacement;
        if (string.IsNullOrEmpty(original) || string.IsNullOrEmpty(replacement))
            return replacement;

        if (original.All(char.IsUpper))
            return replacement.ToUpper();

        if (original.All(char.IsLower))
            return replacement.ToLower();

        if (char.IsUpper(original[0]) && original.Skip(1).All(char.IsLower))
            return char.ToUpper(replacement[0]) + replacement[1..].ToLower();

        return replacement;
    }

    private List<string> GetWhitelistedFiles(List<string> whitelist)
    {
        var result = new List<string>();
        var localizationRoot = repositoryManager.PathToLocalization;

        if (whitelist.Any())
            foreach (var entry in whitelist)
            {
                if (Path.IsPathRooted(entry)) continue;

                var fullPath = Path.Combine(localizationRoot, entry);

                if (entry.Contains('*'))
                {
                    if (entry.Contains('/'))
                    {
                        var lastSlash = entry.LastIndexOf('/');
                        var folder = entry[..lastSlash];
                        var wildcard = entry[(lastSlash + 1)..];
                        var folderPath = Path.Combine(localizationRoot, folder);

                        if (!Directory.Exists(folderPath)) continue;
                        result.AddRange(Directory.GetFiles(folderPath, wildcard, SearchOption.AllDirectories)
                            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)));
                    }
                    else
                    {
                        result.AddRange(Directory.GetFiles(localizationRoot, entry, SearchOption.AllDirectories)
                            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)));
                    }
                }
                else if (Directory.Exists(fullPath))
                {
                    result.AddRange(Directory.GetFiles(fullPath, "*.json", SearchOption.AllDirectories));
                }
                else if (File.Exists(fullPath) && fullPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(fullPath);
                }
            }
        else
            // if whitelist is not enabled, get all files except from StoryData folder
            result.AddRange(Directory.GetFiles(localizationRoot, "*.json", SearchOption.AllDirectories)
                .Where(p => !p.Split(Path.DirectorySeparatorChar).Contains("StoryData")));

        return result.Distinct().ToList();
    }
}