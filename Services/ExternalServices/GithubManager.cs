using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls.ApplicationLifetimes;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Models.Managers;

public sealed partial class GithubManager
{
    private const string RemoteParseRegex = "[:/](?<owner>[^/:]+)/(?<repo>[^/]+?)(?:\\.git)?/?$";


    private const string ReleaseFooterFileName = "RELEASE_FOOTER.md";
    private readonly GitManager _gitManager;

    private readonly HttpClient _httpClient = new();
    private readonly PersistentDataManager _persistentDataManager;

    public GithubManager(PersistentDataManager persistentDataManager,
        GitManager gitManager)
    {
        _persistentDataManager = persistentDataManager;
        _gitManager = gitManager;

        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RainbusToolbox", "1.0"));
    }


    public bool IsOffline { get; private set; }

    public async Task<bool> IsConnectionValid()
    {
        IsOffline =
            string.IsNullOrWhiteSpace(_persistentDataManager.Settings.GitHubToken)
            || !await IsTokenValidAsync(_persistentDataManager.Settings.GitHubToken);

        return !IsOffline;
    }

    private static (string Owner, string Repo) ParseOwnerAndRepoFromUrl(string remoteUrl)
    {
        var match = Regex.Match(remoteUrl, RemoteParseRegex);
        if (!match.Success)
            throw new Exception($"Can't parse remote URL: {remoteUrl}");

        var owner = match.Groups["owner"].Value;
        var repo = match.Groups["repo"].Value;

        return (owner, repo);
    }


    // TODO: slopreview
    public async Task<string?>
        CreateReleaseAsync(string releaseName, string releaseDescription,
            string pathToZip, bool appendReleaseFooter) // return link to the release if successful
    {
        if (!await IsConnectionValid())
        {
            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Ты в оффлайн режиме!",
                "Не удаётся подключится к гитхабу, поэтому увы!");
            return null;
        }

        var repoPath = _gitManager.Repository.Info.WorkingDirectory;
        var remoteUrl = _gitManager.Repository.Network.Remotes["origin"].Url;

