using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls.ApplicationLifetimes;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Models.Managers;

public class GithubManager(PersistentDataManager persistentDataManager, RepositoryManager repositoryManager)
{
    private readonly HttpClient _httpClient = new();

    public bool IsOffline { get; private set; }

    public async Task<bool> IsConnectionValid()
    {
        if (string.IsNullOrWhiteSpace(persistentDataManager.Settings.GitHubToken)) IsOffline = true;

        if (!await IsTokenValidAsync(persistentDataManager.Settings.GitHubToken)) IsOffline = true;

        return !IsOffline;
    }


    // TODO: slopreview
    public async Task CreateReleaseAsync(string releaseName, string releaseDescription, string pathToZip)
    {
        if (!await IsConnectionValid())
        {
            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Ты в оффлайн режиме!",
                "Не удаётся подключится к гитхабу, поэтому увы!");
            return;
        }


        var repoPath = repositoryManager.Repository.Info.WorkingDirectory;
        var remoteUrl = repositoryManager.Repository.Network.Remotes["origin"].Url;

        // Extract owner and repo name from remote URL
        // Example: https://github.com/username/repo.git
        var segments = remoteUrl.Replace(".git", "").Split('/');
        if (segments.Length < 2)
            throw new Exception("Invalid remote URL");

        var owner = segments[^2];
        var repo = segments[^1];

        // 1. Sanitize tag for GitHub
        // Replace all invalid characters with '-'. Valid: letters, numbers, dash, underscore, dot
        var sanitizedTag = Regex.Replace(releaseName, @"[^0-9A-Za-z\-_\.]", "-");

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RainbusToolbox", "1.0"));
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", persistentDataManager.Settings.GitHubToken);

        // 2. Check if release/tag already exists
        var checkResponse =
            await http.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/tags/{sanitizedTag}");
        if (checkResponse.IsSuccessStatusCode)
            throw new Exception($"Release with tag '{sanitizedTag}' already exists.");

        // 3. Create release
        var releaseContent = new
        {
            tag_name = sanitizedTag, // must be sanitized
            name = releaseName, // display name can have spaces
            body = releaseDescription,
            draft = false,
            prerelease = false
        };

        var releaseJson =
            new StringContent(JsonSerializer.Serialize(releaseContent), Encoding.UTF8, "application/json");

        var releaseResponse =
            await http.PostAsync($"https://api.github.com/repos/{owner}/{repo}/releases", releaseJson);
        var releaseBody = await releaseResponse.Content.ReadAsStringAsync();
        if (!releaseResponse.IsSuccessStatusCode) throw new Exception($"Failed to create release: {releaseBody}");

        var releaseData = JsonDocument.Parse(releaseBody);
        var uploadUrlTemplate = releaseData.RootElement.GetProperty("upload_url").GetString();
        var uploadUrl = uploadUrlTemplate.Substring(0, uploadUrlTemplate.IndexOf("{")); // Remove template part

        // 4. Upload ZIP asset
        using var fileStream = File.OpenRead(pathToZip);
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        var assetResponse = await http.PostAsync($"{uploadUrl}?name={Path.GetFileName(pathToZip)}", content);
        var assetBody = await assetResponse.Content.ReadAsStringAsync();
        if (!assetResponse.IsSuccessStatusCode) throw new Exception($"Failed to upload ZIP asset: {assetBody}");

        // 5. Upload README.md if it exists (with HTML stripped and release description prepended)
        var readmePath = Path.Combine(repoPath, "README.md");
        if (File.Exists(readmePath))
            try
            {
                // Read the original README content
                var originalReadmeContent = await File.ReadAllTextAsync(readmePath);

                // Strip HTML tags using regex
                var htmlStrippedContent = Regex.Replace(originalReadmeContent, @"<[^>]*>", string.Empty);

                // Remove specific sections: "## Полезные ссылки" and "## Установка"
                // Split content by lines to process section by section
                var lines = htmlStrippedContent.Split('\n');
                var filteredLines = new List<string>();
                var skipSection = false;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Check if we're starting a section to skip
                    if (trimmedLine == "## Полезные ссылки" || trimmedLine == "## Установка")
                    {
                        skipSection = true;
                        continue;
                    }

                    // Check if we're starting a new ## section (end of section to skip)
                    if (trimmedLine.StartsWith("## ") && skipSection) skipSection = false;
                    // Don't skip this line, it's a new section
                    // Add line if we're not skipping
                    if (!skipSection) filteredLines.Add(line);
                }

                htmlStrippedContent = string.Join('\n', filteredLines);

                // Clean up extra whitespace that might be left after HTML removal and section removal
                htmlStrippedContent =
                    Regex.Replace(htmlStrippedContent, @"\n\s*\n\s*\n",
                        "\n\n"); // Replace multiple empty lines with double newline
                htmlStrippedContent = htmlStrippedContent.Trim();

                // Prepend release description
                var modifiedReadmeContent =
                    $"# Release: {releaseName}\n\n{releaseDescription}\n\n---\n\n{htmlStrippedContent}";

                // Convert to byte array for upload
                var readmeBytes = Encoding.UTF8.GetBytes(modifiedReadmeContent);
                using var readmeStream = new MemoryStream(readmeBytes);
                using var readmeContent = new StreamContent(readmeStream);
                readmeContent.Headers.ContentType = new MediaTypeHeaderValue("text/markdown");

                var readmeResponse = await http.PostAsync($"{uploadUrl}?name=README.md", readmeContent);
                var readmeResponseBody = await readmeResponse.Content.ReadAsStringAsync();
                if (!readmeResponse.IsSuccessStatusCode)
                    // Log warning but don't fail the entire operation
                    Log.Debug($"Warning: Failed to upload README.md: {readmeResponseBody}");
            }
            catch (Exception ex)
            {
                Log.Debug($"Warning: Error processing README.md: {ex.Message}");
            }
        else
            // Log info that README.md was not found
            Log.Debug("README.md not found in repository root, skipping upload.");
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
            return false;
        }
    }

    public async Task<string> GetGithubDisplayNameAsync()
    {
        if (!await IsConnectionValid()) return "Offline";

        try
        {
            using var http = new HttpClient();
            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var token = persistentDataManager.Settings.GitHubToken;
            http.DefaultRequestHeaders.UserAgent.ParseAdd("RainbusToolbox/1.0");
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await http.GetAsync("https://api.github.com/user");
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
            return "Unknown";
        }
    }
}