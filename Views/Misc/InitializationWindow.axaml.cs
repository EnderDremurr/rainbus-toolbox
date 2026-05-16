using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.Views;

public partial class InitializationWindow : Window
{
    private readonly PersistentDataManager _dataManager;
    private readonly GithubManager _githubManager;
    private readonly TextBlock _gitHubTokenStatusTextBlock;
    private readonly TextBox _limbusPathTextBox;

    private readonly TextBox _repoPathTextBox;

    public InitializationWindow(PersistentDataManager dataManager, GithubManager githubManager)
    {
        _dataManager = dataManager;
        _githubManager = githubManager;

        InitializeComponent();


        _repoPathTextBox = RepoPathTextBox;
        _limbusPathTextBox = LimbusPathTextBox;
        _gitHubTokenStatusTextBlock = GitHubTokenStatusTextBlock;

        LoadPathsAndTokenStatus();

        Closing += (_, __) => SavePaths();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Load the image to get its size
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://RainbusToolbox/Assets/Backgrounds/Init.png")));

        Width = bitmap.PixelSize.Width / 1.5f;
        Height = bitmap.PixelSize.Height / 1.5f;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }


    private async void SetGitHubToken_Click(object sender, RoutedEventArgs e)
    {
        await _githubManager.RequestGithubAuthAsync(this);

        var isAuthorized = !string.IsNullOrWhiteSpace(_dataManager.Settings.GitHubToken);
        _gitHubTokenStatusTextBlock.Text = isAuthorized ? "Ты залогинен" : "Ты не залогинен";
    }

    private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var top =
            Application.Current!.ApplicationLifetime is
                IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        if (top == null) return;
        var storage = top.StorageProvider;
        var pickedFolder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с репозиторием",
            AllowMultiple = false
        });
        if (pickedFolder.Count == 0) return;
        var result = pickedFolder[0].Path.ToString();


        var validatedPath = PersistentDataManager.ValidateRepoPath(result);
        if (validatedPath != null)
        {
            _dataManager.Settings.RepositoryPath = validatedPath;
            _repoPathTextBox.Text = validatedPath;
        }
    }

    private async void BrowseLimbusFolder_Click(object sender, RoutedEventArgs e)
    {
        var top =
            Application.Current!.ApplicationLifetime is
                IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        if (top == null) return;
        var storage = top.StorageProvider;
        var pickedFolder = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Выбери папку с лимбусом",
            AllowMultiple = false
        });
        if (pickedFolder.Count == 0) return;
        var result = pickedFolder[0].Path.ToString();

        var validatedPath = PersistentDataManager.ValidateLimbusPath(result);
        if (validatedPath != null)
        {
            _dataManager.Settings.PathToLimbus = validatedPath;
            _limbusPathTextBox.Text = validatedPath;
        }
    }

    private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void RestartApp_Click(object sender, RoutedEventArgs e)
    {
        SavePaths();

        var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
        _ = PopUpWindow.ShowAsync(parent!, "Сохранено!",
            "Настройки были успешно сохранены, теперь нужно просто перезапустить прогу!");
    }


    private void LoadPathsAndTokenStatus()
    {
        try
        {
            var data = _dataManager.Settings;
            _repoPathTextBox.Text = data.RepositoryPath ?? @"C:\Path\To\Repo";
            _limbusPathTextBox.Text = data.PathToLimbus ?? @"C:\Path\To\Limbus";

            _gitHubTokenStatusTextBlock.Text = string.IsNullOrWhiteSpace(data.GitHubToken)
                ? "Ты не залогинен"
                : "Ты залогинен";
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex);
        }
    }

    private void SavePaths()
    {
        _dataManager.Save();
    }
}