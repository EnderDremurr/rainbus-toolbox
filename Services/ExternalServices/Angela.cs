using System.Threading;
using DeepSeek.Core;
using DeepSeek.Core.Models;

namespace RainbusToolbox.Services.ExternalServices;

public sealed class Angela(PersistentDataManager dataManager, DialogService dialogService)
{
    public async Task<string?> ProcessText(string text)
    {
        var token = dataManager.Settings.DeepSeekToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _ = dialogService.ShowPopupAsync("Ошибка", "Нерабочий токен");
            return text;
        }

        var client = new DeepSeekClient(token);
        var cachedPrompt = dataManager.Settings.AngelaPrompt;

        if (string.IsNullOrWhiteSpace(cachedPrompt))
        {
            _ = dialogService.ShowPopupAsync("Ошибка", "Не найден промпт в настройках");
            return text;
        }

        var request = new ChatRequest
        {
            Model = DeepSeekModels.ChatModel,
            Messages =
            {
                Message.NewSystemMessage(cachedPrompt),
                Message.NewUserMessage(text)
            }
        };

        Log.Debug("Requesting chat...");

        try
        {
            var response = await client.ChatAsync(request, CancellationToken.None);
            var responseText = response?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                _ = dialogService.ShowPopupAsync("Ошибка", "Не удалось подключится к API");
                return text;
            }

            Log.Debug(responseText);
            return responseText;
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex, "С дипсиком какая-то хуйня");
            return text;
        }
    }
}