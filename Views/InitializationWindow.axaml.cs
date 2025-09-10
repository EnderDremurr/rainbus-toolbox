using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using RainbusTools.Models.Managers;
using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RainbusTools.ViewModels;
using Avalonia.Platform;
using Avalonia.Media.Imaging;
using System;

namespace RainbusTools.Views;

public partial class InitializationWindow : Window
{
    private PersistentDataManager _dataManager;
    private GithubManager _githubManager;

    private TextBox _repoPathTextBox;
    private TextBox _limbusPathTextBox;
    private TextBlock _gitHubTokenStatusTextBlock;

    public InitializationWindow(PersistentDataManager dataManager, GithubManager githubManager)
    {
        _dataManager = dataManager;
        _githubManager = githubManager;

        InitializeComponent();

        _repoPathTextBox = this.FindControl<TextBox>("RepoPathTextBox");
        _limbusPathTextBox = this.FindControl<TextBox>("LimbusPathTextBox");
        _gitHubTokenStatusTextBlock = this.FindControl<TextBlock>("GitHubTokenStatusTextBlock");

        LoadPathsAndTokenStatus();

        this.Closing += (_, __) => SavePaths();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Load the image to get its size
        var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://RainbusTools/Assets/Init.png")));

        this.Width = bitmap.PixelSize.Width/1.5f;
        this.Height = bitmap.PixelSize.Height/1.5f;
        
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
    
    private void TitleBar_OnPointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            this.BeginMoveDrag(e);
    }


    private async void SetGitHubToken_Click(object sender, RoutedEventArgs e)
    {
        await _githubManager.RequestGithubAuthAsync(this);

        var isAuthorized = !string.IsNullOrEmpty(_dataManager.Settings.GitHubToken);
        _gitHubTokenStatusTextBlock.Text = isAuthorized ? "Ты залогинен" : "Ты не залогинен";
    }

    private async void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Выбери папку с репозиторием" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrEmpty(result))
            _repoPathTextBox.Text = result;
    }

    private async void BrowseLimbusFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Выбери папку с файлами игры" };
        var result = await dialog.ShowAsync(this);
        if (!string.IsNullOrEmpty(result))
            _limbusPathTextBox.Text = result;
    }
    private void Window_OnPointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            this.BeginMoveDrag(e);
    }

    private void RestartApp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Use the App's service provider to open MainWindow
            if (Application.Current is App app)
            {
                // Open MainWindow with its ViewModel via DI
                app.OpenWindow<MainWindow, MainWindowViewModel>();
            }

            // Close the current initialization window
            this.Close();
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

            _gitHubTokenStatusTextBlock.Text = string.IsNullOrEmpty(data.GitHubToken)
                ? "Ты не залогинен"
                : "Ты залогинен";
        }
        catch { }
    }

    private void SavePaths()
    {
        _dataManager.Settings.RepositoryPath = _repoPathTextBox.Text;
        _dataManager.Settings.PathToLimbus = _limbusPathTextBox.Text;
        _dataManager.Save();
    }
}
