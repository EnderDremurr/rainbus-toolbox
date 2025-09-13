using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Views;

namespace RainbusToolbox;

public partial class App : Application
{
    private IServiceProvider _serviceProvider;
    public IServiceProvider ServiceProvider => _serviceProvider;
    public static ViewModelLocator Locator { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            // Set up DI container
            var services = new ServiceCollection();

            // Singletons for managers
            services.AddSingleton<PersistentDataManager>();
            services.AddSingleton<RepositoryManager>();
            services.AddSingleton<DiscordManager>();
            services.AddSingleton<GithubManager>();

            // Transient windows and their ViewModels
            services.AddTransient<MainWindow>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<InitializationWindow>();
            services.AddTransient<InitializationWindowViewModel>();
            services.AddTransient<SettingsWindow>();
            services.AddTransient<ReleaseTabViewModel>();
            services.AddSingleton<ViewModelLocator>();

            // Build service provider
            _serviceProvider = services.BuildServiceProvider();

            // Resolve RepositoryManager first to check validity
            var repoManager = _serviceProvider.GetRequiredService<RepositoryManager>();
            Locator = _serviceProvider.GetRequiredService<ViewModelLocator>();

            Window windowToShow;

            if (repoManager.IsValid)
            {
                // Repository valid → show MainWindow
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                windowToShow = mainWindow;
            }
            else
            {
                // Repository invalid → show InitializationWindow
                var initWindow = _serviceProvider.GetRequiredService<InitializationWindow>();
                initWindow.DataContext = _serviceProvider.GetRequiredService<InitializationWindowViewModel>();
                windowToShow = initWindow;
            }

            desktop.MainWindow = windowToShow;
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

    // Helper method to open windows via DI anywhere in the app
    public TWindow OpenWindow<TWindow, TViewModel>()
        where TWindow : Window
        where TViewModel : class
    {
        var window = _serviceProvider.GetRequiredService<TWindow>();
        window.DataContext = _serviceProvider.GetRequiredService<TViewModel>();
        window.Show();
        return window;
    }
}