        string owner, repo;
        try
        {
            (owner, repo) = ParseOwnerAndRepoFromUrl(remoteUrl);
        }
        catch (Exception e)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(e);
            return null;
        }

        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _persistentDataManager.Settings.GitHubToken);

        // sanitize illegal symbols in release name (need sani cuz release name is not a version, but a repo name + version)
        var sanitizedTag = TagSanitizerRegex().Replace(releaseName, "-");


        // ensure it won't create a dupe release
        var checkResponse =
            await _httpClient.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/tags/{sanitizedTag}");
        if (checkResponse.IsSuccessStatusCode)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(
                new Exception(
                    $"Релиз с тегом '{sanitizedTag}' уже существует! Возможно, что-то создало релиз параллельно!"));
            return null;
        }

        // prepare release data and try creating a release at the remote
        var releaseContent = new
        {
            tag_name = sanitizedTag, // must be sanitized
            name = releaseName, // display name can have spaces
            body = releaseDescription,
            draft = false,
            prerelease = false
        }; // TODO: make release a draft first, then publish if it didn't throw (later)
        var releaseJson =
            new StringContent(JsonSerializer.Serialize(releaseContent), Encoding.UTF8, "application/json");
        var releaseResponse =
            await _httpClient.PostAsync($"https://api.github.com/repos/{owner}/{repo}/releases", releaseJson);
        var releaseBody = await releaseResponse.Content.ReadAsStringAsync();
        if (!releaseResponse.IsSuccessStatusCode)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(new Exception($"Failed to create release: {releaseBody}"));
            return null;
        }

        var releaseData = JsonDocument.Parse(releaseBody);
        var uploadUrlTemplate = releaseData.RootElement.GetProperty("upload_url").GetString();
        var uploadUrl = uploadUrlTemplate.Substring(0, uploadUrlTemplate.IndexOf("{")); // Remove template part
        await using var fileStream = File.OpenRead(pathToZip);
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        var assetResponse = await _httpClient.PostAsync($"{uploadUrl}?name={Path.GetFileName(pathToZip)}", content);
        var assetBody = await assetResponse.Content.ReadAsStringAsync();
        if (!assetResponse.IsSuccessStatusCode) throw new Exception($"Failed to upload ZIP asset: {assetBody}");


        // upload a Readme.md (required by the launcher) containing the release description + a special footer copy if user has enabled it
        var readmeString = $"# Release: {releaseName}\n\n{releaseDescription}";

        if (appendReleaseFooter)
        {
            var readmeFooterPath = Path.Combine(repoPath, ReleaseFooterFileName);

            if (!File.Exists(readmeFooterPath))
                _ = App.Current.HandleNonFatalExceptionAsync(
                    new Exception(
                        $"Была указана опция добавить футер к README.md, однако файл футера отсутствует! Требуется файл по пути {readmeFooterPath}!" +
                        $"\nБудет использовано обычное описание релиза."));
            else
                try
                {
                    var footerText = await File.ReadAllTextAsync(readmeFooterPath);
                    footerText = footerText.Trim();
                    if (!string.IsNullOrEmpty(footerText))
                        readmeString += $"\n\n---\n\n{footerText}";
                    else
                        _ = App.Current.HandleNonFatalExceptionAsync(
                            new Exception("Твой файл с футером для README.md пустой!!!"));
                }
                catch (Exception e)
                {
                    _ = App.Current.HandleNonFatalExceptionAsync(
                        new Exception("Произошла ошибка при чтении футера! Это не помешает релизу!", e));
                }
        }

        var readmeBytes = Encoding.UTF8.GetBytes(readmeString);
        using var readmeStream = new MemoryStream(readmeBytes);
        using var readmeContent = new StreamContent(readmeStream);
        readmeContent.Headers.ContentType = new MediaTypeHeaderValue("text/markdown");

        var readmeResponse = await _httpClient.PostAsync($"{uploadUrl}?name=README.md", readmeContent);
        var readmeResponseBody = await readmeResponse.Content.ReadAsStringAsync();
        if (!readmeResponse.IsSuccessStatusCode)
            _ = App.Current.HandleNonFatalExceptionAsync(
                new Exception(
                    $"Не удалось загрузить README.md для релиза! Информация: {readmeResponseBody}\nСам релиз был успешно выложен!"));

        if (releaseData.RootElement.TryGetProperty("html_url", out var urlElement))
            return urlElement.GetString();
        _ = App.Current.HandleNonFatalExceptionAsync(new Exception(
            $"Релиз был успешно выложен, но не удалось получить ссылку релиза, это не страшно! Информация: {releaseBody}"));
        return string.Empty;
        // returns link or empty string if release succeeded (empty if something's wrong with the link) and null if it's failed
    }

    public static async Task<bool> IsTokenValidAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("RainbusToolbox/1.0");
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("https://api.github.com/rate_limit");
            return response.IsSuccessStatusCode;
        }
        catch (Exception e)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(e);
            return false;
        }
    }

    public async Task<string> GetGithubDisplayNameAsync()
    {
        if (!await IsConnectionValid()) return "Offline";

        try
        {
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var token = _persistentDataManager.Settings.GitHubToken;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync("https://api.github.com/user");
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Failed to get GitHub user info: {response.StatusCode}");
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);


            var displayName = "";
            if (doc.RootElement.TryGetProperty("name", out var nameProp) &&
                !string.IsNullOrWhiteSpace(nameProp.GetString())) displayName = nameProp.GetString();

            if (string.IsNullOrWhiteSpace(displayName)) displayName = doc.RootElement.GetProperty("login").GetString();

            return displayName ?? "Unknown";
        }
        catch (Exception e)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(e);
            return "Unknown";
        }
    }

    [GeneratedRegex(@"[^0-9A-Za-z\-_\.]")]
    private static partial Regex TagSanitizerRegex();
}