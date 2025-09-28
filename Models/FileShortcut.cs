using System.Windows.Input;

namespace RainbusToolbox.Models;

public class FileShortcut
{
    public string Alias { get; set; }
    public string FullPath { get; set; }
    public string Desc { get; set; }
    public bool DoesExist { get; set; }
    public ICommand OpenCommand { get; set; } 
}