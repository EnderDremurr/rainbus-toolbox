using System.IO;
using Avalonia.Controls.ApplicationLifetimes;
using Discord.Webhook;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Models.Managers;

public class DiscordManager
{
    public DiscordManager(string webhookUrl)
    {
        if (!ValidateWebhook(webhookUrl))
            return;

        Client = new DiscordWebhookClient(webhookUrl);
    }

    public DiscordWebhookClient? Client { get; }


    public async Task SendMessageAsync(string message, string? imagePath = null)
    {
        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        if (Client == null)
        {
            _ = PopUpWindow.ShowAsync(parent!, "Ошибка",
                "Ошибка при отправке сообщения, чето поломалась. Проверь вебхук в настройках");
            return;
        }

        if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
            await Client.SendFileAsync(imagePath, message);
        else
            await Client.SendMessageAsync(message);
    }


    public static bool ValidateWebhook(string? webhookUrl)
    {
        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _ = PopUpWindow.ShowAsync(parent!, "Ошибка", "Не указан вебхук");
            return false;
        }

        try
        {
            var cl = new DiscordWebhookClient(webhookUrl);
        }
        catch
        {
            _ = PopUpWindow.ShowAsync(parent!, "Ошибка", "Вебхук что ты вписал хуйня ебаная, поставь другой");
            return false;
        }

        return true;
    }
}