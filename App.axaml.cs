using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
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
    public static new App Current => (App)Application.Current!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Set up global exception handlers
        SetupExceptionHandlers();
    }
    
    private void SetupExceptionHandlers()
    {
        // Handle unhandled exceptions in the UI thread
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        // Handle exceptions in background threads
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;

        // Handle unobserved task exceptions
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            // Use Dispatcher to show dialog on UI thread
            Dispatcher.UIThread.InvokeAsync(() => ShowExceptionDialog(exception));
        }
    }

    private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        // Use Dispatcher to show dialog on UI thread
        Dispatcher.UIThread.InvokeAsync(() => ShowExceptionDialog(e.Exception));
        e.SetObserved(); // Mark as observed to prevent app crash
    }

    private async void ShowExceptionDialog(Exception exception)
    {
        try
        {
            var errorText = FormatExceptionText(exception);
            
            // Create and show the error dialog
            var dialog = new ExceptionDialog(errorText);
            
            // Get the desktop lifetime for later use
            var desktopLifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            
            // Get the main window to use as parent
            Window? parentWindow = desktopLifetime?.MainWindow;

            if (parentWindow != null)
            {
                await dialog.ShowDialog(parentWindow);
            }
            else
            {
                dialog.Show();
                await dialog.GetObservable(Window.IsVisibleProperty)
                    .Where(visible => !visible)
                    .Take(1)
                    .ToTask();
            }

            // After dialog is closed, shut down the application
            desktopLifetime?.Shutdown();
        }
        catch
        {
            // If showing the dialog fails, shut down immediately
            var desktopLifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            desktopLifetime?.Shutdown();
        }
    }

    // Public method to handle exceptions from anywhere in the app
    public void HandleGlobalException(Exception exception)
    {
        Dispatcher.UIThread.InvokeAsync(() => ShowExceptionDialog(exception));
    }

    private string FormatExceptionText(Exception exception)
    {
        var text = $"An unexpected error occurred:\n\n";
        text += $"Error Type: {exception.GetType().Name}\n";
        text += $"Message: {exception.Message}\n\n";
        text += $"Stack Trace:\n{exception.StackTrace}";
        
        // Include inner exceptions
        var innerException = exception.InnerException;
        var level = 1;
        while (innerException != null)
        {
            text += $"\n\n--- Inner Exception {level} ---\n";
            text += $"Type: {innerException.GetType().Name}\n";
            text += $"Message: {innerException.Message}\n";
            text += $"Stack Trace:\n{innerException.StackTrace}";
            
            innerException = innerException.InnerException;
            level++;
        }

        return text;
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

            Window windowToShow;

            try
            {
                // Resolve RepositoryManager first to check validity
                var repoManager = _serviceProvider.GetRequiredService<RepositoryManager>();
                Locator = _serviceProvider.GetRequiredService<ViewModelLocator>();

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
            catch (Exception ex)
            {
                // Handle startup exceptions
                ShowExceptionDialog(ex);
                return;
            }
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