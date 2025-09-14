using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Views;

public partial class UpdaterTab : UserControl
{
    private readonly RepositoryManager _repositoryManager;

    public UpdaterTab()
    {
        InitializeComponent();

        // Pull the RepositoryManager out of DI
        _repositoryManager = ((App)Application.Current)
            .ServiceProvider
            .GetRequiredService<RepositoryManager>();
    }

    private void OnParseButtonClick(object? sender, RoutedEventArgs e)
    {
        _repositoryManager.ParseNewAdditionsFromGame();
    }
}