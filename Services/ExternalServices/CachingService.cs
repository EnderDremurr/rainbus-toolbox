using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Services.ExternalServices;

public class CachingService
{
    private const string EGOGiftWikiCategory = "Category:E.G.O_Gifts";
    private const string AnnouncersWikiCategory = "Category:Battle_Announcer_Icons";

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

        try
        {
            var appCancellationToken = _cts.Token;
            _ = Task.Run(() => SyncCacheAsync("Category:E.G.O_Gifts", appCancellationToken));
            _ = Task.Run(() => SyncCacheAsync("Category:Battle_Announcer_Icons", appCancellationToken));
        }
        catch
        {
            // ignored
        }
    }

    private string BaseCachePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RainbusToolbox", "cache");


    public async Task SyncCacheAsync(string categoryTitle, CancellationToken ct)
    {
        Log.Debug("Starting cache sync for {CategoryTitle}", categoryTitle);
        Directory.CreateDirectory(Path.Combine(BaseCachePath, _cachePathMap[categoryTitle]));

        var allFiles = await FetchAllCategoryMembersFromWikiAsync(categoryTitle, ct);
        var missing = allFiles.Where(name => !File.Exists(
            Path.Combine(BaseCachePath, _cachePathMap[categoryTitle], name.Replace("File:", "")))).ToList();

        Log.Debug("{Missing}/{AllFiles} files need downloading for {CategoryTitle}", missing, allFiles, categoryTitle);
        if (!missing.Any()) return;

        var urls = await BatchResolveUrlsAsync(missing, ct);
        await DownloadAllAsync(urls, _cachePathMap[categoryTitle], ct);
    }

    public async Task<List<string>> FetchAllCategoryMembersFromWikiAsync(string categoryTitle, CancellationToken ct)
    {
        try
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
                Log.Debug("Fetching category members from wiki for {CategoryTitle}", categoryTitle);
                var json = await _httpClient.GetStringAsync(url, ct);
                Log.Debug("Got response for {CategoryTitle}", categoryTitle);
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
        catch (Exception e)
        {
            return [];
        }
    }

    public async Task<Dictionary<string, string>> BatchResolveUrlsAsync(List<string> fileNames, CancellationToken ct)
    {
        try
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
        catch (Exception e)
        {
            return [];
        }
    }

    public async Task DownloadAllAsync(Dictionary<string, string> urls, string cacheSubfolder, CancellationToken ct)
    {
        var semaphore = new SemaphoreSlim(3);

        var tasks = urls.Select(async kvp =>
        {
            var fileName = kvp.Key.Replace("File:", "");
            var savePath = Path.Combine(BaseCachePath, cacheSubfolder, fileName);

            if (File.Exists(savePath)) return;

            await semaphore.WaitAsync(ct);
            try
            {
                var bytes = await _httpClient.GetByteArrayAsync(kvp.Value, ct);
                await File.WriteAllBytesAsync(savePath, bytes, ct);
                Log.Debug("Downloaded {FileName}", fileName);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }
}