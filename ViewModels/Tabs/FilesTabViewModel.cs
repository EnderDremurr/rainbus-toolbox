using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;

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
            LoadingScreenViewModel.StartLoading("Начинается обработку файлов...");

            var mergingService = new FileMergingService();

            // Create progress reporter
            var progress = new Progress<string>(message =>
            {
                LoadingScreenViewModel.SetText(message);

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

                            // Extract stats if available
                            if (message.Contains("Added:") && message.Contains("Merged:"))
                            {
                                var addedStart = message.IndexOf("Added: ", StringComparison.Ordinal) + 7;
                                var mergedStart = message.IndexOf("Merged: ", StringComparison.Ordinal) + 8;
                                var addedEnd = message.IndexOf(",", addedStart, StringComparison.Ordinal);
                                var mergedEnd = message.IndexOf(")", mergedStart, StringComparison.Ordinal);

                                if (addedEnd > addedStart && mergedEnd > mergedStart)
                                {
                                    var added = message.Substring(addedStart, addedEnd - addedStart);
                                    var merged = message.Substring(mergedStart, mergedEnd - mergedStart);
                                    // display these in popup when i'll implement it later
                                }
                            }
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

            // Show final results
            var newFiles = result[0];
            var mergedFiles = result[1];
            var totalFiles = result[2];

            // ProcessingStats =
            // $"Добавлено файлов: {newFiles}, Объединено файлов: {mergedFiles}, Всего обработано: {totalFiles}";
            // display these in popup when i'll implement it later

            await Task.Delay(TimeSpan.FromSeconds(2),
                _cancellationTokenSource.Token); // replace with popup report later
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
            LoadingScreenViewModel.StartLoading("Начинается замена тегов...");

            var progress = new Progress<string>(message =>
            {
                LoadingScreenViewModel.SetText(message);

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

            await _keywordProcessingService.ReplaceEveryTagWithMesh(
                _repositoryManager.PathToLocalization,
                _cancellationTokenSource.Token,
                progress
            );

            await Task.Delay(TimeSpan.FromSeconds(5), _cancellationTokenSource.Token); //replace with popup report later
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