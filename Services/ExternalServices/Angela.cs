using System.Threading;
using DeepSeek.Core;
using DeepSeek.Core.Models;
using RainbusToolbox.Models.Managers;


namespace RainbusToolbox.Services;

public class Angela(PersistentDataManager dataManager)
{
    public async Task<string?> ProcessText(string text)
    {
        var token = dataManager.Settings.DeepSeekToken;
        if(string.IsNullOrEmpty(token))
        {
            await App.Current.ShowErrorNotificationAsync(AppLang.ErrorTitle, "Invalid token, aborting");
            return text;
        }
        var client = new DeepSeekClient(token);
        var cachedPrompt = dataManager.Settings.AngelaPrompt;
        

        if (string.IsNullOrEmpty(cachedPrompt))
        {
            await App.Current.ShowErrorNotificationAsync(AppLang.ErrorTitle, "You haven't specified the prompt, aborting");
            return text;
        }

        var request = new ChatRequest()
        {
            Model = DeepSeekModels.ChatModel,
            Messages =
            {
                Message.NewSystemMessage(cachedPrompt),
                Message.NewUserMessage(text)
            }
        };
        
        Console.WriteLine("Requesting chat...");
        
        try
        {
            var response = await client.ChatAsync(request, CancellationToken.None);
            var responseText = response?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrEmpty(responseText))
            {
                await App.Current.ShowErrorNotificationAsync(AppLang.ErrorTitle, "API has returned no response, returning original text");
                return text;
            }
            Console.WriteLine(responseText);
            return responseText;
        }
        catch (Exception ex)
        {
            await App.Current.ShowErrorNotificationAsync(AppLang.ErrorTitle, $"API call failed: {ex.Message}, returning original text");
            return text;
        }
    }
}