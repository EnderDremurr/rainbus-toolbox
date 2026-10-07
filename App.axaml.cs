using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Utilities;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.Utilities.ExternalServices;
using RainbusToolbox.Utilities.RepositoryServices;
using RainbusToolbox.ViewModels;
using RainbusToolbox.Views;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox;

public class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }
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
        AppDomain.CurrentDomain.UnhandledException += (__, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                _ = HandleGlobalExceptionAsync(ex);
        };

        // Unobserved Task exceptions
        TaskScheduler.UnobservedTaskException += (__, e) =>
        {
            _ = HandleGlobalExceptionAsync(e.Exception);
            e.SetObserved();
        };

        // Avalonia UI thread exceptions
        Dispatcher.UIThread.UnhandledException += (__, e) =>
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

            await PopUpWindow.ShowAsync(parent, "Фатальная ошибка", errorText, false, "", null,
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
            await PopUpWindow.ShowAsync(parent, "Ашыбка",
                userFriendlyMessage ?? exception.Message);
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

            RegisterBackendServices(services);
            RegisterUIServices(services);
            RegisterTranslationEditorVMs(services);

            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.GetRequiredService<CachingService>();

            try
            {
                var repoManager = ServiceProvider.GetRequiredService<LocalizationManager>();
                var dataManager = ServiceProvider.GetRequiredService<PersistentDataManager>();
                var githubManager = ServiceProvider.GetRequiredService<GithubManager>();

                if (repoManager.IsValid && !string.IsNullOrWhiteSpace(dataManager.Settings.GitHubToken) &&
                    !string.IsNullOrWhiteSpace(dataManager.Settings.PathToLimbus))
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

    private static void RegisterBackendServices(ServiceCollection services)
    {
        services.AddSingleton<GitManager>();
        services.AddSingleton<PersistentDataManager>();
        services.AddSingleton<LocalizationManager>();
        services.AddSingleton<GithubManager>();
        services.AddSingleton<KeywordProcessingService>();
        services.AddSingleton<Angela>();
        services.AddSingleton<DiscordRPCService>();
        services.AddSingleton<CachingService>();
        services.AddSingleton<SpellCheckerService>();
        services.AddSingleton<SpellcheckEngine>();
        services.AddSingleton<MassReplacementService>();
        services.AddSingleton<ConfigProvider>();
        services.AddSingleton<EditorFactory>();
    }

    private static void RegisterUIServices(ServiceCollection services)
    {
        // windows
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<InitializationWindow>();
        services.AddSingleton<InitializationWindowViewModel>();

        services.AddTransient<SettingsWindow>();

        // tab viewmodels
        services.AddSingleton<TranslationTabViewModel>();
        services.AddSingleton<FilesTabViewModel>();
        services.AddSingleton<ReleaseTabViewModel>();
        services.AddSingleton<OverviewTabViewModel>();
    }

    private static void RegisterTranslationEditorVMs(ServiceCollection services)
    {
        services.AddKeyedTransient<IFileEditor, StoryTranslationEditorViewModel>(typeof(StoryDataFile));
        services.AddKeyedTransient<IFileEditor, EGOGiftTranslationEditorViewModel>(typeof(EgoGiftsLocalizationFile));
        services.AddKeyedTransient<IFileEditor, SkillsEgoTranslationEditorViewModel>(typeof(SkillLocalizationFile));
        services.AddKeyedTransient<IFileEditor, BattleHintsEditorViewModel>(typeof(NormalBattleHintLocalizationFile));
        services.AddKeyedTransient<IFileEditor, PanicTranslationEditorViewModel>(typeof(PanicInfoLocalizationFile));
        services.AddKeyedTransient<IFileEditor, PassiveTranslationEditorViewModel>(typeof(PassiveLocalizationFile));
        services.AddKeyedTransient<IFileEditor, BattleAnnouncerTranslationEditorViewModel>(
            typeof(AnnouncerVoiceLocalizationFile));
        services.AddKeyedTransient<IFileEditor, KeywordTranslationEditorViewModel>(typeof(KeywordLocalizationFile));
        services.AddKeyedTransient<IFileEditor, PersonalityVoiceTranslationEditorViewModel>(
            typeof(PersonalityVoiceLocalizationFile));
        services.AddKeyedTransient<IFileEditor, EGOVoiceTranslationEditorViewModel>(typeof(EgoVoiceLocalizationFile));
        services.AddKeyedTransient<IFileEditor, AbnormalityGuideTranslationEditorViewModel>(
            typeof(AbnormalityGuideContentLocalizationFile));
        services.AddKeyedTransient<IFileEditor, UiElementTranslationEditorViewModel>(typeof(UiLocalizationFile));


        services.AddTransient<IFileEditor, UnknownFileTranslationEditorViewModel>();
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