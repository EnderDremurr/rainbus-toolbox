using System.IO;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Utilities;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Constructor

    public MainWindowViewModel(
        PersistentDataManager dataManager,
        GithubManager githubManager,
        GitManager gitManager,
        LocalizationManager localizationManager,
        IServiceProvider serviceProvider,
        TranslationTabViewModel translationTabViewModel,
        FilesTabViewModel filesTabViewModel,
        OverviewTabViewModel overviewTabViewModel,
        ReleaseTabViewModel releaseTabViewModel,
        DiscordRPCService discordRPCService)
    {
        _dataManager = dataManager;
        _githubManager = githubManager;
        _gitManager = gitManager;
        _localizationManager = localizationManager;
        _serviceProvider = serviceProvider;
        _discordRPCService = discordRPCService;

        TranslationTabViewModel = translationTabViewModel;
        FilesTabViewModel = filesTabViewModel;
        OverviewTabViewModel = overviewTabViewModel;
        ReleaseTabViewModel = releaseTabViewModel;


        // Initial parse
        _ = ReparseUserDataAsync();

        // Start periodic timer (every 1 minute)
        _reparseTimer = new Timer(
            async _ => await Dispatcher.UIThread.InvokeAsync(ReparseUserDataAsync),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        // Subscribe to window focus if running in desktop lifetime
        if (Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        // Ensure MainWindow exists
        if (desktop.MainWindow == null)
            return;

        desktop.MainWindow.Activated += (_, _) =>
            Dispatcher.UIThread.InvokeAsync(ReparseUserDataAsync);
    }

    #endregion

    #region Methods

    public async Task ReparseUserDataAsync()
    {
        // TODO: THE FUCK IS THIS SHIT!!!!! NEED SLOP REVIEW
        Username = await _githubManager.GetGithubDisplayNameAsync();

        try
        {
            if (!await _githubManager.IsConnectionValid())
            {
                RepoName = "Offline";
                GitStatus = "Offline";
                return;
            }

            var (repoName, repoChanges) = await Task.Run(() =>
            {
                var remoteUrl = _gitManager.Repository.Network.Remotes["origin"].Url;
                var name = Path.GetFileNameWithoutExtension(remoteUrl);
                var changes = _gitManager.CheckRepositoryChanges();
                return (name, changes);
            });

            RepoName = repoName;
            GitStatus = repoChanges is { Behind: 0, Ahead: 0 } ? "✓" : $"{repoChanges.Behind}↓ - {repoChanges.Ahead}↑";
            _discordRPCService.ProjectName = repoName;
            _discordRPCService.ProjectUrl = _gitManager.Repository.Network.Remotes["origin"].Url;
            _discordRPCService.SetState(null);
        }
        catch (Exception e)
        {
            RepoName = "Unknown";
            GitStatus = "Unknown";
        }
    }

    #endregion

    #region Fields

    // ReSharper disable once NotAccessedField.Local
    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly GitManager _gitManager;
    private readonly LocalizationManager _localizationManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordRPCService _discordRPCService;

    // viewmodels for tabs

    public TranslationTabViewModel TranslationTabViewModel { get; }
    public FilesTabViewModel FilesTabViewModel { get; }
    public OverviewTabViewModel OverviewTabViewModel { get; }
    public ReleaseTabViewModel ReleaseTabViewModel { get; }

    // ReSharper disable once NotAccessedField.Local
    private Timer? _reparseTimer;

    #endregion

    #region Properties

    #region Translation info

    [ObservableProperty]
    private string _username = "Loading";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepoDisplay))]
    private string _repoName = "Loading";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepoDisplay))]
    private string _gitStatus = "Loading";

    public string RepoDisplay => $"<{RepoName} - {GitStatus}> ";

    #endregion

    #endregion

    #region Commands

    [RelayCommand]
    private async Task OpenSettings(Window ownerWindow)
    {
        var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
        await settingsWindow.ShowDialog(ownerWindow);
    }

    [RelayCommand]
    private void Minimize(Window ownerWindow)
    {
        ownerWindow.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void Maximize(Window ownerWindow)
    {
        ownerWindow.WindowState = ownerWindow.WindowState == WindowState.FullScreen
            ? WindowState.Normal
            : WindowState.FullScreen;
    }

    [RelayCommand]
    private void Close(Window ownerWindow)
    {
        ownerWindow.Close();
    }

    [RelayCommand]
    public async Task Synchronize()
    {
        if (!await _githubManager.IsConnectionValid())
        {
            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Ты в оффлайн режиме!",
                "Не удаётся подключится к гитхабу, поэтому увы!");
            return;
        }

        try
        {
            LoadingScreenViewModel.StartLoading("Синхронизация...");
            await _gitManager.SynchronizeWithOriginAsync();
            await ReparseUserDataAsync();
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(
                new Exception($"Synchronization failed: {ex.Message}", ex)
            );
        }
        finally
        {
            LoadingScreenViewModel.FinishLoading();
        }
    }

    [RelayCommand]
    private async Task Commit(Window ownerWindow)
    {
        var vm = await PopUpWindow.ShowAsync(
            ownerWindow,
            "Создание коммита",
            "Гит обязательно требует хотя бы 1 символ как описание коммита",
            true,
            "Описание", null,
            new PopupButton { Label = "Отмена", ResultValue = "cancel" },
            new PopupButton { Label = "Создать коммит", ResultValue = "ok" }
        );

        if (vm.Result == "ok" && !string.IsNullOrWhiteSpace(vm.InputValue))
        {
            _gitManager.CommitLocalChanges(vm.InputValue);
            await ReparseUserDataAsync();
        }
    }

    [RelayCommand] private void History()
    {
    }

    #endregion
}