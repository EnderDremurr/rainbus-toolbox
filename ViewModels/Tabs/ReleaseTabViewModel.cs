using System.Collections.Generic;
using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.ViewModels;

public partial class ReleaseTabViewModel : ObservableObject
{
    #region Constructor

    public ReleaseTabViewModel(
        PersistentDataManager dataManager,
        GithubManager githubManager,
        RepositoryManager repositoryManager,
        KeywordProcessingService keywordProcessingService,
        MassReplacementService massReplacementService)
    {
        _dataManager = dataManager;
        PingSetRole = !string.IsNullOrWhiteSpace(_dataManager.Settings.DiscordRoleToPing);
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;
        _keywordProcessingService = keywordProcessingService;
        _massReplacementService = massReplacementService;
    }

    #endregion

    #region Events

    public async void OnTabOpened()
    {
        var rpc = App.Current.ServiceProvider.GetService(typeof(DiscordRPCService)) as DiscordRPCService;

        rpc!.SetState("Делает жесткий релиз");

        if (!await _githubManager.IsConnectionValid())
        {
            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Ты в оффлайн режиме!",
                "Не удаётся подключится к гитхабу, поэтому увы!");
            VersionDisplay = "Оффлайн режим!";
            return;
        }

        VersionDisplay = _repositoryManager.GetLatestReleaseSemantic();
    }

    #endregion

    private async Task RunAllEntriesAsyncBeforeRelease()
    {
        try
        {
            LoadingScreenViewModel.SetText("Замена всех правил...");

            var progress = new Progress<(int Processed, int Total, string Label)>(p =>
            {
                LoadingScreenViewModel.SetProgress(p.Processed, p.Total);
                LoadingScreenViewModel.SetText(p.Label);
            });

            var pathToRegexJson = _repositoryManager.PathToRegexJson;
            if (!File.Exists(pathToRegexJson)) return;

            var entries = JsonConvert.DeserializeObject<List<ReplacementEntry>>(File.ReadAllText(pathToRegexJson));
            if (entries is null) return;

            await _massReplacementService.RunAllRegexesForAllFilesAsync(entries, progress, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex,
                "Ошибка при замене, но релиз всё еще сделается, ничего страшного!");
        }
    }

    #region Fields

    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    private readonly KeywordProcessingService _keywordProcessingService;
    private readonly MassReplacementService _massReplacementService;
    private CancellationTokenSource? _cancellationTokenSource;

    #endregion

    #region Properties

    // Text editor
    [ObservableProperty]
    private string _editorText = string.Empty;

    [ObservableProperty]
    private string _selectedFileName = "файл не выбран";

    private string _selectedFilePath = string.Empty;

    // General section checkboxes
    [ObservableProperty]
    private bool _appendLauncherLink = true;

    [ObservableProperty]
    private bool _mergeWithReadme = true;

    [ObservableProperty]
    private bool _runRegexesBeforeRelease = true;

    // Discord section checkboxes
    [ObservableProperty]
    private bool _sendToDiscord = true;

    [ObservableProperty]
    private bool _option1;

    [ObservableProperty]
    private bool _attachAnImage;

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
    private bool _pingSetRole;

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
        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

        if (!await _githubManager.IsConnectionValid())
        {
            await PopUpWindow.ShowAsync(parent!, "Ты в оффлайн режиме!",
                "Не удаётся подключится к гитхабу, поэтому увы!");
            return;
        }

        if (string.IsNullOrWhiteSpace(_dataManager.Settings.GitHubToken))
        {
            await PopUpWindow.ShowAsync(parent!, "Ошибка!",
                "Для создания релиза необходимо залогиниться в аккаунт гитхаб");
            return;
        }

        if (EditorText.Length > 1800)
        {
            await PopUpWindow.ShowAsync(parent!, "Ошибка!",
                $"Длина описания не должна составлять больше 1800 символов. Сейчас символов - {EditorText.Length}");
            return;
        }

        try
        {
            // run replacements if user enabled the option
            if (RunRegexesBeforeRelease)
            {
                LoadingScreenViewModel.StartLoading("Начинается прогон автозамены...");

                // replacement is run twice before release to ensure everything is replaced properly
                await RunAllEntriesAsyncBeforeRelease();
                await RunAllEntriesAsyncBeforeRelease();
            }

            LoadingScreenViewModel.StartLoading("Начинается замена кейвордов перед релизом...");
            await _keywordProcessingService.ReplaceEveryTagWithMesh(_repositoryManager
                .PathToLocalization); // TODO: don't forget to change this after i move to new keyword conversion pipeline

            var currentVersion =
                _repositoryManager
                    .GetLatestReleaseSemantic(); // TODO: this section should probably be moved to github manager, so this command will only get next verion
            // i guess, like have an enum for version types and then string GetNextSemanticVersion(VersionType type) with major minor patch
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
            // down to here

            // commit and sync release version, dunno what i didn't think of doing that back then

            _repositoryManager.CommitLocalChanges($"Automatic commit of release version {nextVersion}");
            await _repositoryManager
                .SynchronizeWithOriginAsync(); // TODO: don't forget, all the git shit should be moved to another service later cuz my repo manager is fucking huge!!!!!

            // i believe start loading should reset the previous bars? bro i don't even know how my own shit works anymore :sob:

            LoadingScreenViewModel.StartLoading("Упаковывается перевод...");
            // Package the localization
            var package =
                await LocalizationPackager.PackageLocalizationAsync(nextVersion, _repositoryManager);

            LoadingScreenViewModel.StartLoading("Выкладывается на гитхаб...");
            // Create GitHub release
            var localizationName = _repositoryManager.GetRepoDisplayName(_repositoryManager.Repository);

            await _githubManager.CreateReleaseAsync($"{localizationName} v{nextVersion}", EditorText, package);

            // Handle Discord section options - only send if SendToDiscord is checked
            if (SendToDiscord && DiscordManager.ValidateWebhook(_dataManager.Settings.DiscordWebHook))
            {
                LoadingScreenViewModel.StartLoading("Отправляется сообщение в дискорд...");
                var discordManager = new DiscordManager(_dataManager.Settings.DiscordWebHook!);

                var discordMessage = $"# {localizationName} v{nextVersion}!!!\n" + EditorText;

                if (AppendLauncherLink)
                    discordMessage +=
                        $"\n\n[{AppLang.LocalizationManagerHyperlink}](<https://github.com/kimght/LimbusLocalizationManager/releases>)";
                //if (Option1) TODO:implement later LMAO I JUST FORGOT ABOUT TS
                //discordMessage += $"\n\n[Ссылка на релиз](<https://github.com/enqenqenqenqenq/RCR/releases/latest>)";

                if (PingSetRole && !string.IsNullOrWhiteSpace(_dataManager.Settings.DiscordRoleToPing))
                    discordMessage += $"\n<@&{_dataManager.Settings.DiscordRoleToPing}>";

                await discordManager.SendMessageAsync(discordMessage, _selectedFilePath);
            }

            LoadingScreenViewModel.SetText("Готово!");
            // Success message
            await PopUpWindow.ShowAsync(parent!, "Успешно!",
                string.Format(AppLang.ReleaseCreationSuccess, nextVersion));
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