using System.IO;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.Views;

namespace RainbusToolbox.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
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
    [NotifyPropertyChangedFor(nameof(UserRepoDisplay))]
    private string _username = "Unknown";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UserRepoDisplay))]
    private string _repoName = "Unknown";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UserRepoDisplay))]
    private string _gitStatus = "Unknown";

    public string UserRepoDisplay => $"{Username} : [{RepoName} {GitStatus}] ";

    #endregion
    
    #region Checkboxes

    // General section checkboxes
    [ObservableProperty] private bool _appendLauncherLink = true; // Default checked
    [ObservableProperty] private bool _mergeWithReadme = true;    // Default checked
    
    // Discord section checkboxes
    [ObservableProperty] private bool _sendToDiscord;
    [ObservableProperty] private bool _option1;
    [ObservableProperty] private bool _option2;
    [ObservableProperty] private string _roleToPing = string.Empty;

    #endregion
    
    [ObservableProperty] private string _editorText = string.Empty;
    
    #endregion

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
        if (Avalonia.Application.Current!.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
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
        GitStatus = (repoChanges[0] == 0 && repoChanges[1] == 0) ? "✓" : $" {repoChanges[0]}↓ {repoChanges[1]}↑";
        _discordRPCService.ProjectName = repoName;
        _discordRPCService.ProjectUrl = _repositoryManager.Repository.Network.Remotes["origin"].Url;
        _discordRPCService.SetState(null);
    }
    #endregion

    #region Commands
    [RelayCommand]
    private async Task OpenSettings(Window ownerWindow)
    {
        var settingsWindow = _serviceProvider.GetRequiredService<SettingsWindow>();
        await settingsWindow.ShowDialog(ownerWindow);
    }

    [RelayCommand]
    private void Minimize(Window ownerWindow) => ownerWindow.WindowState = WindowState.Minimized;

    [RelayCommand]
    private void Maximize(Window ownerWindow) =>
        ownerWindow.WindowState = ownerWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

    [RelayCommand]
    private void Close(Window ownerWindow) => ownerWindow.Close();

    [RelayCommand] private void Synchronize()
    {
        _repositoryManager.SynchronizeWithOrigin();
        _ = ReparseUserDataAsync();
    }

    [RelayCommand]
    private async Task Commit(Window ownerWindow)
    {
        var inputDialog = new InputDialog();
        await inputDialog.ShowDialog(ownerWindow);

        if (!string.IsNullOrWhiteSpace(inputDialog.CommitMessage))
        {
            _repositoryManager.CommitLocalChanges(inputDialog.CommitMessage);
            await ReparseUserDataAsync();
        }
    }

    [RelayCommand] private void History() { }
    #endregion
}