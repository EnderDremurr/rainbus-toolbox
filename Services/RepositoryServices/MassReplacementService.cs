using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Services.RepositoryServices;

public partial class MassReplacementService(RepositoryManager repositoryManager)
{
    private readonly Regex _protectedPattern = ProtectedRegex();

    [GeneratedRegex(@"(\[[^\]]*\]|<[^>]*>)")]
    private static partial Regex ProtectedRegex();

    // also don't forget about whitelists (if empty then there's no whitelist)
    public async Task RunAllRegexesForAllFilesAsync(
        List<ReplacementEntry> entries,
        IProgress<(int Processed, int Total, string Label)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Log.Debug("Initializing regex replacement service for all entries");

        // compile regexes
        var compiled = entries.Select(e => new CompiledEntry(e, BuildRegex(e))).ToList();

        // now goes vice versa
        var fileToEntries = new Dictionary<string, List<CompiledEntry>>(StringComparer.OrdinalIgnoreCase);
        foreach (var ce in compiled)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var files = GetWhitelistedFiles(ce.Entry.FileWhiteList.Select(f => f.FilePath).ToList());
            foreach (var file in files)
            {
                if (!fileToEntries.TryGetValue(file, out var list))
                    fileToEntries[file] = list = new List<CompiledEntry>();
                list.Add(ce);
            }
        }

        var work = fileToEntries.ToList();
        var total = work.Count;
        var processed = 0;

        // multithreading
        await Parallel.ForEachAsync(
            work,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            },
            (kvp, ct) =>
            {
                ProcessFile(kvp.Key, kvp.Value, ct);

                var done = Interlocked.Increment(ref processed);
                progress?.Report((done, total, Path.GetFileName(kvp.Key)));
                return ValueTask.CompletedTask;
            });
    }

    public async Task RunOneRegexForAllFilesAsync(
        ReplacementEntry entry,
        IProgress<(int Processed, int Total, string Label)>? progress = null,
        CancellationToken cancellationToken = default,
        int totalEntries = 1,
        int currentEntryIndex = 0)
    {
        Log.Debug("Initializing regex replacement service for one entry");

        var compiled = new CompiledEntry(entry, BuildRegex(entry));
        var single = new List<CompiledEntry> { compiled };

        var filesToEdit = GetWhitelistedFiles(entry.FileWhiteList.Select(f => f.FilePath).ToList());
        var total = filesToEdit.Count;
        var processed = 0;

        await Parallel.ForEachAsync(
            filesToEdit,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cancellationToken
            },
            (filePath, cancellationTokenForFile) =>
            {
                ProcessFile(filePath, single, cancellationTokenForFile);

                var done = Interlocked.Increment(ref processed);
                progress?.Report((done, total,
                    $"[{currentEntryIndex + 1}/{totalEntries}] {Path.GetFileName(filePath)}"));
                return ValueTask.CompletedTask;
            });
    }

    // now reads file once and then farts all regexes onto it, instead of checking file for every regex
    private void ProcessFile(string filePath, List<CompiledEntry> entries, CancellationToken cancellationToken)
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
                var current = original;

                // tries every regex for every string
                foreach (var compiledEntry in entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    current = ApplyEntry(current, compiledEntry);
                }

                if (current == original) continue;

                token.Value = current;
                dirty = true;
            }

            if (dirty)
                File.WriteAllText(filePath, JsonConvert.SerializeObject(root, Formatting.Indented));
        }
        catch (Exception ex)
        {
            Log.Error($"Failed processing {filePath}: {ex.Message}");
        }
    }

    private string ApplyEntry(string input, CompiledEntry ce)
    {
        var entry = ce.Entry;
        var regex = ce.Regex;

        //this shit is false if it doesn't need to replace
        var parts = entry.ReplaceTags
            ? _protectedPattern.Split(input)
            : [input];

        for (var j = 0; j < parts.Length; j++)
        {
            if (j % 2 != 0) continue; // odd elements are ones that match the regex (don't replace)

            parts[j] = entry.PreserveCase
                ? regex.Replace(parts[j],
                    m => ReplaceWithCasePreservation(m, entry.Replacement, entry.PreserveCase))
                : regex.Replace(parts[j], entry.Replacement);
        }

        return string.Join("", parts);
    }

    private static Regex BuildRegex(ReplacementEntry entry)
    {
        // compiles regexes
        var options = entry.MatchCase
            ? RegexOptions.Compiled
            : RegexOptions.Compiled | RegexOptions.IgnoreCase;

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

    // regex
    private sealed record CompiledEntry(ReplacementEntry Entry, Regex Regex);
}