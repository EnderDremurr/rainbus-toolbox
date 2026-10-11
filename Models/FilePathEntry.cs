using CommunityToolkit.Mvvm.ComponentModel;

namespace RainbusToolbox.Services.ExternalServices;

public partial class FilePathEntry : ObservableObject
{
    [ObservableProperty] public string _filePath = string.Empty;
}