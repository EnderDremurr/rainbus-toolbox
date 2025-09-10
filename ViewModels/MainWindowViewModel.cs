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
using RainbusTools.Models.Managers;
using Microsoft.Extensions.DependencyInjection;
using RainbusTools.Views;

namespace RainbusTools.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    #region Fields
    private readonly PersistentDataManager _dataManager;
    private readonly DiscordManager _discordManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;
    private readonly IServiceProvider _serviceProvider;
    
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
    [ObservableProperty] private bool _option1;
    [ObservableProperty] private bool _option2;
    [ObservableProperty] private string _roleToPing = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _version = string.Empty;
    public ObservableCollection<string> Choices { get; } = new() { "RCR папищеки", "Choice B", "Choice C" };
    [ObservableProperty] private ObservableCollection<string> _selectedChoices = new();
    #endregion

    #region Constructor
    public MainWindowViewModel(
        PersistentDataManager dataManager,
        DiscordManager discordManager,
        GithubManager githubManager,
        RepositoryManager repositoryManager,
        IServiceProvider serviceProvider)
    {
        _dataManager = dataManager;
        _discordManager = discordManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;
        _serviceProvider = serviceProvider;

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
    private async Task Submit(Window ownerWindow)
    {
        if (string.IsNullOrEmpty(_dataManager.Settings.GitHubToken))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Error",
                "You must log in to GitHub first", ButtonEnum.Ok);
            await messageBox.ShowAsync();
            return;
        }

        try
        {
            IsLoading = true;

            var package = await Task.Run(() => _repositoryManager.PackageLocalization(Version));
            await _githubManager.CreateReleaseAsync($"RCR v{Version}", EditorText, package);

            var discordMessage = $"# v{Version}\n\n" + EditorText;
            if (Option1)
                discordMessage += "\n\n[Link to Release](https://github.com/enqenqenqenqenq/RCR/releases/latest)";
            if (Option2)
                discordMessage += $"\n\n\n\n<@&{RoleToPing}>";

            await _discordManager.SendMessageAsync(discordMessage);
        }
        catch (Exception ex)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Error", ex.Message, ButtonEnum.Ok);
            await messageBox.ShowAsync();
        }
        finally
        {
            IsLoading = false;
        }
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
