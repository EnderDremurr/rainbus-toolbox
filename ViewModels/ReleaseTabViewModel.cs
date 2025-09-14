using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;

namespace RainbusToolbox.ViewModels;

public partial class ReleaseTabViewModel : ObservableObject
{
    #region Fields
    private readonly PersistentDataManager _dataManager;
    private readonly DiscordManager _discordManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    
    private string _username = "Unknown";
    private string _repoName = "Unknown";
    #endregion

    #region Constructor
    public ReleaseTabViewModel(
        PersistentDataManager dataManager, 
        DiscordManager discordManager, 
        GithubManager githubManager, 
        RepositoryManager repositoryManager)
    {
        _dataManager = dataManager;
        _discordManager = discordManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;

        // Set default values for checkboxes
        MustAppendLauncherLink = true;
        MergeWithReadme = true;
        SendToDiscord = false;
        Option1 = false;
        Option2 = false;
        
        // Explicitly ensure loading is false on startup
        IsLoading = false;

    }
    #endregion

    #region Properties
    // User and repo information
    public string Username
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                OnPropertyChanged(nameof(UserRepoDisplay));
            }
        }
    }

    public string RepoName
    {
        get => _repoName;
        set
        {
            if (_repoName != value)
            {
                _repoName = value;
                OnPropertyChanged(nameof(UserRepoDisplay));
            }
        }
    }

    public string UserRepoDisplay => $"{Username} : [{RepoName}]";

    // Text editor
    [ObservableProperty]
    private string _editorText = string.Empty;

    // General section checkboxes
    [ObservableProperty]
    private bool _mustAppendLauncherLink;

    [ObservableProperty]
    private bool _mergeWithReadme;

    // Discord section checkboxes
    [ObservableProperty]
    private bool _sendToDiscord;

    [ObservableProperty]
    private bool _option1;

    [ObservableProperty]
    private bool _option2;

    [ObservableProperty]
    private string _roleToPing = string.Empty;

    // Version and loading
    [ObservableProperty]
    private string _version = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    // Debug tracking for loading state
    partial void OnIsLoadingChanged(bool value)
    {
        System.Diagnostics.Debug.WriteLine($"ReleaseTabViewModel: IsLoading changed to: {value} at {DateTime.Now}");
        if (value)
        {
            System.Diagnostics.Debug.WriteLine($"Stack trace: {Environment.StackTrace}");
        }
    }

    #endregion

    private Window GetMainWindow()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }
        return null;
    }


    #region Commands

    [RelayCommand]
    private async Task Submit()
    {
        if (string.IsNullOrWhiteSpace(Version))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                "Необходимо указать версию релиза", ButtonEnum.Ok);
            await messageBox.ShowAsync();
            return;
        }

        if (string.IsNullOrEmpty(_dataManager.Settings.GitHubToken))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                "Сначала нужно залогинится в GitHub", ButtonEnum.Ok);
            await messageBox.ShowAsync();
            return;
        }

        try
        {
            IsLoading = true;

            // Handle General section options
            if (MergeWithReadme)
            {
                // TODO: Implement README.md merging logic
                System.Diagnostics.Debug.WriteLine("TODO: Implement README.md merging");
            }

            // Package the localization
            var package = await Task.Run(() => LocalizationPackager.PackageLocalization(Version, _repositoryManager));

            // Create GitHub release
            await _githubManager.CreateReleaseAsync($"RCR v{Version}", EditorText, package);

            // Handle Discord section options - only send if SendToDiscord is checked
            if (SendToDiscord)
            {
                var discordMessage = $"# v{Version}\n\n" + EditorText;
                
                if (MustAppendLauncherLink)
                    discordMessage += "\n\n[Лаунчер для переводов](<https://github.com/kimght/LimbusLocalizationManager/releases>)";
                if (Option1)
                    discordMessage += "\n\n[Ссылка на релиз](<https://github.com/enqenqenqenqenq/RCR/releases/latest>)";
                if (Option2 && !string.IsNullOrWhiteSpace(RoleToPing))
                    discordMessage += $"\n\n\n\n<@&{RoleToPing}>";
                
                await _discordManager.SendMessageAsync(discordMessage);
            }

            // Success message
            var successBox = MessageBoxManager.GetMessageBoxStandard("Успех",
                $"Релиз v{Version} успешно создан!", ButtonEnum.Ok);
            await successBox.ShowAsync();
        }
        catch (Exception ex)
        {
            App.Current.HandleGlobalException(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    #endregion
}