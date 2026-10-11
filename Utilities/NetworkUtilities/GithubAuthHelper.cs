using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RainbusToolbox.Utilities.NetworkUtilities;

public static class GithubAuthHelper
{
    private const string ClientId = "Ov23libxwWDK2iVhx2Zg";
    private const string Scope = "repo";

    private static readonly HttpClient HttpClient = new();

    public static async Task<string> RequestGithubAuthAsync(Func<string, Task> codeCallback)
    {
        HttpClient.DefaultRequestHeaders.Accept.Clear();
        HttpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        // first takes the code json
        var deviceCodeResponse = await HttpClient.PostAsync(
            "https://github.com/login/device/code",
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("scope", Scope)
            ]));
        if (!deviceCodeResponse.IsSuccessStatusCode) throw new Exception("Не удалось получить код авторизации");
        using var deviceJson = JsonDocument.Parse(await deviceCodeResponse.Content.ReadAsStringAsync());

        var userCode = deviceJson.RootElement.GetProperty("user_code").GetString();
        var verificationUri = deviceJson.RootElement.GetProperty("verification_uri").GetString();
        var deviceCode = deviceJson.RootElement.GetProperty("device_code").GetString();
        var expiresIn = deviceJson.RootElement.GetProperty("expires_in").GetInt32();
        var interval = deviceJson.RootElement.GetProperty("interval").GetInt32();

        if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(verificationUri) || string.IsNullOrEmpty(deviceCode))
            throw new Exception("Гитхаб вернул невалидный ответ");

        Process.Start(new ProcessStartInfo
        {
            FileName = verificationUri,
            UseShellExecute = true
        });

        // popup window is now moved to callback, so that actual helper won't touch the view
        await codeCallback(userCode);

        // polling till token is recieved
        var timeOfStart = DateTime.UtcNow;
        while ((DateTime.UtcNow - timeOfStart).TotalSeconds < expiresIn)
        {
            var tokenResp = await HttpClient.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent([
                    new KeyValuePair<string, string>("client_id", ClientId),
                    new KeyValuePair<string, string>("device_code", deviceCode),
                    new KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:device_code")
                ]));

            if (!tokenResp.IsSuccessStatusCode) throw new Exception("Не удалось получить код авторизации");

            using var tokenJson = JsonDocument.Parse(await tokenResp.Content.ReadAsStringAsync());

            if (tokenJson.RootElement.TryGetProperty("error", out var errorProp) &&
                errorProp.GetString() != "authorization_pending")
                throw new Exception("Не удалось авторизоваться");

            if (!tokenJson.RootElement.TryGetProperty("access_token", out var accessTokenProp))
            {
                await Task.Delay(interval * 1000);
                continue;
            }

            var accessToken = accessTokenProp.GetString();
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception(
                    "Гитхаб вернул пустой токен");
            return accessToken;
        }

        throw new Exception("Что-то пошло не так при получении токена");
    }
}