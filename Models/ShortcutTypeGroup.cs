using System.Collections.Generic;
using RainbusToolbox.Services.ExternalServices.Data;

namespace RainbusToolbox.Services.ExternalServices;

public class ShortcutTypeGroup
{
    public string Name { get; set; } = "";
    public IEnumerable<ShortcutFolderGroup> Groups { get; set; } = [];
}