using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services;

namespace RainbusToolbox.ViewModels;

public partial class FilesTabViewModel : ObservableObject
{
    private readonly RepositoryManager _repositoryManager;
    private readonly KeyWordConversionService _keyWordConversionService;
    private CancellationTokenSource? _cancellationTokenSource;

    public FilesTabViewModel()
    {
        _repositoryManager = (App.Current.ServiceProvider.GetService(typeof(RepositoryManager)) as RepositoryManager)!;
        _keyWordConversionService =
            (App.Current.ServiceProvider.GetService(typeof(KeyWordConversionService)) as KeyWordConversionService)!;
    }

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _progressMessage = "Готов к работе";

    [ObservableProperty]
    private bool _isProgressIndeterminate = true;

    [ObservableProperty]
    private double _progressValue;

    [ObservableProperty]
    private double _progressMaximum = 100;

    [ObservableProperty]
    private string _processingStats = "";

    [RelayCommand(CanExecute = nameof(CanExecuteParseFiles))]
    private async Task ParseFilesAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsProcessing = true;
            ProgressMessage = "Начинаем обработку файлов...";
            IsProgressIndeterminate = true;
            ProcessingStats = "";

            var mergingService = new FileMergingService();

            // Create progress reporter
            var progress = new Progress<string>(message =>
            {
                ProgressMessage = message;

                // Try to extract stats from progress message
                if (message.Contains("Processed") && message.Contains("/"))
                    try
                    {
                        var parts = message.Split('/');
                        if (parts.Length >= 2)
                        {
                            var processed = int.Parse(parts[0].Split(' ').Last());
                            var total = int.Parse(parts[1].Split(' ')[0]);

                            IsProgressIndeterminate = false;
                            ProgressValue = processed;
                            ProgressMaximum = total;

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
                                    ProcessingStats = $"Добавлено: {added}, Объединено: {merged}";
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

            ProgressMessage = "Завершено успешно!";
            ProcessingStats =
                $"Добавлено файлов: {newFiles}, Объединено файлов: {mergedFiles}, Всего обработано: {totalFiles}";
            IsProgressIndeterminate = false;
            ProgressValue = ProgressMaximum;
            
            await Task.Delay(TimeSpan.FromSeconds(10), _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            ProgressMessage = "Операция отменена пользователем";
            ProcessingStats = "";
        }
        catch (Exception ex)
        {
            ProgressMessage = $"Ошибка: {ex.Message}";
            ProcessingStats = "";

            // Show error notification if available
            try
            {
                _ = App.Current.ShowErrorNotificationAsync($"Ошибка при обработке файлов: {ex.Message}", "Ошибка");
            }
            catch
            {
                // Fallback if notification system isn't available
            }
        }
        finally
        {
            IsProcessing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteCancel))]
    private void Cancel()
    {
        _cancellationTokenSource?.Cancel();
    }

    private bool CanExecuteParseFiles()
    {
        return !IsProcessing;
    }

    private bool CanExecuteCancel()
    {
        return IsProcessing;
    }

    [RelayCommand(CanExecute = nameof(CanExecuteReplaceFiles))]
    public async Task ReplaceAllTagsWithMeshesAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            IsProcessing = true;
            ProgressMessage = "Начинаем замену тегов...";
            IsProgressIndeterminate = true;
            ProcessingStats = "";

            // Create progress reporter (same pattern as ParseFilesAsync)
            var progress = new Progress<string>(message =>
            {
                ProgressMessage = message;

                // Parse progress from your service messages
                if (message.Contains("Processed") && message.Contains("/"))
                    try
                    {
                        // Extract "Processed 5/100 files (Replaced: 3)" format
                        var parts = message.Split('/');
                        if (parts.Length >= 2)
                        {
                            var processed = int.Parse(parts[0].Split(' ').Last());
                            var totalPart = parts[1].Split(' ')[0];
                            var total = int.Parse(totalPart);

                            IsProgressIndeterminate = false;
                            ProgressValue = processed;
                            ProgressMaximum = total;

                            // Extract replacement stats if available
                            if (message.Contains("Replaced:"))
                            {
                                var replacedStart = message.IndexOf("Replaced: ") + 10;
                                var replacedEnd = message.IndexOf(")", replacedStart);
                                if (replacedEnd > replacedStart)
                                {
                                    var replaced = message.Substring(replacedStart, replacedEnd - replacedStart);
                                    ProcessingStats = $"Заменено файлов: {replaced}";
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Parsing failed, just show message as-is
                    }
            });

            await _keyWordConversionService.ReplaceEveryTagWithMesh(
                _repositoryManager.PathToLocalization,
                _cancellationTokenSource.Token,
                progress
            );

            // Final success message
            ProgressMessage = "Замена тегов завершена успешно!";
            IsProgressIndeterminate = false;
            ProgressValue = ProgressMaximum;
            
            await Task.Delay(TimeSpan.FromSeconds(10), _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            ProgressMessage = "Операция отменена пользователем";
            ProcessingStats = "";
        }
        catch (Exception ex)
        {
            ProgressMessage = $"Ошибка: {ex.Message}";
            ProcessingStats = "";

            try
            {
                _ = App.Current.ShowErrorNotificationAsync($"Ошибка при замене тегов: {ex.Message}", "Ошибка");
            }
            catch
            {
                // Fallback if notification system isn't available
            }
        }
        finally
        {
            IsProcessing = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private bool CanExecuteReplaceFiles()
    {
        return !IsProcessing;
    }

    public async void ReplaceAllTagsWithMeshes()
    {
        await ReplaceAllTagsWithMeshesAsync();
    }

}