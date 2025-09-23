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
    private PersistentDataManager _dataManager;
    public DiscordWebhookClient? Client { get; private set; }
    public DiscordManager(PersistentDataManager manager)
    {
        _dataManager = manager;
        
        TryInitialize();
    }

    public void TryInitialize(Window window = null)
    {
        var webhookUrl = _dataManager.Settings.DiscordWebHook;

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            Client = null;
            _ = App.Current.ShowErrorNotificationAsync("Не указан вебхук");
            return;
        }

        try
        {
            Client = new DiscordWebhookClient(webhookUrl);
        }
        catch
        {
            Client = null;
            _ = App.Current.ShowErrorNotificationAsync("Вебхук что ты вписал хуйня ебаная, поставь другой");
        }
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
}