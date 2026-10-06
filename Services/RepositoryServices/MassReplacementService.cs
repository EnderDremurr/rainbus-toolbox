using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Utilities.RepositoryServices;

public partial class MassReplacementService
{
    private readonly Matcher _defaultBlacklistMatcher = new(StringComparison.OrdinalIgnoreCase);
    private readonly Regex _protectedPattern = ProtectedRegex();

    private readonly RepositoryManager _repositoryManager;

    public MassReplacementService(RepositoryManager repositoryManager, ConfigProvider configProvider)
    {
        _repositoryManager = repositoryManager;

        // populate matcher
        _defaultBlacklistMatcher.AddInclude("**/*.json");

        var blacklist = configProvider.GetYamlConfig<List<string>>("regex-blacklist");
        var sanitizedBlacklist = new List<string>();
        foreach (var entry in blacklist) // sanitizing
        {
            if (string.IsNullOrEmpty(entry))
                continue;

            var sanitizedEntry = entry.Trim();
            sanitizedEntry = sanitizedEntry.Replace('\\', '/'); // murder stupid windows slashes

            sanitizedEntry = sanitizedEntry.TrimStart('/');

            var isADirectory = false;
            if (sanitizedEntry.EndsWith('/')) // strip / if this is a directory
            {
                sanitizedEntry = sanitizedEntry.TrimEnd('/');
                isADirectory = true;
            }

            if (string.IsNullOrEmpty(sanitizedEntry))
                continue;

            if (!sanitizedEntry.Contains('/')) // add a prefix if it's not a folder
                sanitizedEntry = "**/" + sanitizedEntry;

            if (isADirectory) // append directory glob
                sanitizedEntry += "/**";

            sanitizedBlacklist.Add(sanitizedEntry);
        }

        sanitizedBlacklist.ForEach(p => _defaultBlacklistMatcher.AddExclude(p));
    }

    [GeneratedRegex(@"(\[[^\]]*\]|<[^>]*>)")]
    private static partial Regex ProtectedRegex();

    // also don't forget about whitelists (if empty then there's no whitelist)
    public async Task RunAllRegexesForAllFilesAsync(
        List<ReplacementEntry> entries,
        IProgress<(int Processed, int Total, string Label)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Log.Debug("Initializing regex replacement service for all entries");

        // update default blacklist
        var defaultAllowedFileList = _defaultBlacklistMatcher
            .GetResultsInFullPath(_repositoryManager.PathToLocalization)
            .ToList();

        // compile regexes
        var compiled = entries.Select(e => new CompiledEntry(
                e,
                BuildRegex(e),
                BuildMatchEvaluator(e)
            )
        ).ToList();

        // now goes vice versa
        var fileToEntries = new Dictionary<string, List<CompiledEntry>>(StringComparer.OrdinalIgnoreCase);
        foreach (var ce in compiled)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var sanitizedWhitelist = ce.Entry.FileWhiteList.Where(f => !string.IsNullOrWhiteSpace(f.FilePath))
                .Select(f => f.FilePath.Trim()).ToList();
            var files =
                sanitizedWhitelist.Count != 0
                    ? GetWhitelistedFiles(sanitizedWhitelist)
                    : defaultAllowedFileList;

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

        // update default blacklist
        var defaultAllowedFileList = _defaultBlacklistMatcher
            .GetResultsInFullPath(_repositoryManager.PathToLocalization)
            .ToList();

        var compiled = new CompiledEntry(
            entry,
            BuildRegex(entry),
            BuildMatchEvaluator(entry)
        );
        var single = new List<CompiledEntry> { compiled };


        var sanitizedWhitelist = entry.FileWhiteList.Where(f => !string.IsNullOrWhiteSpace(f.FilePath))
            .Select(f => f.FilePath.Trim()).ToList();
        var filesToEdit =
            sanitizedWhitelist.Count != 0
                ? GetWhitelistedFiles(sanitizedWhitelist)
                : defaultAllowedFileList;

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
            Log.Error("Failed processing {FilePath}: {ExMessage}", filePath, ex.Message);
        }
    }

    private string ApplyEntry(string input, CompiledEntry compiledEntry)
    {
        if (!compiledEntry.Regex.IsMatch(input))
            return input;

        var entry = compiledEntry.Entry;
        var regex = compiledEntry.Regex;

        // so this seems to be not ram usage but gc allocation warning, and should be fixed now (i hope i didnt break everything)
        //this shit is false if it is not allowed to replace tags
        var parts = entry.ReplaceTags
            ? [input]
            : input.AsSpan().IndexOfAny('[', '<') < 0
                ? [input]
                : _protectedPattern.Split(input);


        for (var j = 0; j < parts.Length; j++)
        {
            if (j % 2 != 0) continue; // odd elements are ones that match the regex (don't replace)

            parts[j] = compiledEntry.Evaluator is { } evaluator
                ? regex.Replace(parts[j], evaluator)
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

    private static string ReplaceWithCasePreservation(Match match, string replacement)
    {
        var expanded = match.Result(replacement);

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
        var localizationRoot = _repositoryManager.PathToLocalization;

        foreach (var dirtyEntry in whitelist)
        {
            var cleanEntry = dirtyEntry.Replace('\\', '/');

            if (Path.IsPathRooted(dirtyEntry)) continue;

            var fullPath = Path.Combine(localizationRoot, cleanEntry);


            if (cleanEntry.Contains('*'))
            {
                if (cleanEntry.Contains('/'))
                {
                    var lastSlash = cleanEntry.LastIndexOf('/');
                    var folder = cleanEntry[..lastSlash];
                    var wildcard = cleanEntry[(lastSlash + 1)..];
                    var folderPath = Path.Combine(localizationRoot, folder);

                    if (!Directory.Exists(folderPath)) continue;
                    result.AddRange(Directory.GetFiles(folderPath, wildcard, SearchOption.AllDirectories)
                        .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase)));
                }
                else
                {
                    result.AddRange(Directory.GetFiles(localizationRoot, cleanEntry, SearchOption.AllDirectories)
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

        return result.Select(Path.GetFullPath).Distinct().ToList();
    }

    private MatchEvaluator? BuildMatchEvaluator(ReplacementEntry entry)
    {
        return entry.PreserveCase ? new MatchEvaluator(m => ReplaceWithCasePreservation(m, entry.Replacement)) : null;
    }

    // regex
    private sealed record CompiledEntry(ReplacementEntry Entry, Regex Regex, MatchEvaluator? Evaluator);
}