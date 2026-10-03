using System.Windows.Input;
using YamlDotNet.Serialization;

namespace RainbusToolbox.Models;

public class FileShortcut
{
    public required string Alias { get; set; }

    public string? PathRelativeToRoot { get; set; }

    [YamlIgnore]
    public string? FullPath { get; set; } = null;

    public string Desc { get; set; } = "-";

    [YamlIgnore]
    public bool DoesExist { get; set; }

    [YamlIgnore]
    public ICommand? OpenCommand { get; set; } = null;

    public required string Type { get; set; } = "Разное";
    public required string Group { get; set; } = "Прочее";
}