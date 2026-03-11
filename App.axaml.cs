using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Views;
using RainbusToolbox.Views.Misc;
using Serilog;

namespace RainbusToolbox;

public class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    public static ViewModelLocator Locator { get; private set; }
    public new static App Current => (App)Application.Current!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        SetupExceptionHandlers();
    }

    private void SetupExceptionHandlers()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

        // CLR-level unhandled exceptions (non-UI threads)
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                _ = HandleGlobalExceptionAsync(ex);
        };

        // Unobserved Task exceptions
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            _ = HandleGlobalExceptionAsync(e.Exception);
            e.SetObserved();
        };

        // Avalonia UI thread exceptions
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            _ = HandleGlobalExceptionAsync(e.Exception);
            e.Handled = true; // prevent Avalonia from shutting down immediately
        };
    }

    // Global exception handler for fatal exceptions
    public async Task HandleGlobalExceptionAsync(Exception exception)
    {
        Log.Fatal(exception, "We are cooked. FATAL");
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var desktop = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var parent = desktop?.MainWindow;
            if (parent == null) return;

            var clipboard = TopLevel.GetTopLevel(parent)?.Clipboard;
            var errorText = FormatExceptionText(exception);

            await PopUpWindow.ShowAsync(parent, "Фатальная ошибка", errorText, false, "",
                new PopupButton
                {
                    Label = "Copy Error",
                    ResultValue = "copy",
                    KeepOpen = true,
                    OnClick = () => clipboard?.SetTextAsync(errorText)
                },
                new PopupButton { Label = "Close Application", ResultValue = "ok" }
            );

            desktop?.Shutdown();
        });
    }

    // Non-fatal exception handler - just informs the user, doesn't shut down
    public async Task HandleNonFatalExceptionAsync(Exception exception, string? userFriendlyMessage = null)
    {
        Log.Error(exception, userFriendlyMessage ?? "Error");
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var parent = (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            if (parent == null) return;
            await PopUpWindow.ShowAsync(parent, "Error",
                userFriendlyMessage ?? "An error occurred, but the application can continue.");
        });
    }


    private string FormatExceptionText(Exception exception)
    {
        var text = "An unexpected error occurred:\n\n";
        text += $"Error Type: {exception.GetType().Name}\n";
        text += $"Message: {exception.Message}\n\n";
        text += $"Stack Trace:\n{exception.StackTrace}";

        var inner = exception.InnerException;
        var level = 1;
        while (inner != null)
        {
            text += $"\n\n--- Inner Exception {level} ---\n";
            text += $"Type: {inner.GetType().Name}\n";
            text += $"Message: {inner.Message}\n";
            text += $"Stack Trace:\n{inner.StackTrace}";

            inner = inner.InnerException;
            level++;
        }

        return text;
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (Design.IsDesignMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var services = new ServiceCollection();

            // Singletons
            services.AddSingleton<PersistentDataManager>();
            services.AddSingleton<RepositoryManager>();
            services.AddSingleton<GithubManager>();
            services.AddSingleton<KeywordProcessingService>();
            services.AddSingleton<Angela>();
            services.AddSingleton<DiscordRPCService>();

            // Windows and VMs
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<InitializationWindow>();
            services.AddTransient<InitializationWindowViewModel>();
            services.AddTransient<SettingsWindow>();
            services.AddSingleton<ReleaseTabViewModel>();
            services.AddSingleton<ViewModelLocator>();

            ServiceProvider = services.BuildServiceProvider();

            try
            {
                var repoManager = ServiceProvider.GetRequiredService<RepositoryManager>();
                Locator = ServiceProvider.GetRequiredService<ViewModelLocator>();

                if (repoManager.IsValid)
                {
                    var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>();
                    desktop.MainWindow = mainWindow;
                }
                else
                {
                    var initWindow = ServiceProvider.GetRequiredService<InitializationWindow>();
                    initWindow.DataContext = ServiceProvider.GetRequiredService<InitializationWindowViewModel>();
                    desktop.MainWindow = initWindow;
                }
            }
            catch (Exception ex)
            {
                _ = HandleGlobalExceptionAsync(ex);
                return;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var toRemove = BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();
        foreach (var plugin in toRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }

    public TWindow OpenWindow<TWindow, TViewModel>()
        where TWindow : Window
        where TViewModel : class
    {
        var window = ServiceProvider.GetRequiredService<TWindow>();
        window.DataContext = ServiceProvider.GetRequiredService<TViewModel>();
        window.Show();
        return window;
    }
}