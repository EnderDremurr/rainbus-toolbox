using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;

namespace RainbusToolbox.ViewModels;

public partial class ReleaseTabViewModel : ObservableObject
{
    #region Constructor

    public ReleaseTabViewModel(
        PersistentDataManager dataManager,
        GithubManager githubManager,
        RepositoryManager repositoryManager,
        KeywordProcessingService keywordProcessingService)
    {
        _dataManager = dataManager;
        Option2 = !string.IsNullOrWhiteSpace(_dataManager.Settings.DiscordRoleToPing);
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;
        _keywordProcessingService = keywordProcessingService;

        VersionDisplay = _repositoryManager.GetLatestReleaseSemantic();
    }

    #endregion

    #region Events

    public void OnTabOpened()
    {
        var rpc = App.Current.ServiceProvider.GetService(typeof(DiscordRPCService)) as DiscordRPCService;

        rpc!.SetState("Делает жесткий релиз");
    }

    #endregion

    #region Fields

    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    private readonly KeywordProcessingService _keywordProcessingService;
    private string _username = AppLang.Unknown;
    private string _repoName = AppLang.Unknown;

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
    private bool _mustAppendLauncherLink = true;

    [ObservableProperty]
    private bool _mergeWithReadme = true;

    // Discord section checkboxes
    [ObservableProperty]
    private bool _sendToDiscord = true;

    [ObservableProperty]
    private bool _option1;

    [ObservableProperty]
    private bool _attachAnImage = true;

    [ObservableProperty]
    private Bitmap? _selectedImagePreview;

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

    #endregion


    #region Commands

    [RelayCommand]
    public async Task SelectFile()
    {
        var top =
            Application.Current!.ApplicationLifetime is
                IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        if (top == null) return;

        var storage = top.StorageProvider;


        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выбери картиночку пупсик",
            AllowMultiple = false,
            FileTypeFilter = [FilePickerFileTypes.ImageAll]
        });

        if (files.Count == 0) return;

        var file = files[0].Path.LocalPath;

        _selectedFilePath = file;
        SelectedFileName = Path.GetFileName(_selectedFilePath);
        SelectedImagePreview = new Bitmap(_selectedFilePath);
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
            LoadingScreenViewModel.StartLoading("Создаётся релиз...");
            await _keywordProcessingService.ReplaceEveryTagWithMesh(_repositoryManager.PathToLocalization);

            var currentVersion = _repositoryManager.GetLatestReleaseSemantic();
            var parts = currentVersion.Split('.');
            var major = int.Parse(parts[0]);
            var minor = int.Parse(parts[1]);
            var patch = int.Parse(parts[2]);

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

            LoadingScreenViewModel.SetText("Упаковывается перевод...");
            // Package the localization
            var package =
                await Task.Run(() => LocalizationPackager.PackageLocalization(nextVersion, _repositoryManager));

            LoadingScreenViewModel.SetText("Выкладывается на гитхаб...");
            // Create GitHub release
            var localizationName = _repositoryManager.GetRepoDisplayName(_repositoryManager.Repository);

            await _githubManager.CreateReleaseAsync($"{localizationName} v{nextVersion}", EditorText, package);

            // Handle Discord section options - only send if SendToDiscord is checked
            if (SendToDiscord && DiscordManager.ValidateWebhook(_dataManager.Settings.DiscordWebHook))
            {
                LoadingScreenViewModel.SetText("Отправляется сообщение в дискорд...");
                var discordManager = new DiscordManager(_dataManager.Settings.DiscordWebHook!);

                var discordMessage = $"# {localizationName} v{nextVersion}!!!\n" + EditorText;

                if (MustAppendLauncherLink)
                    discordMessage +=
                        $"\n\n[{AppLang.LocalizationManagerHyperlink}](<https://github.com/kimght/LimbusLocalizationManager/releases>)";
                //if (Option1) TODO:implement later
                //discordMessage += $"\n\n[Ссылка на релиз](<https://github.com/enqenqenqenqenq/RCR/releases/latest>)";
                if (Option2 && !string.IsNullOrWhiteSpace(_dataManager.Settings.DiscordRoleToPing))
                    discordMessage += $"\n<@&{_dataManager.Settings.DiscordRoleToPing}>";

                await discordManager.SendMessageAsync(discordMessage, _selectedFilePath);
            }

            LoadingScreenViewModel.SetText("Готово!");
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
            LoadingScreenViewModel.FinishLoading();
        }
    }

    #endregion
}