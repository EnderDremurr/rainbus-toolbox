using System.Diagnostics;
using System.IO;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models;
using RainbusToolbox.Services;
using RainbusToolbox.Services.ExternalServices;
using RainbusToolbox.Utilities.NetworkUtilities;

namespace RainbusToolbox.ViewModels.Misc;

public partial class SettingsWindowViewModel : ObservableObject
{
    private static readonly Random _random = new();
    private readonly ClipboardService _clipboardService;
    private readonly PersistentDataManager _dataManager;
    private readonly DialogService _dialogService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GithubAuthState))]
    private SettingsData _localSettingsCopy;

    public SettingsWindowViewModel(PersistentDataManager dataManager, DialogService dialogService,
        ClipboardService clipboardService)
    {
        _dataManager = dataManager;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        _localSettingsCopy = _dataManager.Settings.Copy();
    }


    [RelayCommand]
    private async Task BrowseRepoFolder()
    {
        var folder = await _dialogService.PickSingleFolderAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с репозиторием!!!"
        });

        if (folder != null)
            LocalSettingsCopy = LocalSettingsCopy with { RepositoryPath = folder };
    }

    [RelayCommand]
    private async Task BrowseLimbusFolder()
    {
        var folder = await _dialogService.PickSingleFolderAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с бимбус кемпани МЕНЕДЖЕР ЭСКВАЕР!!!"
        });

        if (folder != null)
            LocalSettingsCopy = LocalSettingsCopy with { PathToLimbus = folder };
    }

    [RelayCommand]
    private void OpenFolderWithLogs()
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RainbusToolbox", "logs");

        Directory.CreateDirectory(path);

        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", path);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", path);
        else if (OperatingSystem.IsLinux())
            Process.Start("xdg-open", path);
    }

    [RelayCommand]
    private async Task AskSetGitHubToken()
    {
        var newToken = await GithubAuthHelper.RequestGithubAuthAsync(async userCode =>
        {
            await _dialogService.ShowPopupAsync("Нужна авторизация",
                $"Проге нужен токен с GitHub.\nВведи этот код на открытой странице:\n\n{userCode}\n\nЗатем нажми ОК",
                false,
                "",
                null,
                new PopupButton
                {
                    Label = "Скопировать код",
                    ResultValue = "copy",
                    KeepOpen = true,
                    OnClick = () => _ = _clipboardService.SetClipboardTextAsync(userCode)
                },
                new PopupButton { Label = "OK", ResultValue = "ok" });
        });

        if (!await GithubManager.IsTokenValidAsync(newToken))
        {
            await App.Current.HandleNonFatalExceptionAsync(new Exception("Гитхаб вернул невалидный токен."));
            return;
        }

        _dataManager.Settings = _dataManager.Settings with { GitHubToken = newToken };
        _dataManager.SaveToFile();
        LocalSettingsCopy = LocalSettingsCopy with { GitHubToken = newToken };
    }

    [RelayCommand]
    private void SaveSettings()
    {
        if (LocalSettingsCopy.DiscordWebHook != _dataManager.Settings.DiscordWebHook)
        {
            var manager = new DiscordManager(_dataManager.Settings.DiscordWebHook!, _dialogService);
            if (!manager.ValidateWebhook(LocalSettingsCopy.DiscordWebHook))
                LocalSettingsCopy = LocalSettingsCopy with { DiscordWebHook = _dataManager.Settings.DiscordWebHook };
        }

        if (LocalSettingsCopy != _dataManager.Settings)
        {
            _dataManager.Settings = LocalSettingsCopy.Copy();
            _dataManager.SaveToFile();
            Log.Debug("User had changed one of the settings before closing the settings window. Saving new settings");
            return;
        }

        Log.Debug("User didn't change any settings before closing the settings window");
    }

    [RelayCommand]
    public void RandomizeRPCAssetID()
    {
        var randomId = _random.Next(1, DiscordRPCService.MaxAssets + 1);
        LocalSettingsCopy = LocalSettingsCopy with { SelectedDiscordRPCAssetId = randomId };
    }


    #region Variables

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WebhookToggleText))]
    private bool _isWebhookVisible;

    public string WebhookToggleText => IsWebhookVisible ? "Скрыть" : "Показать";

    public string GithubAuthState => string.IsNullOrWhiteSpace(LocalSettingsCopy.GitHubToken)
        ? "Ты не залогинен"
        : "Аккаунт найден";

    #endregion
}