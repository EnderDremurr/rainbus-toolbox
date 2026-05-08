using System.Collections.Generic;

namespace RainbusToolbox.Models;

public class ReplacementEntry
{
    public string Target { get; set; } = string.Empty;
    public string Replacement { get; set; } = string.Empty;

    public bool IsRegex { get; set; } = false;

    public bool MatchCase { get; set; } = false;
    public bool MatchWholeWord { get; set; } = false;

    public bool PreserveCase { get; set; } = false;

    public List<string> FileWhiteList { get; set; } = new();
}