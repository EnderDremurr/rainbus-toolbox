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
using RainbusToolbox.Services;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Constructor

    public MainWindowViewModel(
        PersistentDataManager dataManager,
        GithubManager githubManager,
        RepositoryManager repositoryManager,
        IServiceProvider serviceProvider,
        ReleaseTabViewModel releaseTabViewModel,
        DiscordRPCService discordRPCService)
    {
        _dataManager = dataManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;
        _serviceProvider = serviceProvider;
        _discordRPCService = discordRPCService;

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
        Username = await _githubManager.GetGithubDisplayNameAsync();

        var (repoName, repoChanges) = await Task.Run(() =>
        {
            var remoteUrl = _repositoryManager.Repository.Network.Remotes["origin"].Url;
            var name = Path.GetFileNameWithoutExtension(remoteUrl);
            var changes = _repositoryManager.CheckRepositoryChanges();
            return (name, changes);
        });

        RepoName = repoName;
        GitStatus = repoChanges[0] == 0 && repoChanges[1] == 0 ? "✓" : $"{repoChanges[0]}↓ - {repoChanges[1]}↑";
        _discordRPCService.ProjectName = repoName;
        _discordRPCService.ProjectUrl = _repositoryManager.Repository.Network.Remotes["origin"].Url;
        _discordRPCService.SetState(null);
    }

    #endregion

    #region Fields

    // ReSharper disable once NotAccessedField.Local
    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly DiscordRPCService _discordRPCService;

    public ReleaseTabViewModel ReleaseTabViewModel { get; }

    // ReSharper disable once NotAccessedField.Local
    private Timer? _reparseTimer;

    #endregion

    #region Properties

    #region Translation info

    [ObservableProperty]
    private string _username = "Unknown";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepoDisplay))]
    private string _repoName = "Unknown";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RepoDisplay))]
    private string _gitStatus = "Unknown";

    public string RepoDisplay => $"<{RepoName} - {GitStatus}> ";

    #endregion

    #region Checkboxes

    // General section checkboxes
    [ObservableProperty] private bool _appendLauncherLink = true; // Default checked
    [ObservableProperty] private bool _mergeWithReadme = true; // Default checked

    // Discord section checkboxes
    [ObservableProperty] private bool _sendToDiscord;
    [ObservableProperty] private bool _option1;
    [ObservableProperty] private bool _option2;
    [ObservableProperty] private string _roleToPing = string.Empty;

    #endregion

    [ObservableProperty] private string _editorText = string.Empty;

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

    [RelayCommand] private void Synchronize()
    {
        _repositoryManager.SynchronizeWithOrigin();
        _ = ReparseUserDataAsync();
    }

    [RelayCommand]
    private async Task Commit(Window ownerWindow)
    {
        var vm = await PopUpWindow.ShowAsync(
            ownerWindow,
            "Создание коммита",
            "Гит обязательно требует хотя бы 1 символ как описание коммита",
            true,
            "Описание",
            new PopupButton { Label = "Отмена", ResultValue = "cancel" },
            new PopupButton { Label = "Создать коммит", ResultValue = "ok" }
        );

        if (vm.Result == "ok" && !string.IsNullOrWhiteSpace(vm.InputValue))
        {
            _repositoryManager.CommitLocalChanges(vm.InputValue);
            await ReparseUserDataAsync();
        }
    }

    [RelayCommand] private void History()
    {
    }

    #endregion
}