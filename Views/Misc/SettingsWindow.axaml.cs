using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox;

public partial class SettingsWindow : Window
{
    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly RepositoryManager _repositoryManager;

    public SettingsWindow(PersistentDataManager manager, GithubManager githubManager,
        RepositoryManager repositoryManager)
    {
        InitializeComponent();

        _dataManager = manager;
        _githubManager = githubManager;
        _repositoryManager = repositoryManager;

        LoadSettings();

        // Save settings when window is closing
        Closing += (_, _) => SaveSettings();
    }

    // Toolbar drag
    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    // Close button
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Folder picker
    private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        var folders = await topLevel!.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Выбери папку с репозиторием!!!"
            });

        if (folders.Count > 0)
            RepoPathTextBox.Text = folders[0].Path.LocalPath;
    }

    private async void BrowseLimbusFolder_Click(object sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        var folders = await topLevel!.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                Title = "Выбери папку с бимбус кемпани МЕНЕДЖЕР ЭСКВАЕР!!!"
            });

        if (folders.Count > 0)
            LimbusPathTextBox.Text = folders[0].Path.LocalPath;
    }

    private void OpenFolderWithLogs_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RainbusToolbox", "logs");

        Directory.CreateDirectory(path);

        if (OperatingSystem.IsWindows())
            Process.Start("explorer.exe", path);
        else if (OperatingSystem.IsMacOS())
            Process.Start("open", path);
        else if (OperatingSystem.IsLinux())
            Process.Start("xdg-open", path);
    }

    // GitHub token button
    private async void SetGitHubToken_Click(object sender, RoutedEventArgs e)
    {
        await _githubManager.RequestGithubAuthAsync(this);

        var isAuthorized = !string.IsNullOrEmpty(_dataManager.Settings.GitHubToken);
        GitHubTokenStatusTextBlock.Text = isAuthorized ? "Ты залогинен" : "Ты не залогинен";
    }

    private void SaveSettings()
    {
        var isDirty = false;
        var didRepoChange = false;

        var settingsCache = _dataManager.Settings;
        if (settingsCache.DiscordWebHook != DiscordWebHookBox.Text &&
            DiscordManager.ValidateWebhook(DiscordWebHookBox.Text))
        {
            _dataManager.Settings.DiscordWebHook = DiscordWebHookBox.Text;
            isDirty = true;
        }

        if (settingsCache.RepositoryPath != RepoPathTextBox.Text)
        {
            _dataManager.Settings.RepositoryPath = RepoPathTextBox.Text;
            isDirty = true;
            didRepoChange = true;
        }

        if (settingsCache.PathToLimbus != LimbusPathTextBox.Text)
        {
            _dataManager.Settings.PathToLimbus = LimbusPathTextBox.Text;
            isDirty = true;
        }

        if (AngelaTokenBox.Text != settingsCache.DeepSeekToken)
        {
            _dataManager.Settings.DeepSeekToken = AngelaTokenBox.Text;
            isDirty = true;
        }

        if (AngelaPromptBox.Text != settingsCache.AngelaPrompt)
        {
            _dataManager.Settings.AngelaPrompt = AngelaPromptBox.Text;
            isDirty = true;
        }

        if (DiscordRoleToPingBox.Text != settingsCache.DiscordRoleToPing)
        {
            _dataManager.Settings.DiscordRoleToPing = DiscordRoleToPingBox.Text;
            isDirty = true;
        }

        if (isDirty)
            _dataManager.Save();

        if (didRepoChange)
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
            DiscordWebHookBox.Text = data.DiscordWebHook ?? "";
            RepoPathTextBox.Text = data.RepositoryPath ?? "";
            LimbusPathTextBox.Text = data.PathToLimbus ?? "";
            AngelaPromptBox.Text = data.AngelaPrompt ?? "";
            AngelaTokenBox.Text = data.DeepSeekToken ?? "";

            GitHubTokenStatusTextBlock.Text = string.IsNullOrEmpty(data.GitHubToken)
                ? "Ты не залогинен"
                : "Ты залогинен";
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleGlobalExceptionAsync(ex);
        }
    }
}