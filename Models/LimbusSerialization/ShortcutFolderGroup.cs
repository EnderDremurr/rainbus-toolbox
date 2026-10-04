using System.Collections.Generic;
using RainbusToolbox.Models;

namespace RainbusToolbox.Utilities.Data;

public class ShortcutFolderGroup
{
    public string Name { get; set; } = "";
    public IEnumerable<FileShortcut> Shortcuts { get; set; } = [];
}