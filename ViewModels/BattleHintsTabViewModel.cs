using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusTools.Converters.Managers;
using RainbusTools.Utilities.Data;
using Avalonia.Threading;

namespace RainbusTools.ViewModels;

public partial class BattleHintsTabViewModel : ObservableObject
{
    private readonly RepositoryManager _repositoryManager;

    public BattleHintsTabViewModel(RepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        Hints = new ObservableCollection<BattleHint>();
        LoadHints();
    }

    public ObservableCollection<BattleHint> Hints { get; }

    [ObservableProperty]
    private string _newHintText;

    /// <summary>
    /// Loads BattleHints from the repository and updates the observable collection.
    /// </summary>
    private void LoadHints()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var battleHints = _repositoryManager.GetBattleHints();

            Hints.Clear();

            if (battleHints?.DataList == null)
                return;

            foreach (var hint in battleHints.DataList)
            {
                Hints.Add(hint);
            }

            HintsUpdated?.Invoke();
        });
    }

    public event Action? HintsUpdated;

    [RelayCommand]
    private void AddHint()
    {
        if (string.IsNullOrWhiteSpace(NewHintText))
            return;

        _repositoryManager.AddHint(NewHintText);

        LoadHints();

        NewHintText = string.Empty;
    }

    [RelayCommand]
    private void DeleteHint(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        // Delete from repository
        _repositoryManager.DeleteHintAtId(int.Parse(id));

        // Remove from observable collection
        var toRemove = Hints.FirstOrDefault(h => h.Id == id);
        if (toRemove != null)
            Hints.Remove(toRemove);
    }
}