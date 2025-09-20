using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;
using RainbusToolbox.Utilities.Data;
using RainbusToolbox.ViewModels;

public partial class BattleHintsEditorViewModel : TranslationEditorViewModel<BattleHintsFile, GenericIdContent>
{
    [ObservableProperty] private string _newHintText;

    // Observable collection bound to the UI
    public ObservableCollection<GenericIdContent> ObservableDataList { get; } = new();

    public override void LoadEditableFile(BattleHintsFile file)
    {
        base.LoadEditableFile(file);

        // Clear and populate the observable collection from the file's list
        ObservableDataList.Clear();
        foreach (var item in EditableFile?.DataList)
            ObservableDataList.Add(item);

        // Subscribe to changes in the observable collection if needed
        ObservableDataList.CollectionChanged += (s, e) =>
        {
            // Keep the underlying list in sync for serialization
            EditableFile.DataList = ObservableDataList.ToList();
        };
    }

    [RelayCommand]
    private void AddHint()
    {
        if (string.IsNullOrWhiteSpace(NewHintText)) return;

        var nextId = ObservableDataList.Any() 
            ? ObservableDataList.Max(h => int.Parse(h.Id)) + 1 
            : 1;

        var newHint = new GenericIdContent
        {
            Id = nextId.ToString(),
            Content = NewHintText
        };

        ObservableDataList.Add(newHint);
        NewHintText = string.Empty;
    }

    [RelayCommand]
    private void DeleteHint(string id)
    {
        var hint = ObservableDataList.FirstOrDefault(h => int.Parse(h.Id) == int.Parse(id));
        if (hint != null)
            ObservableDataList.Remove(hint);
    }

    public void UpdateHint(int id, string newContent)
    {
        var hint = ObservableDataList.FirstOrDefault(h => int.Parse(h.Id) == id);
        if (hint != null)
            hint.Content = newContent;
    }
}
