using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Views;

namespace RainbusToolbox.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Fields
    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    private readonly IServiceProvider _serviceProvider;
    
    public ReleaseTabViewModel ReleaseTabViewModel { get; }
    
    private Timer? _reparseTimer;

    private string _username = "Unknown";
    private string _repoName = "Unknown";
    private string _gitStatus = "Unknown";
    #endregion

    #region Properties
    public string GitStatus
    {
        get => _gitStatus;
        set => SetProperty(ref _gitStatus, value, nameof(UserRepoDisplay));
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value, nameof(UserRepoDisplay));
    }

    public string RepoName
    {
        get => _repoName;
        set => SetProperty(ref _repoName, value, nameof(UserRepoDisplay));
    }

    public string UserRepoDisplay => $"{Username} : [{RepoName} {GitStatus}] ";

    [ObservableProperty] private string _editorText = string.Empty;
    
    // General section checkboxes
    [ObservableProperty] private bool _appendLauncherLink = true; // Default checked
    [ObservableProperty] private bool _mergeWithReadme = true;    // Default checked
    
    // Discord section checkboxes
    [ObservableProperty] private bool _sendToDiscord;
    [ObservableProperty] private bool _option1;
    [ObservableProperty] private bool _option2;
    [ObservableProperty] private string _roleToPing = string.Empty;
    
    
    #endregion

    #region Constructor
    public MainWindowViewModel(
        PersistentDataManager dataManager,
        GithubManager githubManager,
        RepositoryManager repositoryManager,
        IServiceProvider serviceProvider,
        ReleaseTabViewModel releaseTabViewModel)
    {
        _dataManager = dataManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;
        _serviceProvider = serviceProvider;
        
        ReleaseTabViewModel = releaseTabViewModel;

        // Initial parse
        ReparseUserDataAsync();

        // Start periodic timer (every 1 minute)
        _reparseTimer = new Timer(
            async _ => await Dispatcher.UIThread.InvokeAsync(ReparseUserDataAsync),
            null,
            TimeSpan.Zero,
            TimeSpan.FromMinutes(1));

        // Subscribe to window focus if running in desktop lifetime
        if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Ensure MainWindow exists
            if (desktop.MainWindow != null)
            {
                desktop.MainWindow.Activated += (_, __) =>
                    Dispatcher.UIThread.InvokeAsync(ReparseUserDataAsync);
            }
        }
    }
    #endregion

    #region Methods
    public async Task ReparseUserDataAsync()
    {
        Username = await _githubManager.GetGithubDisplayNameAsync();
        var remoteUrl = _repositoryManager.Repository.Network.Remotes["origin"].Url;
        RepoName = Path.GetFileNameWithoutExtension(remoteUrl);

        var repoChanges = _repositoryManager.CheckRepositoryChanges();
        GitStatus = (repoChanges[0] == 0 && repoChanges[1] == 0) ? "✓" : $" {repoChanges[0]}↓ {repoChanges[1]}↑";
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
        ReparseUserDataAsync();
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