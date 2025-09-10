using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MsBox.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models; // for IClipboard


namespace RainbusToolbox.Models.Managers;

public class GithubManager
{
    private const string ClientId = "Ov23libxwWDK2iVhx2Zg";
    private const string Scope = "repo";
    
    private PersistentDataManager _dataManager;
    private RepositoryManager _repositoryManager;
    public GithubManager(PersistentDataManager manager, RepositoryManager repositoryManager)
    {
        _dataManager = manager;
        _repositoryManager = repositoryManager;
        TryInitialize();
    }

    public void TryInitialize()
    {
        if (string.IsNullOrEmpty(_dataManager.Settings.GitHubToken))
            return;
    }
    
    public async Task RequestGithubAuthAsync(Window window)
    {

        using var http = new HttpClient();
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


        // Step 1: Request device/user codes
        var deviceResp = await http.PostAsync(
            "https://github.com/login/device/code",
            new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("client_id", ClientId),
                new KeyValuePair<string,string>("scope", Scope),
            }));

        if (!deviceResp.IsSuccessStatusCode)
        {
            // TODO: Show MsBox error: "Failed to request device code from GitHub."
            return;
        }

        var deviceJson = JsonDocument.Parse(await deviceResp.Content.ReadAsStringAsync());

        string userCode = deviceJson.RootElement.GetProperty("user_code").GetString();
        string verificationUri = deviceJson.RootElement.GetProperty("verification_uri").GetString();
        string deviceCode = deviceJson.RootElement.GetProperty("device_code").GetString();
        int expiresIn = deviceJson.RootElement.GetProperty("expires_in").GetInt32();
        int interval = deviceJson.RootElement.GetProperty("interval").GetInt32();

        // Launch browser to verification page
        Process.Start(new ProcessStartInfo
        {
            FileName = verificationUri,
            UseShellExecute = true
        });
        


        // TODO: Show MsBox info: 
        // $"To authorize the app, go to {verificationUri} and enter code: {userCode}"
        await ShowUserCodeBox(window, userCode);


        // Step 2: Poll for access token
        string token = null;
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start).TotalSeconds < expiresIn)
        {
            await Task.Delay(interval * 1000);

            var tokenResp = await http.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string,string>("client_id", ClientId),
                    new KeyValuePair<string,string>("device_code", deviceCode),
                    new KeyValuePair<string,string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code")
                }));

            if (!tokenResp.IsSuccessStatusCode)
            {
                // TODO: Show MsBox error: "Failed to request access token from GitHub."
                return;
            }

            var tokenJson = JsonDocument.Parse(await tokenResp.Content.ReadAsStringAsync());

            if (tokenJson.RootElement.TryGetProperty("access_token", out var accessTokenProp))
            {
                token = accessTokenProp.GetString();
                break;
            }

            if (tokenJson.RootElement.TryGetProperty("error", out var errorProp) &&
                errorProp.GetString() != "authorization_pending")
            {
                var errorMessage = MessageBoxManager.GetMessageBoxStandard("Ошибочка", "Гитхаб насрал ошибкой");
                errorMessage.ShowAsync();
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            var errorMessage = MessageBoxManager.GetMessageBoxStandard("Ошибочка", "Гитхаб насрал ошибкой");
            // errorMessage.ShowAsync();
            return;
        }

        // Persist token
        _dataManager.Settings.GitHubToken = token;
        _dataManager.Save();
    }



