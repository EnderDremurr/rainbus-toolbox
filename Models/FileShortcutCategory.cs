namespace RainbusToolbox.Models;

public class FileShortcutCategory
{
    public required string Group { get; set; }
    public required string Pattern { get; set; }
    public string Subfolder { get; set; } = "";
    public required string Type { get; set; }
}