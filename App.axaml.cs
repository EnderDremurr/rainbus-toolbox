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
        SetupExceptionHandlers();
    }

    private void SetupExceptionHandlers()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

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
        Console.WriteLine(FormatExceptionText(exception)); // log to console
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await ShowExceptionDialogAsync(exception);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while showing dialog: {ex}");
                (ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
            }
        });
    }

    // Non-fatal exception handler - just informs the user, doesn't shut down
    public async Task HandleNonFatalExceptionAsync(Exception exception, string? userFriendlyMessage = null)
    {
        Console.WriteLine($"Non-fatal exception: {FormatExceptionText(exception)}"); // log to console
        
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                await ShowNonFatalExceptionDialogAsync(exception, userFriendlyMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while showing non-fatal error dialog: {ex}");
                // Don't shut down for non-fatal exceptions, just log the error
            }
        });
    }

    // Convenience method for quick non-fatal error notifications
    public async Task ShowErrorNotificationAsync(string message, string? title = null)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            try
            {
                var dialog = new Window
                {
                    Title = title ?? "Error",
                    Width = 400,
                    Height = 200,
                    CanResize = false,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(20),
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap
                };

                var button = new Button
                {
                    Content = "OK",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    Margin = new Thickness(20)
                };

                button.Click += (_, _) => dialog.Close();

                var stackPanel = new StackPanel();
                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(button);

                dialog.Content = stackPanel;

                var desktopLifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                Window? parentWindow = desktopLifetime?.MainWindow;

                if (parentWindow != null)
                {
                    await dialog.ShowDialog(parentWindow);
                }
                else
                {
                    dialog.Show();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception while showing error notification: {ex}");
            }
        });
    }

    private async Task ShowExceptionDialogAsync(Exception exception)
    {
        var errorText = FormatExceptionText(exception);
        var dialog = new ExceptionDialog(errorText);

        var desktopLifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        Window? parentWindow = desktopLifetime?.MainWindow;

        if (parentWindow != null)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }

        desktopLifetime?.Shutdown();
    }

    private async Task ShowNonFatalExceptionDialogAsync(Exception exception, string? userFriendlyMessage = null)
    {
        var dialog = new Window
        {
            Title = "Error",
            Width = 500,
            Height = 400,
            CanResize = true,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        var mainPanel = new StackPanel { Margin = new Thickness(20) };

        // User-friendly message
        var messageText = userFriendlyMessage ?? "An error occurred, but the application can continue running.";
        var messageBlock = new TextBlock
        {
            Text = messageText,
            FontSize = 14,
            FontWeight = Avalonia.Media.FontWeight.Medium,
            Margin = new Thickness(0, 0, 0, 20),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        mainPanel.Children.Add(messageBlock);

        // Details section
        var detailsExpander = new Expander
        {
            Header = "Technical Details",
            Margin = new Thickness(0, 0, 0, 20)
        };

        var detailsText = new TextBox
        {
            Text = FormatExceptionText(exception),
            IsReadOnly = true,
            AcceptsReturn = true,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Height = 200,
        };

        detailsExpander.Content = detailsText;
        mainPanel.Children.Add(detailsExpander);

        // Buttons
        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
        };

        var okButton = new Button
        {
            Content = "OK",
            Margin = new Thickness(10, 0, 0, 0),
            MinWidth = 80
        };

        okButton.Click += (_, _) => dialog.Close();
        buttonPanel.Children.Add(okButton);

        mainPanel.Children.Add(buttonPanel);
        dialog.Content = mainPanel;

        var desktopLifetime = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        Window? parentWindow = desktopLifetime?.MainWindow;

        if (parentWindow != null)
        {
            await dialog.ShowDialog(parentWindow);
        }
        else
        {
            dialog.Show();
        }
    }

    private string FormatExceptionText(Exception exception)
    {
        var text = $"An unexpected error occurred:\n\n";
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
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            var services = new ServiceCollection();

            // Singletons
            services.AddSingleton<PersistentDataManager>();
            services.AddSingleton<RepositoryManager>();
            services.AddSingleton<GithubManager>();
            services.AddSingleton<KeyWordConversionService>();

            // Windows and VMs
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<InitializationWindow>();
            services.AddTransient<InitializationWindowViewModel>();
            services.AddTransient<SettingsWindow>();
            services.AddSingleton<ReleaseTabViewModel>();
            services.AddSingleton<ViewModelLocator>();

            _serviceProvider = services.BuildServiceProvider();

            try
            {
                var repoManager = _serviceProvider.GetRequiredService<RepositoryManager>();
                Locator = _serviceProvider.GetRequiredService<ViewModelLocator>();

                if (repoManager.IsValid)
                {
                    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                    mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                    desktop.MainWindow = mainWindow;
                }
                else
                {
                    var initWindow = _serviceProvider.GetRequiredService<InitializationWindow>();
                    initWindow.DataContext = _serviceProvider.GetRequiredService<InitializationWindowViewModel>();
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
        var window = _serviceProvider.GetRequiredService<TWindow>();
        window.DataContext = _serviceProvider.GetRequiredService<TViewModel>();
        window.Show();
        return window;
    }
}