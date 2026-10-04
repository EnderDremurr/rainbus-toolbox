using System.Collections.Generic;
using RainbusToolbox.Utilities.Data;

namespace RainbusToolbox.Models;

public class ShortcutTypeGroup
{
    public string Name { get; set; } = "";
    public IEnumerable<ShortcutFolderGroup> Groups { get; set; } = [];
}