using System.IO;
using Discord.Webhook;

namespace RainbusToolbox.Services.ExternalServices;

public sealed class DiscordManager
{
    private readonly DialogService _dialogService;

    public DiscordManager(string webhookUrl, DialogService dialogService)
    {
        if (!ValidateWebhook(webhookUrl))
            return;

        _dialogService = dialogService;
        Client = new DiscordWebhookClient(webhookUrl);
    }

    public DiscordWebhookClient? Client { get; }


    public async Task SendMessageAsync(string message, string? imagePath = null)
    {
        if (Client == null)
        {
            _ = _dialogService.ShowPopupAsync("Ошибка",
                "Ошибка при отправке сообщения, чето поломалась. Проверь вебхук в настройках");
            return;
        }

        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            await Client.SendFileAsync(imagePath, message);
        else
            await Client.SendMessageAsync(message);
    }


    public bool ValidateWebhook(string? webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _ = _dialogService.ShowPopupAsync("Ошибка", "Не указан вебхук");
            return false;
        }

        try
        {
            var unused = new DiscordWebhookClient(webhookUrl);
        }
        catch
        {
            _ = _dialogService.ShowPopupAsync("Ошибка", "Вебхук что ты вписал хуйня ебаная, поставь другой");
            return false;
        }

        return true;
    }
}