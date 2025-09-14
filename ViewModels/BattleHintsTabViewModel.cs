using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;
using RainbusToolbox.Models.Managers;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.ViewModels;

public partial class BattleHintsTabViewModel : ObservableObject
{
    private readonly RepositoryManager _repositoryManager;

    public BattleHintsTabViewModel(RepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        Hints = new ObservableCollection<EditableGenericIdContent>();
        LoadHints();
    }
    
    private BattleHintTypes _selectedType;
    public BattleHintTypes SelectedType
    {
        get => _selectedType;
        set
        {
            if (_selectedType != value)
            {
                _selectedType = value;
                OnPropertyChanged(nameof(SelectedType));

                // Call your method when value changes
                LoadHints();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    public ObservableCollection<EditableGenericIdContent> Hints { get; }

    [ObservableProperty]
    private string _newHintText;

    /// <summary>
    /// Loads BattleHints from the repository and updates the observable collection.
    /// </summary>
    private void LoadHints()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            var battleHints = _repositoryManager.GetBattleHints(_selectedType);

            Hints.Clear();

            if (battleHints?.DataList == null)
                return;

            foreach (var hint in battleHints.DataList)
            {
                Hints.Add(new EditableGenericIdContent(hint));
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

        _repositoryManager.AddHint(NewHintText, _selectedType);

        LoadHints();

        NewHintText = string.Empty;
    }

    [RelayCommand]
    private void DeleteHint(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        // Delete from repository
        _repositoryManager.DeleteHintAtId(int.Parse(id), _selectedType);

        // Remove from observable collection
        var toRemove = Hints.FirstOrDefault(h => h.Id == id);
        if (toRemove != null)
            Hints.Remove(toRemove);
    }

    [RelayCommand]
    private void EditHint(EditableGenericIdContent hint)
    {
        if (hint == null)
            return;

        // Cancel any other editing hints
        foreach (var h in Hints.Where(x => x != hint && x.IsEditing))
        {
            h.CancelEdit();
        }

        // Start editing this hint
        hint.StartEdit();
    }

    [RelayCommand]
    private void SaveHint(EditableGenericIdContent hint)
    {
        if (hint == null || string.IsNullOrWhiteSpace(hint.EditContent))
            return;

        // Update in repository
        _repositoryManager.UpdateHint(int.Parse(hint.Id), hint.EditContent, _selectedType);

        // Update the local object
        hint.SaveEdit();
    }
}