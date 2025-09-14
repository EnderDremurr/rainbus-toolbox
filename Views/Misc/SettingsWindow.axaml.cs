using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System.Threading.Tasks;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox;

public partial class SettingsWindow : Window
{
    private PersistentDataManager _dataManager;
    private GithubManager _githubManager;
    private DiscordManager _discordManager;
    private RepositoryManager _repositoryManager;

    private TextBox _discordWebHookBox;
    private TextBox _repoPathTextBox;
    private TextBlock _gitHubTokenStatusTextBlock;
    private TextBox _pathToLimbus;

    public SettingsWindow(PersistentDataManager manager, DiscordManager discordManager, GithubManager githubManager, RepositoryManager repositoryManager)
    {
        InitializeComponent();

        _dataManager = manager;
        _discordManager = discordManager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;

        _discordWebHookBox = this.FindControl<TextBox>("DiscordWebHookBox");
        _repoPathTextBox = this.FindControl<TextBox>("RepoPathTextBox");
        _gitHubTokenStatusTextBlock = this.FindControl<TextBlock>("GitHubTokenStatusTextBlock");
        _pathToLimbus = this.FindControl<TextBox>("LimbusPathTextBox");
        

        LoadSettings();

        // Save settings when window is closing
        this.Closing += (_, __) => SaveSettings();
    }

    // Toolbar drag
    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            this.BeginMoveDrag(e);
    }

    // Close button
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    // Folder picker
    private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Выбери папку с репозиторием" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrEmpty(result))
            _repoPathTextBox.Text = result;
    }
    
    private async void BrowseLimbusFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Выбери папку с бимбус кемпани МЕНЕДЖЕР ЭСКВАЕР!!!" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrEmpty(result))
            LimbusPathTextBox.Text = result;
    }

    // GitHub token button
    private async void SetGitHubToken_Click(object sender, RoutedEventArgs e)
    {
        await _githubManager.RequestGithubAuthAsync(this);

        var isAuthorized = !string.IsNullOrEmpty(_dataManager.Settings.GitHubToken);
        _gitHubTokenStatusTextBlock.Text = isAuthorized ? "Ты залогинен" : "Ты не залогинен";
    }

    private void SaveSettings()
    {
        // Always read the current value of the webhook box
        _dataManager.Settings.DiscordWebHook = _discordWebHookBox.Text;
        _dataManager.Settings.RepositoryPath = _repoPathTextBox.Text;
        _dataManager.Settings.PathToLimbus = LimbusPathTextBox.Text;
        _dataManager.Save();
        _discordManager.TryInitialize(this);
        _repositoryManager.TryInitialize();
    }
    private void ToggleWebhookVisibility_Click(object sender, RoutedEventArgs e)
    {
        if (DiscordWebHookBox.PasswordChar == '*')
        {
            DiscordWebHookBox.PasswordChar = '\0'; // Show text
            ToggleWebhookButton.Content = "Скрыть";
        }
        else
        {
            DiscordWebHookBox.PasswordChar = '*'; // Mask text
            ToggleWebhookButton.Content = "Показать";
        }
    }


    private void LoadSettings()
    {
        try
        {
            var data = _dataManager.Settings;
            _discordWebHookBox.Text = data.DiscordWebHook ?? "";
            _repoPathTextBox.Text = data.RepositoryPath ?? "";
            LimbusPathTextBox.Text = data.PathToLimbus ?? "";

            _gitHubTokenStatusTextBlock.Text = string.IsNullOrEmpty(data.GitHubToken)
                ? "Ты не залогинен"
                : "Ты залогинен";
        }
        catch { }
    }
}
