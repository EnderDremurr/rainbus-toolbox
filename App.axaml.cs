using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using RainbusTools.ViewModels;
using RainbusTools.Views;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RainbusTools.Converters;
using RainbusTools.Converters.Managers;

namespace RainbusTools;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var collection = new ServiceCollection();
            collection.AddCommonServices();
            var services = collection.BuildServiceProvider();
            

            // Create main window
            var mainWindow = new MainWindow();
            var data = new PersistentDataManager();
            var discord = new DiscordManager(data, mainWindow);
            var repo = new RepositoryManager(data);
            var github = new GithubManager(data,repo);
            // Pass the window instance to the ViewModel
            mainWindow.DataContext = new MainWindowViewModel(mainWindow, data, discord, github, repo);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}