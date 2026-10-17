using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using Newtonsoft.Json;
using RainbusToolbox.Models;
using RainbusToolbox.Services;
using RainbusToolbox.Services.RepositoryServices;

namespace RainbusToolbox.ViewModels.Misc;

public partial class RegexEditorViewModel : ObservableObject
{
    private const int PageSize = 10;
    private readonly DialogService _dialogService;
    private readonly MassReplacementService _massReplacementService;
    private readonly string _pathToRegexJson;

    private CancellationTokenSource? _cancellationTokenSource;


    [ObservableProperty] protected string _navigationCountText = "";
    [ObservableProperty] protected string _navigationText = "";


    public RegexEditorViewModel(LocalizationManager localizationManager, MassReplacementService massReplacementService,
        DialogService dialogService)
    {
        _pathToRegexJson = localizationManager.PathToRegexJson;
        _massReplacementService = massReplacementService;
        _dialogService = dialogService;

        if (!File.Exists(_pathToRegexJson)) return;

        Entries = JsonConvert.DeserializeObject<List<ReplacementEntry>>(File.ReadAllText(_pathToRegexJson)) ?? [];
        OnIndexChanged();
    }

    public int ItemCount =>
        Math.Max(1, (int)Math.Ceiling(Entries.Count / (double)PageSize));

    public int CurrentIndex { get; protected set; }

    public List<ReplacementEntry> Entries { get; } = [];

    public ObservableCollection<ReplacementEntry> CurrentPageEntries { get; } = [];

    [RelayCommand]
    public void AddEntry()
    {
        Entries.Add(new ReplacementEntry());
        CurrentIndex = ItemCount - 1;
        OnIndexChanged();
    }

    [RelayCommand]
    public void RemoveEntry(ReplacementEntry entry)
    {
        Entries.Remove(entry);
        OnIndexChanged();
    }

    [RelayCommand]
    public void AddFilePath(ReplacementEntry entry)
    {
        entry.FileWhiteList.Add(new FilePathEntry());
        OnIndexChanged();
    }

    [RelayCommand]
    public void RemoveFilePath(FilePathEntry filePath)
    {
        foreach (var entry in Entries)
            entry.FileWhiteList.Remove(filePath);
        OnIndexChanged();
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

            await _dialogService.ShowPopupAsync("Готово!", $"Замена \"{entry.Target}\" завершена.");
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

            await _dialogService.ShowPopupAsync("Готово!", "Все замены завершены.");
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

    public void StepIndex(int step)
    {
        if (Entries.Count == 0)
            return;

        var maxIndex = ItemCount - 1;

        var tempIndex = CurrentIndex + step;
        if (tempIndex >= maxIndex)
            tempIndex = maxIndex;
        if (tempIndex < 0)
            tempIndex = 0;
        CurrentIndex = tempIndex;
        OnIndexChanged();
    }

    public void OnIndexChanged()
    {
        CurrentPageEntries.Clear();
        var start = CurrentIndex * PageSize;
        CurrentPageEntries.AddRange(Entries.Skip(start).Take(PageSize));
        UpdateNavigation();
    }

    protected void UpdateNavigation()
    {
        NavigationText = $"{CurrentIndex + 1}";
        NavigationCountText = $"{ItemCount}";
    }
}