public async Task CreateReleaseAsync(string releaseName, string releaseDescription, string pathToZip)
{
    var repoPath = _repositoryManager.Repository.Info.WorkingDirectory;
    var remoteUrl = _repositoryManager.Repository.Network.Remotes["origin"].Url;

    // Extract owner and repo name from remote URL
    // Example: https://github.com/username/repo.git
    var segments = remoteUrl.Replace(".git", "").Split('/');
    if (segments.Length < 2)
        throw new Exception("Invalid remote URL");

    string owner = segments[^2];
    string repo = segments[^1];

    // 1. Sanitize tag for GitHub
    // Replace all invalid characters with '-'. Valid: letters, numbers, dash, underscore, dot
    var sanitizedTag = Regex.Replace(releaseName, @"[^0-9A-Za-z\-_\.]", "-");

    using var http = new HttpClient();
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
    http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RainbusToolbox", "1.0"));
    http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", _dataManager.Settings.GitHubToken);

    // 2. Check if release/tag already exists
    var checkResponse = await http.GetAsync($"https://api.github.com/repos/{owner}/{repo}/releases/tags/{sanitizedTag}");
    if (checkResponse.IsSuccessStatusCode)
    {
        throw new Exception($"Release with tag '{sanitizedTag}' already exists.");
    }

    // 3. Create release
    var releaseContent = new
    {
        tag_name = sanitizedTag,        // must be sanitized
        name = releaseName,             // display name can have spaces
        body = releaseDescription,
        draft = false,
        prerelease = false
    };

    var releaseJson = new StringContent(JsonSerializer.Serialize(releaseContent), Encoding.UTF8, "application/json");

    var releaseResponse = await http.PostAsync($"https://api.github.com/repos/{owner}/{repo}/releases", releaseJson);
    var releaseBody = await releaseResponse.Content.ReadAsStringAsync();
    if (!releaseResponse.IsSuccessStatusCode)
    {
        throw new Exception($"Failed to create release: {releaseBody}");
    }

    var releaseData = JsonDocument.Parse(releaseBody);
    var uploadUrlTemplate = releaseData.RootElement.GetProperty("upload_url").GetString();
    var uploadUrl = uploadUrlTemplate.Substring(0, uploadUrlTemplate.IndexOf("{")); // Remove template part

    // 4. Upload asset (ZIP)
    using var fileStream = File.OpenRead(pathToZip);
    using var content = new StreamContent(fileStream);
    content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

    var assetResponse = await http.PostAsync($"{uploadUrl}?name={Path.GetFileName(pathToZip)}", content);
    var assetBody = await assetResponse.Content.ReadAsStringAsync();
    if (!assetResponse.IsSuccessStatusCode)
    {
        throw new Exception($"Failed to upload asset: {assetBody}");
    }
}

    
    public async Task ShowUserCodeBox(Window parent, string userCode)
    {
        var box = MessageBoxManager.GetMessageBoxCustom(
            new MessageBoxCustomParams
            {
                ContentTitle = "Нужна авторизация",
                ContentMessage = $"Проге нужен токен с GitHub.\n" +
                                 $"В открытой вкладке нужно вписать код:\n\n{userCode}\n\n" +
                                 "Можешь нажать «Copy» чтобы скопировать код в буфер обмена.",
                ButtonDefinitions = new[]
                {
                    new ButtonDefinition { Name = "Copy", IsDefault = true },
                    new ButtonDefinition { Name = "OK" }
                },
                Icon = Icon.Info,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            });

        var result = await box.ShowWindowDialogAsync(parent);

        if (result == "Copy")
        {
            var clipboard = TopLevel.GetTopLevel(parent)?.Clipboard;
            if (clipboard != null)
            {
                await clipboard.SetTextAsync(userCode);
            }
        }
    }
    
    public async Task<string> GetGithubDisplayNameAsync()
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
    
        var token = _dataManager.Settings.GitHubToken;
        // GitHub requires a User-Agent header
        http.DefaultRequestHeaders.UserAgent.ParseAdd("RainbusToolbox/1.0");

        // Add the OAuth token
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await http.GetAsync("https://api.github.com/user");
    
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get GitHub user info: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Try to get the display name
        string displayName = null;
        if (doc.RootElement.TryGetProperty("name", out var nameProp) && !string.IsNullOrWhiteSpace(nameProp.GetString()))
        {
            displayName = nameProp.GetString();
        }

        // Fallback to login if display name is not set
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = doc.RootElement.GetProperty("login").GetString();
        }

        return displayName;
    }

}