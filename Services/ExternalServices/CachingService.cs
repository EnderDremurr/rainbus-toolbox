using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using RainbusToolbox.Models.Managers;
using Serilog;

namespace RainbusToolbox.Services.ExternalServices;

public class CachingService
{
    private const string EGOGiftWikiCategory = "Category:E.G.O_Gifts";
    private const string AnnouncersWikiCategory = "Category:Battle_Announcer_Icons";
    private readonly CancellationToken _appCancellationToken;

    private readonly Dictionary<string, string> _cachePathMap =
        new()
        {
            { EGOGiftWikiCategory, "egogifts" },
            { AnnouncersWikiCategory, "announcers" }
        };

    private readonly CancellationTokenSource _cts = new();


    private readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders = { { "User-Agent", "RainbusToolbox/1.0" } }
    };

    public CachingService(PersistentDataManager persistentDataManager)
    {
        //check if user hasn't initialized the game yet
        if (string.IsNullOrWhiteSpace(persistentDataManager.Settings.PathToLimbus))
            return;

        _appCancellationToken = _cts.Token;
        _ = Task.Run(() => SyncCacheAsync("Category:E.G.O_Gifts", _appCancellationToken));
        _ = Task.Run(() => SyncCacheAsync("Category:Battle_Announcer_Icons", _appCancellationToken));
    }

    private string _baseCachePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RainbusToolbox", "cache");


    public async Task SyncCacheAsync(string categoryTitle, CancellationToken ct)
    {
        Log.Debug($"Starting cache sync for {categoryTitle}");
        Directory.CreateDirectory(Path.Combine(_baseCachePath, _cachePathMap[categoryTitle]));

        var allFiles = await FetchAllCategoryMembersFromWikiAsync(categoryTitle, ct);
        var missing = allFiles.Where(name => !File.Exists(
            Path.Combine(_baseCachePath, _cachePathMap[categoryTitle], name.Replace("File:", "")))).ToList();

        Log.Debug($"{missing}/{allFiles} files need downloading for {categoryTitle}");
        if (!missing.Any()) return;

        var urls = await BatchResolveUrlsAsync(missing, ct);
        await DownloadAllAsync(urls, _cachePathMap[categoryTitle], ct);
    }

    public async Task<List<string>> FetchAllCategoryMembersFromWikiAsync(string categoryTitle, CancellationToken ct)
    {
        var results = new List<string>();
        string? continueToken = null;

        do
        {
            var url = "https://limbuscompany.wiki.gg/api.php" +
                      "?action=query&list=categorymembers" +
                      "&cmtitle=" + Uri.EscapeDataString(categoryTitle) +
                      "&cmtype=file&cmlimit=500&format=json";

            if (continueToken != null)
                url += $"&cmcontinue={continueToken}";
            Log.Debug($"Fetching category members from wiki for {categoryTitle}");
            var json = await _httpClient.GetStringAsync(url, ct);
            Log.Debug($"Got response for {categoryTitle}");
            var doc = JsonDocument.Parse(json);

            var members = doc.RootElement
                .GetProperty("query")
                .GetProperty("categorymembers")
                .EnumerateArray();

            foreach (var member in members)
                results.Add(member.GetProperty("title").GetString()!);

            continueToken = doc.RootElement.TryGetProperty("continue", out var cont)
                ? cont.GetProperty("cmcontinue").GetString()
                : null;
        } while (continueToken != null);

        return results;
    }

    public async Task<Dictionary<string, string>> BatchResolveUrlsAsync(List<string> fileNames, CancellationToken ct)
    {
        var result = new Dictionary<string, string>();

        foreach (var chunk in fileNames.Chunk(50))
        {
            var titles = string.Join("|", chunk);
            var url = "https://limbuscompany.wiki.gg/api.php" +
                      $"?action=query&titles={Uri.EscapeDataString(titles)}" +
                      "&prop=imageinfo&iiprop=url&format=json";

            var json = await _httpClient.GetStringAsync(url, ct);
            var doc = JsonDocument.Parse(json);

            var pages = doc.RootElement
                .GetProperty("query")
                .GetProperty("pages")
                .EnumerateObject();

            foreach (var page in pages)
            {
                var title = page.Value.GetProperty("title").GetString()!;
                var imageUrl = page.Value
                    .GetProperty("imageinfo")[0]
                    .GetProperty("url")
                    .GetString()!;

                result[title] = imageUrl;
            }
        }

        return result;
    }

    public async Task DownloadAllAsync(Dictionary<string, string> urls, string cacheSubfolder, CancellationToken ct)
    {
        var semaphore = new SemaphoreSlim(3);

        var tasks = urls.Select(async kvp =>
        {
            var fileName = kvp.Key.Replace("File:", "");
            var savePath = Path.Combine(_baseCachePath, cacheSubfolder, fileName);

            if (File.Exists(savePath)) return;

            await semaphore.WaitAsync(ct);
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(kvp.Value, ct);
                await File.WriteAllBytesAsync(savePath, bytes, ct);
                Log.Debug($"Downloaded {fileName}");
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}