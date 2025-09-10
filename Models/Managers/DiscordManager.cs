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
            ShowMessageAsync(window,
                "Внимание спасибо за внимание",
                "Ты не указал вебхук, без него сообщение в дс не сможет отправится",
                ButtonEnum.Ok,
                Icon.Info);
            return;
        }

        try
        {
            Client = new DiscordWebhookClient(webhookUrl);
        }
        catch
        {
            Client = null;  
            ShowMessageAsync(window,
                "АШЫЫЫЫЫЫЫЫЫЫЫЫЫБКААААААААА",
                "Вебхук что ты вписал хуйня ебаная, поставь другой",
                ButtonEnum.Ok,
                Icon.Error);
        }
    }

    /// <summary>
    /// Safely shows a message box, handling cases where the main window is not yet open or already closed.
    /// </summary>
    private async Task ShowMessageAsync(Window window, string title, string message, ButtonEnum buttons, Icon icon)
    {
        // Wait until the window is visible
        if (window != null && !window.IsVisible)
        {
            var tcs = new TaskCompletionSource<bool>();

            void Handler(object sender, EventArgs e)
            {
                window.Opened -= Handler;
                tcs.SetResult(true);
            }

            window.Opened += Handler;
            await tcs.Task;
        }

        var msgBox = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            buttons,
            icon,
            WindowStartupLocation.CenterScreen
        );

        // Show modal attached to window
        if (window != null && window.IsVisible)
        {
            await msgBox.ShowWindowDialogAsync(window);
        }
        else
        {
            // fallback, should rarely hit
            await msgBox.ShowWindowAsync();
        }
    }

    public void SendMessage(string message)
    {
        Client?.SendMessageAsync(message);
    }
    
    public async Task SendMessageAsync(string message)
    {
        await Client.SendMessageAsync(message);
    }
}