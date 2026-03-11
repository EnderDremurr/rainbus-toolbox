using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.ViewModels;

public partial class FilesTabViewModel : ObservableObject
{
    private readonly KeywordProcessingService _keywordProcessingService =
        (App.Current.ServiceProvider.GetService(typeof(KeywordProcessingService)) as KeywordProcessingService)!;

    private readonly RepositoryManager _repositoryManager =
        (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;

    private CancellationTokenSource? _cancellationTokenSource;


    [RelayCommand]
    private async Task ParseFilesAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            LoadingScreenViewModel.StartLoading("Обработка файлов...");

            var mergingService = new FileMergingService();
            // Create progress reporter
            var progress = new Progress<string>(message =>
            {
                // Try to extract stats from progress message
                if (message.Contains("Processed") && message.Contains("/"))
                    try
                    {
                        var parts = message.Split('/');
                        if (parts.Length >= 2)
                        {
                            var processed = int.Parse(parts[0].Split(' ').Last());
                            var total = int.Parse(parts[1].Split(' ')[0]);

                            LoadingScreenViewModel.SetProgress(processed, total);
                        }
                    }
                    catch
                    {
                        // If parsing fails, just show the message as is
                    }
            });

            var result = await mergingService.PullFilesFromTheGameAsync(
                _repositoryManager.PathToLocalization,
                _repositoryManager.PathToReferenceLocalization,
                _cancellationTokenSource.Token,
                progress
            );

            var newFiles = result[0];
            var mergedFiles = result[1];
            var totalFiles = result[2];

            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Готово!",
                $"Добавлено файлов: {newFiles}, Объединено файлов: {mergedFiles}, Всего обработано: {totalFiles}");
        }
        catch (OperationCanceledException)
        {
            LoadingScreenViewModel.SetText("Операция отменена пользователем");
        }
        catch (Exception ex)
        {
            LoadingScreenViewModel.SetText("Ошибка");

            _ = App.Current.HandleNonFatalExceptionAsync(ex, "Ошибка при замене тегов");
        }
        finally
        {
            LoadingScreenViewModel.FinishLoading();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }


    [RelayCommand]
    public async Task ReplaceAllTagsWithMeshesAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            LoadingScreenViewModel.StartLoading("Замена тегов...");
            var progress = new Progress<string>(message =>
            {
                if (message.Contains("Processed") && message.Contains("/"))
                    try
                    {
                        var parts = message.Split('/');
                        if (parts.Length >= 2)
                        {
                            var processed = int.Parse(parts[0].Split(' ').Last());
                            var totalPart = parts[1].Split(' ')[0];
                            var total = int.Parse(totalPart);

                            LoadingScreenViewModel.SetProgress(processed, total);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
            });

            var finalProcessed = await _keywordProcessingService.ReplaceEveryTagWithMesh(
                _repositoryManager.PathToLocalization,
                _cancellationTokenSource.Token,
                progress
            );


            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Готово!",
                $"Теги были заменены в {finalProcessed} файлов.");
        }
        catch (OperationCanceledException)
        {
            LoadingScreenViewModel.SetText("Операция отменена пользователем");
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex, "Ошибка при замене тегов");
        }
        finally
        {
            LoadingScreenViewModel.FinishLoading();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand]
    public async Task PullNewKeywordsFromTheGame()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            LoadingScreenViewModel.StartLoading("Начинается поиск новых кейвордов...");
            var progress = new Progress<string>(LoadingScreenViewModel.SetText);
            await _keywordProcessingService.PullNewKeywordsFromTheGame(
                _cancellationTokenSource.Token,
                progress
            );
            LoadingScreenViewModel.SetText("Готово!");
            await Task.Delay(TimeSpan.FromSeconds(1), _cancellationTokenSource.Token); //replace with popup report later
        }
        catch (OperationCanceledException)
        {
            LoadingScreenViewModel.SetText("Операция отменена пользователем");
        }
        catch (Exception ex)
        {
            LoadingScreenViewModel.SetText($"Ошибка: {ex.Message}");

            _ = App.Current.HandleNonFatalExceptionAsync(ex, "Ошибка при замене тегов");
        }
        finally
        {
            LoadingScreenViewModel.FinishLoading();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    #region Events

    public void OnTabOpened()
    {
        var rpc = App.Current.ServiceProvider.GetService(typeof(DiscordRPCService)) as DiscordRPCService;

        rpc!.SetState("Люто обновляет файлы");
    }

    #endregion
}