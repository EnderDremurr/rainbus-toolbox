using System.Collections.Generic;
using RainbusToolbox.Services.ExternalServices;

namespace RainbusToolbox.Services.ExternalServices.Data;

public class ShortcutFolderGroup
{
    public string Name { get; set; } = "";
    public IEnumerable<FileShortcut> Shortcuts { get; set; } = [];
}