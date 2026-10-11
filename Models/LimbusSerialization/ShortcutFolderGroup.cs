using System.Collections.Generic;

namespace RainbusToolbox.Models.LimbusSerialization;

public class ShortcutFolderGroup
{
    public string Name { get; set; } = "";
    public IEnumerable<FileShortcut> Shortcuts { get; set; } = [];
}