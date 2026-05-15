using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;
using RainbusToolbox.Models;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Services.RepositoryServices;
using RainbusToolbox.Views.Misc;

namespace RainbusToolbox.ViewModels;

public partial class RegexEditorViewModel : ObservableObject
{
    private readonly MassReplacementService _massReplacementService;
    private readonly string _pathToRegexJson;

    private CancellationTokenSource? _cancellationTokenSource;


    public RegexEditorViewModel(RepositoryManager repositoryManager, MassReplacementService massReplacementService)
    {
        _pathToRegexJson = repositoryManager.PathToRegexJson;
        _massReplacementService = massReplacementService;

        if (!File.Exists(_pathToRegexJson)) return;

        var json = JsonConvert.DeserializeObject<List<ReplacementEntry>>(File.ReadAllText(_pathToRegexJson));
        if (json is null) return;
        foreach (var entry in json) Entries.Add(entry);
    }

    public ObservableCollection<ReplacementEntry> Entries { get; } = new();

    [RelayCommand]
    public void AddEntry()
    {
        Entries.Add(new ReplacementEntry());
    }

    [RelayCommand]
    public void RemoveEntry(ReplacementEntry entry)
    {
        Entries.Remove(entry);
    }

    [RelayCommand]
    public void AddFilePath(ReplacementEntry entry)
    {
        entry.FileWhiteList.Add(new FilePathEntry());
    }

    [RelayCommand]
    public void RemoveFilePath(FilePathEntry filePath)
    {
        foreach (var entry in Entries)
            entry.FileWhiteList.Remove(filePath);
    }

    [RelayCommand]
    public void SaveCurrentJsons()
    {
        File.WriteAllText(_pathToRegexJson, JsonConvert.SerializeObject(Entries, Formatting.Indented));
    }

    [RelayCommand]
    public async Task RunEntryAsync(ReplacementEntry entry)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            LoadingScreenViewModel.StartLoading($"Замена: {entry.Target}...");

            var progress = new Progress<(int Processed, int Total, string Label)>(p =>
            {
                LoadingScreenViewModel.SetProgress(p.Processed, p.Total);
                LoadingScreenViewModel.SetText(p.Label);
            });

            await _massReplacementService.RunOneRegexForAllFilesAsync(
                entry, progress, _cancellationTokenSource.Token);

            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Готово!", $"Замена \"{entry.Target}\" завершена.");
        }
        catch (OperationCanceledException)
        {
            LoadingScreenViewModel.SetText("Операция отменена пользователем");
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex, "Ошибка при замене");
        }
        finally
        {
            LoadingScreenViewModel.FinishLoading();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    [RelayCommand]
    public async Task RunAllEntriesAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        try
        {
            LoadingScreenViewModel.StartLoading("Замена всех правил...");

            var progress = new Progress<(int Processed, int Total, string Label)>(p =>
            {
                LoadingScreenViewModel.SetProgress(p.Processed, p.Total);
                LoadingScreenViewModel.SetText(p.Label);
            });

            await _massReplacementService.RunAllRegexesForAllFilesAsync(
                Entries.ToList(), progress, _cancellationTokenSource.Token);

            var parent = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await PopUpWindow.ShowAsync(parent!, "Готово!", "Все замены завершены.");
        }
        catch (OperationCanceledException)
        {
            LoadingScreenViewModel.SetText("Операция отменена пользователем");
        }
        catch (Exception ex)
        {
            _ = App.Current.HandleNonFatalExceptionAsync(ex, "Ошибка при замене");
        }
        finally
        {
            LoadingScreenViewModel.FinishLoading();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }
}