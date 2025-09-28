using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Discord.Webhook;
using Discord.WebSocket;
using MsBox;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

namespace RainbusToolbox.Models.Managers;

public class DiscordManager
{
    public DiscordWebhookClient? Client { get; private set; }
    public DiscordManager(string webhookUrl)
    {
        if(!ValidateWebhook(webhookUrl))
            return;
        
        Client = new DiscordWebhookClient(webhookUrl);
    }

    
    public async Task SendMessageAsync(string message, string? imagePath = null)
    {
        if (Client == null)
        {
            _ = App.Current.ShowErrorNotificationAsync("Ошибка при отправке сообщения, чето поломалась. Проверь вебхук в настройках");
            return;
        }
        if(imagePath != null)
            await Client.SendFileAsync(imagePath, message);
        else
            await Client.SendMessageAsync(message);
    }



    public static bool ValidateWebhook(string? webhookUrl)
    {

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _ = App.Current.ShowErrorNotificationAsync("Не указан вебхук");
            return false;
        }

        try
        {
            var cl = new DiscordWebhookClient(webhookUrl);
        }
        catch
        {
            _ = App.Current.ShowErrorNotificationAsync("Вебхук что ты вписал хуйня ебаная, поставь другой");
            return false;
        }
        
        return true;
    }
}