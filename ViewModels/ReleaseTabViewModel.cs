using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.ViewModels;

public partial class ReleaseTabViewModel : ObservableObject
{
    private readonly Window _window;
    private PersistentDataManager _dataManager;
    private DiscordManager _discordManager;
    private GithubManager _githubManager;
    private RepositoryManager _repositoryManager;
    private string _username = "Unknown";
    private string _repoName = "Unknown";
    public ReleaseTabViewModel(Window window, PersistentDataManager dataManager, DiscordManager discordManager, GithubManager githubManager, RepositoryManager repositoryManager)
    {
        _window = window;
        _dataManager = dataManager;
        _discordManager = discordManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;

        ReparseUserDataAsync();
    }

    public async Task ReparseUserDataAsync()
    {
        Username = await _githubManager.GetGithubDisplayNameAsync();
        var remoteUrl = _repositoryManager.Repository.Network.Remotes["origin"].Url;
        string repoName = Path.GetFileNameWithoutExtension(remoteUrl);
        RepoName = repoName;
    }
    
    // Bindable Username
    public string Username
    {
        get => _username;
        set
        {
            if (_username != value)
            {
                _username = value;
                OnPropertyChanged(nameof(UserRepoDisplay)); // Update combined property
            }
        }
    }

    // Bindable RepoName
    public string RepoName
    {
        get => _repoName;
        set
        {
            if (_repoName != value)
            {
                _repoName = value;
                OnPropertyChanged(nameof(UserRepoDisplay)); // Update combined property
            }
        }
    }
    
    

    // Combined property for UI
    public string UserRepoDisplay => $"{Username} : [{RepoName}]";

    // Text editor
    [ObservableProperty]
    private string _editorText = string.Empty;

    // Checkboxes
    [ObservableProperty]
    private bool _option1;

    [ObservableProperty]
    private string _roleToPing;
    
    [RelayCommand]
    private async Task OpenSettings()
    {
        var settingsWindow = new SettingsWindow(_dataManager,_discordManager, _githubManager,_repositoryManager);
        await settingsWindow.ShowDialog(_window);
    }


    [ObservableProperty]
    private bool _option2;
    
    [ObservableProperty]
    private bool _isLoading;

    // Version field
    [ObservableProperty]
    private string _version = string.Empty;

    // ListBox items and selection
    public ObservableCollection<string> Choices { get; } = new()
    {
        "RCR папищеки", "Choice B", "Choice C"
    };

    [ObservableProperty]
    private ObservableCollection<string> _selectedChoices = new();

    // Submit button
    [RelayCommand]
    private async void Submit()
    {
        if (string.IsNullOrEmpty(_dataManager.Settings.GitHubToken))
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка",
                "Сначала нужно залогинится в GitHub", ButtonEnum.Ok);
            await messageBox.ShowAsync();
            return;
        }

        try
        {
            IsLoading = true; // Show loading screen

            // Offload packaging to background thread
            var package = await Task.Run(() => _repositoryManager.PackageLocalization(Version));

            await _githubManager.CreateReleaseAsync($"RCR v{Version}", EditorText, package);

            var discordMessage = $"# v{Version}\n\n" + EditorText;
            if (Option1)
                discordMessage += "\n\n[Ссылка на релиз](https://github.com/enqenqenqenqenq/RCR/releases/latest)";
            if (Option2)
                discordMessage += $"\n\n\n\n<@&{RoleToPing}>";
            
            
            await _discordManager.SendMessageAsync(discordMessage);
        }
        catch (Exception ex)
        {
            var messageBox = MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message, ButtonEnum.Ok);
            await messageBox.ShowAsync();
        }
        finally
        {
            IsLoading = false; // Hide loading screen
        }
    }


    // Window commands
    [RelayCommand]
    private void Minimize()
    {
        _window.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    private void Maximize()
    {
        _window.WindowState = _window.WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    [RelayCommand]
    private void Close()
    {
        _window.Close();
    }
}