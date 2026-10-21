using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Newtonsoft.Json;
using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.ViewModels.Misc;

namespace RainbusToolbox.Services.RepositoryServices;

//TODO: this entire class is a fucking mess
public sealed partial class KeywordProcessingService(
    LocalizationManager localizationManager,
    DialogService dialogService)
{
    private const string RCRKeywordColorsLink =
        "https://raw.githubusercontent.com/Let-It-Rain/RCR-LCB/refs/heads/main/keyword_colors.json";

    private const string MTLKeywordColorsLink =
        "https://raw.githubusercontent.com/kimght/LimbusCompanyRuMTL/main/data/build/keyword_colors.txt";

    private static readonly Regex MeshRegex = new(
        @"<sprite name=\\?""(?<spriteKey>[^""\\]+)\\?"">" +
        @"<color=(?<color>[^>]+)><u>" +
        @"<link=\\?""(?<linkKey>[^""\\]+)\\?"">" +
        @"(?:<noparse>)?(?<fill>.*?)(?:</noparse>)?</link></u></color>",
        RegexOptions.Singleline | RegexOptions.Compiled);

    private static readonly Regex SpacesBetweenTagsRegex = new(@">\s+<", RegexOptions.Compiled);

    private readonly HttpClient _httpClient = new();

    private readonly Regex _tagRegex = TagRegex();

    private bool _isInitialized;

    private Dictionary<string, string> _keywordColorList = [];

    private static string ConvertMeshMatch(Match m)
    {
        var spriteKey = m.Groups["spriteKey"].Value;
        if (!string.Equals(spriteKey, m.Groups["linkKey"].Value, StringComparison.Ordinal))
            return m.Value;

        var fill = m.Groups["fill"].Value;

        return fill == spriteKey
            ? $"[{spriteKey}]"
            : $"[{spriteKey}:*{fill}*]";
    }

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await InitializeAsync();
        _isInitialized = true;
    }


    public async Task InitializeAsync()
    {
        var keywordColorPath = localizationManager.PathToKeywordColorList;

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

    public async Task DownloadMTLKeywordColorsAndOverrideCurrentAsync()
    {
        try
        {
            var content = await _httpClient.GetStringAsync(MTLKeywordColorsLink);

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StringReader(content);
            while (await reader.ReadLineAsync() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split('¤', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    continue;

                var key = parts[0].Trim();
                var value = GetColorName(parts[1].Trim());
                dict.TryAdd(key, value);
            }

            var keywordColorPath = localizationManager.PathToKeywordColorList;
            File.Delete(keywordColorPath);
            await File.WriteAllTextAsync(keywordColorPath, JsonConvert.SerializeObject(dict, Formatting.Indented));
        }
        catch (Exception e)
        {
            await App.Current.HandleNonFatalExceptionAsync(e);
        }
    }

    public async Task AskToDefineKeywords()
    {
        await EnsureInitializedAsync();

        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (_keywordColorList.All(c => c.Value != "Unknown"))
        {
            await dialogService.ShowPopupAsync("Определение цвета кейвордов",
                "Все кейворды имеют цвет, ничего делать не нужно!");
            return;
        }

        var colorlessKeywords = _keywordColorList.Where(c => c.Value == "Unknown");
        foreach (var colorlessKeyword in colorlessKeywords)
        {
            var response = await dialogService.ShowPopupAsync("Определение цвета кейвордов",
                $"Обнаружен новый кейворд {colorlessKeyword.Key}, укажи его цвет",
                false,
                "",
                null,
                new PopupButton
                {
                    Label = "Дебафф (Красный)",
                    ResultValue = "Debuff"
                }, new PopupButton
                {
                    Label = "Баф (Жёлтый)",
                    ResultValue = "Buff"
                }, new PopupButton
                {
                    Label = "Статус (Коричневый)",
                    ResultValue = "Status"
                }
            );

            if (response != null)
                _keywordColorList[colorlessKeyword.Key] = response.Result!;
        }

        await File.WriteAllTextAsync(localizationManager.PathToKeywordColorList,
            JsonConvert.SerializeObject(_keywordColorList, Formatting.Indented));
        await dialogService.ShowPopupAsync("Определение цвета кейвордов",
            "Все кейворды были определены!");
    }

    // TODO: Slopreview
    public string GetMeshFromTag(string tag)
    {
        // TODO: replace this shit with a proper regex later, cuz this fucker is huge 
        // Validate the tag format
        if (string.IsNullOrWhiteSpace(tag) || tag.Length < 2 || tag[0] != '[' || tag[^1] != ']')
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


    // TODO: this can also be made parallel like i did with regexes, should become way way faster, but it's not something i need to do right away
    public async Task<int> ReplaceEveryTagWithMeshAsync(string path,
        IProgress<(int ProcessedFiles, int TotalFiles, int ReplacementsCount)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        await EnsureInitializedAsync();

        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;

        progress?.Report((0, totalFiles, 0));

        var processedCount = 0;
        var replacedCount = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var encoding = GetFileEncoding(file);

                var content = await File.ReadAllTextAsync(file, encoding, cancellationToken);
                var replacedContent = _tagRegex.Replace(content, match =>
                {
                    var result = GetMeshFromTag(match.Value);
                    return result;
                });

                if (content != replacedContent)
                {
                    await File.WriteAllTextAsync(file, replacedContent, new UTF8Encoding(false), cancellationToken);
                    replacedCount++;
                }

                processedCount++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _ = App.Current.HandleNonFatalExceptionAsync(new Exception($"Error processing {file}", ex));
            }

            progress?.Report((processedCount, totalFiles, replacedCount));

            if (processedCount % 10 == 0)
                await Task.Delay(1, cancellationToken);
        }

        progress?.Report((processedCount, totalFiles, replacedCount));
        return replacedCount;
    }

    public static string RevertMeshToTag(string mesh)
    {
        if (string.IsNullOrEmpty(mesh))
            return mesh;

        var cleanedMesh = SpacesBetweenTagsRegex.Replace(mesh, "><"); // remove spaces between tags cuz it breaks shit

        return MeshRegex.Replace(cleanedMesh, ConvertMeshMatch);
    }
    //TODO: do the progress reporting more cleanly, this shit sucks ass

    public async Task<int> ReplaceEveryMeshWithTagAsync(
        string path,
        IProgress<(int ProcessedFiles, int TotalFiles, int ReplacementsCount)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        await EnsureInitializedAsync();

        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories).ToList();
        var totalFiles = files.Count;

        progress?.Report((0, totalFiles, 0));

        var processedCount = 0;
        var replacedCount = 0;

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var encoding = GetFileEncoding(file);

                var content = await File.ReadAllTextAsync(
                    file, encoding, cancellationToken);

                var replacedContent = RevertMeshToTag(content);

                if (content != replacedContent)
                {
                    await File.WriteAllTextAsync(
                        file,
                        replacedContent,
                        new UTF8Encoding(false),
                        cancellationToken);

                    replacedCount++;
                }

                processedCount++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _ = App.Current.HandleNonFatalExceptionAsync(
                    new Exception($"Error processing {file}", ex));
            }

            progress?.Report((processedCount, totalFiles, replacedCount));

            if (processedCount % 10 == 0 && processedCount > 0)
                await Task.Delay(1, cancellationToken);
        }

        progress?.Report((processedCount, totalFiles, replacedCount));
        return replacedCount;
    }


    public async Task PullNewKeywordsFromTheGame(IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();

        var pathToGameLocalization = localizationManager.PathToReferenceLocalization;

        Log.Debug("Starting to add new keywords from the game. " +
                  "Path to origin: {PathToGameLocalization}, " +
                  "does exist: {Exists}",
            pathToGameLocalization, Directory.Exists(pathToGameLocalization));

        // find all files of BattleKeywords*.json
        var filesToScan =
            Directory.GetFiles(pathToGameLocalization, "*BattleKeywords*.json", SearchOption.AllDirectories);

        progress?.Report($"Found {filesToScan.Length} keyword files");
        var oldKeywordCount = _keywordColorList.Count;

        // parse parse parse
        foreach (var file in filesToScan)
        {
            Log.Debug("Scanning file {File}", file);
            cancellationToken.ThrowIfCancellationRequested();
            var deserialized =
                JsonConvert.DeserializeObject<BufLocalizationFile>(
                    await File.ReadAllTextAsync(file, cancellationToken));
            var ids = deserialized?.DataList.Select(x => x.Id) ?? [];
            foreach (var id in ids)
            {
                Log.Debug("Found new keyword: {Id}", id);
                _keywordColorList.TryAdd(id, "Unknown");
            }
        }

        await File.WriteAllTextAsync(localizationManager.PathToKeywordColorList,
            JsonConvert.SerializeObject(_keywordColorList, Formatting.Indented), cancellationToken);
        progress?.Report($"Added {_keywordColorList.Count - oldKeywordCount} keywords");
    }

    public static string GetColorName(string hex)
    {
        var color = hex switch
        {
            "#f8c200" => "Buff",
            "#e30000" => "Debuff",
            "#9f6a3a" => "Status",
            _ => "Unknown"
        };

        return color;
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

    [GeneratedRegex(@"\[[^:\]]+:[`*'][^`*']+[`*']\]", RegexOptions.Compiled)]
    private static partial Regex TagRegex();
}