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

                    var dirty = false;

                    foreach (var token in root.Descendants().OfType<JValue>())
                    {
                        if (token.Type != JTokenType.String) continue;
                        if (token.Parent is JProperty { Name: "id" }) continue;

                        var original = token.Value<string>()!;
                        var replaced = entry.PreserveCase
                            ? regex.Replace(original,
                                m => ReplaceWithCasePreservation(m, entry.Replacement, entry.PreserveCase))
                            : regex.Replace(original, entry.Replacement);

                        if (replaced == original) continue;

                        token.Value = replaced;
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

    private static string ReplaceWithCasePreservation(Match match, string replacement, bool preserveCase)
    {
        var expanded = match.Result(replacement);

        if (!preserveCase)
            return expanded;
        if (string.IsNullOrEmpty(match.Value) || string.IsNullOrEmpty(expanded))
            return expanded;

        if (match.Value.All(char.IsUpper))
            return expanded.ToUpper();
        if (match.Value.All(char.IsLower))
            return expanded.ToLower();
        if (char.IsUpper(match.Value[0]) && match.Value.Skip(1).All(char.IsLower))
            return char.ToUpper(expanded[0]) + expanded[1..].ToLower();

        return expanded;
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
                .Where(p => !p.Split(Path.DirectorySeparatorChar).Contains("StoryData")
                            && !p.Split(Path.DirectorySeparatorChar).Contains("ScenarioModelCodes-AutoCreated.json")
                            && !p.Contains("StageNode"))
            );

        return result.Distinct().ToList();
    }
}