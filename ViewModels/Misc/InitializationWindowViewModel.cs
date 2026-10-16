using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Services;
using RainbusToolbox.Services.ExternalServices;
using RainbusToolbox.Utilities.NetworkUtilities;

namespace RainbusToolbox.ViewModels.Misc;

public partial class InitializationWindowViewModel : ObservableObject
{
    private readonly ClipboardService _clipboardService;
    private readonly PersistentDataManager _dataManager;
    private readonly DialogService _dialogService;

    [ObservableProperty]
    private string _gitHubTokenStatus = "Ты не залогинен";

    [ObservableProperty]
    private string _limbusPath = @"C:\Path\To\Limbus";

    [ObservableProperty]
    private string _repoPath = @"C:\Path\To\Repo";

    public InitializationWindowViewModel(PersistentDataManager dataManager, DialogService dialogService,
        ClipboardService clipboardService)
    {
        _dataManager = dataManager;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        LoadPathsAndTokenStatus();
    }

    [RelayCommand]
    private async Task SetGitHubTokenCommand()
    {
        try
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
                    new PopupButton { Label = "OK", ResultValue = "ok" }
                );
            });

            if (!await GithubManager.IsTokenValidAsync(newToken))
            {
                await App.Current.HandleNonFatalExceptionAsync(new Exception("Гитхаб вернул невалидный токен."));
                return;
            }

            _dataManager.Settings.GitHubToken = newToken;
            _dataManager.SaveToFile();

            GitHubTokenStatus = "Ты залогинен";
        }
        catch (Exception exception)
        {
            await App.Current.HandleGlobalExceptionAsync(exception);
        }
    }

    [RelayCommand]
    public async Task BrowseFolder()
    {
        var folder = await _dialogService.PickSingleFolderAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с репозиторием",
            AllowMultiple = false
        });
        if (folder == null)
            return;

        var validatedPath = PersistentDataManager.ValidateRepoPath(folder);
        if (validatedPath != null)
        {
            _dataManager.Settings.RepositoryPath = validatedPath;
            RepoPath = validatedPath;
        }
    }

    [RelayCommand]
    public async Task BrowseLimbusFolder()
    {
        var folder = await _dialogService.PickSingleFolderAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с лимбусом",
            AllowMultiple = false
        });
        if (folder == null)
            return;

        var validatedPath = PersistentDataManager.ValidateLimbusPath(folder);
        if (validatedPath != null)
        {
            _dataManager.Settings.PathToLimbus = validatedPath;
            LimbusPath = validatedPath;
        }
    }

    [RelayCommand]
    public void RestartApp()
    {
        Save();

        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        _ = _dialogService.ShowPopupAsync("Сохранено!",
            "Настройки были успешно сохранены, теперь нужно просто перезапустить прогу!");
    }

    private void LoadPathsAndTokenStatus()
    {
        try
        {
            var data = _dataManager.Settings;
            RepoPath = data.RepositoryPath ?? @"C:\Path\To\Repo";
            LimbusPath = data.PathToLimbus ?? @"C:\Path\To\Limbus";

            GitHubTokenStatus = string.IsNullOrWhiteSpace(data.GitHubToken)
                ? "Ты не залогинен"
                : "Ты залогинен";
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex);
        }
    }

    [RelayCommand]
    public void Save()
    {
        _dataManager.SaveToFile();
    }
}