using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Services;

public class KeywordProcessingService(RepositoryManager repositoryManager)
{
    private const string RCRKeywordColorsLink =
        "https://raw.githubusercontent.com/Let-It-Rain/RCR-LCB/refs/heads/main/keyword_colors.json";

    private const string MTLKeywordColorsLink =
        "https://raw.githubusercontent.com/kimght/LimbusCompanyRuMTL/main/data/build/keyword_colors.txt";

    private readonly HttpClient _httpClient = new();

    private readonly Regex _tagRegex = new(@"\[[^:\]]+:[`*'][^`*']+[`*']\]", RegexOptions.Compiled);

    private bool _isInitialized;

    private Dictionary<string, string> _keywordColorList = [];

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await InitializeAsync();
        _isInitialized = true;
    }


    public async Task InitializeAsync()
    {
        var keywordColorPath = repositoryManager.PathToKeywordColorList;

        // check if there is a cached keyword colors file
        if (Path.Exists(keywordColorPath)
            && string.IsNullOrWhiteSpace(await File.ReadAllTextAsync(keywordColorPath)))
            // no cache, then try to get the RCR keywords
            try
            {
                _keywordColorList = await DownloadRCRKeywordColorsAsync() ?? throw new NullReferenceException();
                // write cache
                await File.WriteAllTextAsync(keywordColorPath, JsonConvert.SerializeObject(_keywordColorList));
            }
            catch (Exception e)
            {
                await App.Current.HandleGlobalExceptionAsync(e);
            }
        else
            // cache found, deserializing it
            try
            {
                _keywordColorList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        await File.ReadAllTextAsync(keywordColorPath)) ?? throw new NullReferenceException();
            }
            catch (Exception e)
            {
                await App.Current.HandleGlobalExceptionAsync(e);
            }
        // not sure how to handle exceptions tho, ig for now it'll just crash XD
    }

    public async Task<Dictionary<string, string>?> DownloadRCRKeywordColorsAsync()
    {
        try
        {
            var raw = await _httpClient.GetStringAsync(RCRKeywordColorsLink);
            var deserialized = JsonConvert.DeserializeObject<Dictionary<string, string>>(raw);
            return deserialized;
        }
        catch (Exception e)
        {
            await App.Current.HandleNonFatalExceptionAsync(e);
            return null;
        }
    }

    public async Task<Dictionary<string, string>?> DownloadMTLKeywordColorsAsync()
    {
        try
        {
            var content = await _httpClient.GetStringAsync(MTLKeywordColorsLink);

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StringReader(content);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('¤', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    if (!dict.ContainsKey(key))
                        dict[key] = value;
                }
            }

            return dict;
        }
        catch (Exception e)
        {
            await App.Current.HandleNonFatalExceptionAsync(e);
            return null;
        }
    }

    // TODO: Slopreview
    public string GetMeshFromTag(string tag)
    {
        // Validate the tag format
        if (string.IsNullOrEmpty(tag) || tag.Length < 2 || tag[0] != '[' || tag[tag.Length - 1] != ']')
            throw new ArgumentException("Invalid tag format");

        var inner = tag.Substring(1, tag.Length - 2); // strip [ and ]
        string tagKeyword;
        string tagFill;

        // Check for delimiter patterns (backtick, asterisk, or single quote)
        var delimiterIndex = inner.IndexOfAny(['`', '*', '\'']);
        if (delimiterIndex >= 0 && inner[delimiterIndex - 1] == ':')
        {
            var delimiter = inner[delimiterIndex];
            tagKeyword = inner.Substring(0, delimiterIndex - 1);

            // Find the closing delimiter
            var endDelimiter = inner.IndexOf(delimiter, delimiterIndex + 1);
            if (endDelimiter < 0)
                throw new ArgumentException($"Invalid tag format: missing closing {delimiter}");

            tagFill = inner.Substring(delimiterIndex + 1, endDelimiter - (delimiterIndex + 1));
        }
        else
        {
            tagKeyword = inner;
            tagFill = inner;
        }

        // Case-insensitive lookup
        var actualKey = _keywordColorList.Keys
            .FirstOrDefault(k => string.Equals(k, tagKeyword, StringComparison.OrdinalIgnoreCase));

        // If no color found, return original tag unchanged
        if (actualKey == null) return tag;
        if (GetKeywordColor(actualKey) is not { } color) return tag;
        var mesh =
            $"<sprite name=\\\"{actualKey}\\\"><color={color}><u><link=\\\"{actualKey}\\\"><noparse>{tagFill}</noparse></link></u></color>";
        return mesh;
    }

    // TODO: Slopreview
    public async Task<int> ReplaceEveryTagWithMesh(string path, CancellationToken cancellationToken = default,
        IProgress<string>? progress = null)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        await EnsureInitializedAsync();


        // Exclude JSON files from general processing to avoid corruption
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        var totalFiles = files.Count();

        progress?.Report($"Starting replacement for {totalFiles} files...");

        var processedCount = 0;
        var replacedCount = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                // Preserve original encoding and timestamp
                var encoding = GetFileEncoding(file);
                var originalTime = File.GetLastWriteTime(file);

                var content = File.ReadAllText(file, encoding);
                var matches = _tagRegex.Matches(content);
                if (matches.Count > 0)
                    Console.WriteLine($"Found {matches.Count} matches in {Path.GetFileName(file)}");
                var replacedContent = _tagRegex.Replace(content, match =>
                {
                    Console.WriteLine($"Processing match: '{match.Value}'");
                    var result = GetMeshFromTag(match.Value);
                    Console.WriteLine($"Original: '{match.Value}'");
                    Console.WriteLine($"Result:   '{result}'");
                    Console.WriteLine($"Are they equal? {match.Value == result}");
                    return result;
                });

                Console.WriteLine($"Content changed: {content != replacedContent}");
                Console.WriteLine($"Original content length: {content.Length}");
                Console.WriteLine($"Replaced content length: {replacedContent.Length}");


                if (Math.Abs(content.Length - replacedContent.Length) < 100)
                    for (var i = 0; i < Math.Min(content.Length, replacedContent.Length); i++)
                        if (content[i] != replacedContent[i])
                        {
                            Console.WriteLine($"First difference at position {i}:");
                            Console.WriteLine(
                                $"Original: '{content.Substring(Math.Max(0, i - 10), Math.Min(20, content.Length - i + 10))}'");
                            Console.WriteLine(
                                $"Replaced: '{replacedContent.Substring(Math.Max(0, i - 10), Math.Min(20, replacedContent.Length - i + 10))}'");
                            break;
                        }

                if (content != replacedContent)
                {
                    Console.WriteLine($"Writing changes to {Path.GetFileName(file)}");
                    File.WriteAllText(file, replacedContent, new UTF8Encoding(false));
                    replacedCount++;
                }
                else
                {
                    Console.WriteLine($"No changes detected in {Path.GetFileName(file)} - content comparison failed");
                }

                processedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {file}: {ex.Message}");
            }

            progress?.Report($"Processed {processedCount}/{totalFiles} files (Replaced: {replacedCount})");

            if (processedCount % 10 == 0)
                await Task.Delay(1, cancellationToken);
        }

        progress?.Report($"Completed! Processed {totalFiles} files, {replacedCount} files were modified");
        return replacedCount;
    }

    public async Task PullNewKeywordsFromTheGame(CancellationToken cancellationToken = default,
        IProgress<string>? progress = null)
    {
        var pathToGameLocalization = repositoryManager.PathToReferenceLocalization;

        // find all files of BattleKeywords*.json
        var filesToScan =
            Directory.GetFiles(pathToGameLocalization, "BattleKeywords*.json", SearchOption.AllDirectories);

        progress?.Report($"Found {filesToScan.Length} keyword files");
        var oldKeywordCount = _keywordColorList.Count;

        // parse parse parse
        foreach (var file in filesToScan)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var deserialized =
                JsonConvert.DeserializeObject<BufLocalizationFile>(
                    await File.ReadAllTextAsync(file, cancellationToken));
            var ids = deserialized?.DataList.Select(x => x.Id) ?? [];
            foreach (var id in ids) _keywordColorList.TryAdd(id, "Unknown");
        }

        await File.WriteAllTextAsync(repositoryManager.PathToKeywordColorList,
            JsonConvert.SerializeObject(_keywordColorList), cancellationToken);
        progress?.Report($"Added {_keywordColorList.Count - oldKeywordCount} keywords");
    }

    public string? GetKeywordColor(string keyword)
    {
        var colorName = _keywordColorList[keyword];
        var color = colorName switch
        {
            "Buff" => "#f8c200",
            "Debuff" => "#e30000",
            "Status" => "#9f6a3a",
            _ => null
        };

        return color;
    }

    private static Encoding GetFileEncoding(string filename)
    {
        using var reader = new StreamReader(filename, true);
        reader.Peek();
        return reader.CurrentEncoding;
    }
}