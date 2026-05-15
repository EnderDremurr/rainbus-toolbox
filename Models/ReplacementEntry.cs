using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.Models;

public partial class ReplacementEntry : ObservableObject
{
    [ObservableProperty] private bool _isRegex;
    [ObservableProperty] private bool _matchCase;
    [ObservableProperty] private bool _matchWholeWord;
    [ObservableProperty] private bool _preserveCase;
    [ObservableProperty] private string _replacement = string.Empty;
    [ObservableProperty] private string _target = string.Empty;

    public ObservableCollection<FilePathEntry> FileWhiteList { get; set; } = new();
}