using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RainbusTools.Converters.Managers;
using Avalonia.Threading;

namespace RainbusTools.ViewModels;

public partial class BattleHintsTabViewModel : ObservableObject
{
    private readonly RepositoryManager _repositoryManager;
    
    public BattleHintsTabViewModel(RepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        LoadHints(); // Initial load
    }

    [ObservableProperty]
    private string _battleHintsText;

    [ObservableProperty]
    private string _newHintText;

    /// <summary>
    /// Loads BattleHints from the repository and updates BattleHintsText.
    /// </summary>
    private void LoadHints()
    {
        // Ensure we are on the UI thread
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var battleHints = _repositoryManager.GetBattleHints();

            if (battleHints?.DataList == null)
            {
                BattleHintsText = string.Empty;
                return;
            }

            BattleHintsText = string.Join("\n", battleHints.DataList.Select(h => $"{h.Id} : {h.Content}"));
            HintsUpdated?.Invoke();
        });
    }
    
    public event Action? HintsUpdated;


    [RelayCommand]
    private void AddHint()
    {
        if (string.IsNullOrWhiteSpace(NewHintText))
            return;

        // Add hint to JSON
        _repositoryManager.AddHint(NewHintText);

        // Refresh BattleHintsText from latest file
        LoadHints();

        // Clear input box
        NewHintText = string.Empty;
    }
}