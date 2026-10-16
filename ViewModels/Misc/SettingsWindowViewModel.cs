using System.Diagnostics;
using System.IO;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Services;
using RainbusToolbox.Services.ExternalServices;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Utilities.NetworkUtilities;

namespace RainbusToolbox.ViewModels.Misc;

public partial class SettingsWindowViewModel : ObservableObject
{
    private readonly ClipboardService _clipboardService;
    private readonly PersistentDataManager _dataManager;
    private readonly DialogService _dialogService;
    private readonly GithubManager _githubManager;
    private readonly LocalizationManager _localizationManager;

    public SettingsWindowViewModel(PersistentDataManager manager, GithubManager githubManager,
        LocalizationManager localizationManager, DialogService dialogService, ClipboardService clipboardService)
    {
        _dataManager = manager;
        _githubManager = githubManager;
        _localizationManager = localizationManager;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        LoadSettings();
    }


    [RelayCommand]
    private async Task BrowseRepoFolder()
    {
        var folder = await _dialogService.PickSingleFolderAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с репозиторием!!!"
        });

        if (folder != null)
            RepoPath = folder;
    }

    [RelayCommand]
    private async Task BrowseLimbusFolder()
    {
        var folder = await _dialogService.PickSingleFolderAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с бимбус кемпани МЕНЕДЖЕР ЭСКВАЕР!!!"
        });

        if (folder != null)
            LimbusPath = folder;
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

        _dataManager.Settings.GitHubToken = newToken;
        _dataManager.SaveToFile();

        GitHubAuthState = "Ты залогинен";
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var isDirty = false;

        var settingsCache = _dataManager.Settings;
        if (settingsCache.DiscordWebHook != DiscordWebhook)
        {
            var discordManager = new DiscordManager(settingsCache.DiscordWebHook!, _dialogService);
            if (discordManager.ValidateWebhook(DiscordWebhook))
            {
                _dataManager.Settings.DiscordWebHook = DiscordWebhook;
                isDirty = true;
            }
        }

        if (settingsCache.RepositoryPath != RepoPath)
        {
            _dataManager.Settings.RepositoryPath = RepoPath;
            isDirty = true;
        }

        if (settingsCache.PathToLimbus != LimbusPath)
        {
            _dataManager.Settings.PathToLimbus = LimbusPath;
            isDirty = true;
        }

        if (AngelaToken != settingsCache.DeepSeekToken)
        {
            _dataManager.Settings.DeepSeekToken = AngelaToken;
            isDirty = true;
        }

        if (AngelaPrompt != settingsCache.AngelaPrompt)
        {
            _dataManager.Settings.AngelaPrompt = AngelaPrompt;
            isDirty = true;
        }

        if (DiscordRoleToPing != settingsCache.DiscordRoleToPing)
        {
            _dataManager.Settings.DiscordRoleToPing = DiscordRoleToPing;
            isDirty = true;
        }

        if (isDirty)
            _dataManager.SaveToFile();
    }


    private void LoadSettings()
    {
        try
        {
            var data = _dataManager.Settings;
            DiscordWebhook = data.DiscordWebHook ?? "";
            RepoPath = data.RepositoryPath ?? "";
            LimbusPath = data.PathToLimbus ?? "";
            AngelaPrompt = data.AngelaPrompt ?? "";
            AngelaToken = data.DeepSeekToken ?? "";
            DiscordRoleToPing = data.DiscordRoleToPing ?? "";

            GitHubAuthState = string.IsNullOrWhiteSpace(data.GitHubToken)
                ? "Ты не залогинен"
                : "Ты залогинен";
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(ex);
        }
    }

    #region Variables

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WebhookToggleText))]
    private bool _isWebhookVisible;

    public string WebhookToggleText => IsWebhookVisible ? "Скрыть" : "Показать";

    [ObservableProperty]
    public string _discordWebhook = "";

    [ObservableProperty]
    public string _repoPath = "";

    [ObservableProperty]
    public string _limbusPath = "";

    [ObservableProperty]
    public string _angelaPrompt = "";

    [ObservableProperty]
    public string _angelaToken = "";

    [ObservableProperty]
    public string _discordRoleToPing = "";

    [ObservableProperty]
    public string _gitHubAuthState = "";

    #endregion
}