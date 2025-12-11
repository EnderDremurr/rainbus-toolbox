using System.IO;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.Views;

namespace RainbusToolbox.ViewModels;

public partial class ReleaseTabViewModel : ObservableObject
{
    #region Fields
    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    private readonly KeyWordConversionService _keyWordConversionService;
    private string _username = AppLang.Unknown;
    private string _repoName = AppLang.Unknown;
    #endregion

    #region Constructor
    public ReleaseTabViewModel(
        PersistentDataManager dataManager, 
        GithubManager githubManager, 
        RepositoryManager repositoryManager,
        KeyWordConversionService keyWordConversionService)
    {
        _dataManager = dataManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;
        _keyWordConversionService = keyWordConversionService;

        MustAppendLauncherLink = true;
        MergeWithReadme = true;
        SendToDiscord = false;
        Option1 = false;
        Option2 = false;
        
        IsLoading = false;
        VersionDisplay = _repositoryManager.GetLatestReleaseSemantic();

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

    [ObservableProperty]
    private string _selectedFileName = "файл не выбран";
    private string _selectedFilePath = string.Empty;

    // General section checkboxes
    [ObservableProperty]
    private bool _mustAppendLauncherLink;

    [ObservableProperty]
    private bool _mergeWithReadme;

    // Discord section checkboxes
    [ObservableProperty]
    private bool _sendToDiscord = true;

    [ObservableProperty]
    private bool _option1;

    [ObservableProperty]
    private bool _attachAnImage;
    
    [ObservableProperty]
    private bool _globalVersion;
    [ObservableProperty]
    private bool _majorVersion;
    [ObservableProperty]
    private bool _minorVersion = true;
    
    [ObservableProperty]
    private string _versionDisplay;
    

    [ObservableProperty]
    private bool _option2;

    [ObservableProperty]
    private string _roleToPing = string.Empty;


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
    


    #region Commands

    [RelayCommand]
    public async Task SelectFile()
    {
        var dialog = new OpenFileDialog();
        var result = await dialog.ShowAsync((App.Current.ServiceProvider.GetService(typeof(MainWindow)) as MainWindow)!);
        if (result != null) _selectedFilePath = result[0];
        SelectedFileName = Path.GetFileName(_selectedFilePath);
    }

    [RelayCommand]
    public async Task Submit()
    {
        if (string.IsNullOrEmpty(_dataManager.Settings.GitHubToken))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard(AppLang.ErrorTitle,
                AppLang.NoGithubLogin);
            await messageBox.ShowAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var currentVersion = _repositoryManager.GetLatestReleaseSemantic();
            var parts = currentVersion.Split('.');
            int major = int.Parse(parts[0]);
            int minor = int.Parse(parts[1]);
            int patch = int.Parse(parts[2]);

            if (GlobalVersion)
            {
                major++;
                minor = 0;
                patch = 0;
            }
            else if (MajorVersion)
            {
                minor++;
                patch = 0;
            }
            else if (MinorVersion)
            {
                patch++;
            }

            var nextVersion = $"{major}.{minor}.{patch}";

            // Package the localization
            var package = await Task.Run(() => LocalizationPackager.PackageLocalization(nextVersion, _repositoryManager));

            
            // Create GitHub release

            var localizationName = _repositoryManager.GetRepoDisplayName(_repositoryManager.Repository);
            
            await _githubManager.CreateReleaseAsync($"{localizationName} v{nextVersion}", EditorText, package);

            // Handle Discord section options - only send if SendToDiscord is checked
            if (SendToDiscord && DiscordManager.ValidateWebhook(_dataManager.Settings.DiscordWebHook))
            {
                var discordManager = new DiscordManager(_dataManager.Settings.DiscordWebHook!);
                
                var discordMessage = $"# {localizationName} v{nextVersion}!!!\n" + EditorText;
                
                if (MustAppendLauncherLink)
                    discordMessage += $"\n\n[{AppLang.LocalizationManagerHyperlink}](<https://github.com/kimght/LimbusLocalizationManager/releases>)";
                //if (Option1) TODO:implement later
                    //discordMessage += $"\n\n[Ссылка на релиз](<https://github.com/enqenqenqenqenq/RCR/releases/latest>)";
                if (Option2 && !string.IsNullOrWhiteSpace(RoleToPing))
                    discordMessage += $"\n<@&{RoleToPing}>";
                
                await discordManager.SendMessageAsync(discordMessage, _selectedFilePath);
            }

            // Success message
            var successBox = MessageBoxManager.GetMessageBoxStandard(AppLang.SuccessTitle,
                string.Format(AppLang.ReleaseCreationSuccess, nextVersion));
            await successBox.ShowAsync();
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(ex);
        }
        finally
        {
            VersionDisplay = _repositoryManager.GetLatestReleaseSemantic();
            IsLoading = false;
        }
    }
    
    #endregion
}