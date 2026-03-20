using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.ViewModels;

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

        _repoPathTextBox = this.FindControl<TextBox>("RepoPathTextBox");
        _limbusPathTextBox = this.FindControl<TextBox>("LimbusPathTextBox");
        _gitHubTokenStatusTextBlock = this.FindControl<TextBlock>("GitHubTokenStatusTextBlock");

        LoadPathsAndTokenStatus();

        Closing += (_, __) => SavePaths();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);

        // Load the image to get its size
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://RainbusToolbox/Assets/Init.png")));

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
        var dialog = new OpenFolderDialog { Title = "Выбери папку с репозиторием" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrWhiteSpace(result))
            _repoPathTextBox.Text = result;
    }

    private async void BrowseLimbusFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Выбери папку с файлами игры" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrWhiteSpace(result))
            _limbusPathTextBox.Text = result;
    }

    private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }

    private void RestartApp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (Application.Current is App app) app.OpenWindow<MainWindow, MainWindowViewModel>();

            Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to open MainWindow: " + ex.Message);
        }
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
        catch
        {
        }
    }

    private void SavePaths()
    {
        _dataManager.Settings.RepositoryPath = _repoPathTextBox.Text;
        _dataManager.Settings.PathToLimbus = _limbusPathTextBox.Text;
        _dataManager.Save();
    }
}