using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox;

public partial class SettingsWindow : Window
{
    private PersistentDataManager _dataManager;
    private GithubManager _githubManager;
    private RepositoryManager _repositoryManager;

    public SettingsWindow(PersistentDataManager manager, GithubManager githubManager, RepositoryManager repositoryManager)
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
        var dialog = new OpenFolderDialog { Title = "Выбери папку с репозиторием" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrEmpty(result))
            RepoPathTextBox.Text = result;
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
        GitHubTokenStatusTextBlock.Text = isAuthorized ? "Ты залогинен" : "Ты не залогинен";
    }

    private void SaveSettings()
    {
        var isDirty = false;
        var didRepoChange = false;
        
        var settingsCache = _dataManager.Settings;
        if (settingsCache.DiscordWebHook != DiscordWebHookBox.Text && DiscordManager.ValidateWebhook(DiscordWebHookBox.Text))
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
            _dataManager.Settings.DeepSeekToken= AngelaTokenBox.Text;
            isDirty = true;
        }

        if (AngelaPromptBox.Text != settingsCache.AngelaPrompt)
        {
            _dataManager.Settings.AngelaPrompt = AngelaPromptBox.Text;
            isDirty = true;
        }
        
        
        if(isDirty)
            _dataManager.Save();
        
        if(didRepoChange)
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
