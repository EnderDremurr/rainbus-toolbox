using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.NetworkUtilities;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.ViewModels;

public partial class InitializationWindowViewModel : ObservableObject
{
    private readonly PersistentDataManager _dataManager;

    [ObservableProperty]
    private string _gitHubTokenStatus = "Ты не залогинен";

    [ObservableProperty]
    private string _limbusPath = @"C:\Path\To\Limbus";

    [ObservableProperty]
    private string _repoPath = @"C:\Path\To\Repo";

    public InitializationWindowViewModel(PersistentDataManager dataManager)
    {
        _dataManager = dataManager;

        LoadPathsAndTokenStatus();
    }

    // events for view
    public event Func<string, Task<IStorageFolder?>>? RequestFolderPicker;
    public event Action<string>? RequestGithubAuthPopup;

    [RelayCommand]
    private async Task SetGitHubTokenCommand()
    {
        try
        {
            var newToken = await GithubAuthHelper.RequestGithubAuthAsync(userCode =>
            {
                RequestGithubAuthPopup?.Invoke(userCode);
                return Task.CompletedTask;
            });

            if (!await GithubManager.IsTokenValidAsync(newToken))
            {
                await App.Current.HandleNonFatalExceptionAsync(new Exception("Гитхаб вернул невалидный токен."));
                return;
            }

            _dataManager.Settings.GitHubToken = newToken;
            _dataManager.Save();

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
        if (RequestFolderPicker == null) return;
        var pickedFolder = await RequestFolderPicker.Invoke("Выбери папку с репозиторием");
        if (pickedFolder == null) return;
        var result = pickedFolder.Path.ToString();

        var validatedPath = PersistentDataManager.ValidateRepoPath(result);
        if (validatedPath != null)
        {
            _dataManager.Settings.RepositoryPath = validatedPath;
            RepoPath = validatedPath;
        }
    }

    [RelayCommand]
    public async Task BrowseLimbusFolder()
    {
        if (RequestFolderPicker == null) return;
        var pickedFolder = await RequestFolderPicker.Invoke("Выбери папку с лимбусом");
        if (pickedFolder == null) return;
        var result = pickedFolder.Path.ToString();

        var validatedPath = PersistentDataManager.ValidateLimbusPath(result);
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
        _ = PopUpWindow.ShowAsync(parent!, "Сохранено!",
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
        _dataManager.Save();
    }
}