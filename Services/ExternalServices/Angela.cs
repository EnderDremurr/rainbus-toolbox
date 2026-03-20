using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using DeepSeek.Core;
using DeepSeek.Core.Models;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Services;

public class Angela(PersistentDataManager dataManager)
{
    public async Task<string?> ProcessText(string text)
    {
        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        var token = dataManager.Settings.DeepSeekToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _ = PopUpWindow.ShowAsync(parent!, "Ошибка", "Нерабочий токен");
            return text;
        }

        var client = new DeepSeekClient(token);
        var cachedPrompt = dataManager.Settings.AngelaPrompt;


        if (string.IsNullOrWhiteSpace(cachedPrompt))
        {
            _ = PopUpWindow.ShowAsync(parent!, "Ошибка", "Не найден промпт в настройках");
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

        Console.WriteLine("Requesting chat...");

        try
        {
            var response = await client.ChatAsync(request, CancellationToken.None);
            var responseText = response?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                _ = PopUpWindow.ShowAsync(parent!, "Ошибка", "Не удалось подключится к API");
                return text;
            }

            Console.WriteLine(responseText);
            return responseText;
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex, "С дипсиком какая-то хуйня");
            return text;
        }
    }
}