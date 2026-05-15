using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.Models;

public partial class FilePathEntry : ObservableObject
{
    [ObservableProperty] public string _filePath = string.Empty;
}