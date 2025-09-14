using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RainbusToolbox.Utilities.Data;
using System.IO;

namespace RainbusToolbox.Models.Data;

public static class FileToObjectCaster
{
    public static List<Type> LocalizationFileTypes { get; } =
    [
        typeof(DialogueFile),
        typeof(SkillsFile),
        typeof(BattleHintsFile)
    ];  
    public static Type? GetType(string pathToFile)
    {
        var fileName = Path.GetFileName(pathToFile);
        var type = LocalizationFileTypes.FirstOrDefault(t => 
            t.GetCustomAttribute<FilePatternAttribute>()?.Pattern is { } pattern && 
            MatchesPattern(fileName, pattern));

        return type;
    }
    
    private static bool MatchesPattern(string fileName, string pattern)
    {
        // Handle exact matches (no wildcards)
        if (!pattern.Contains("*"))
        {
            return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        // Convert wildcard pattern to regex-like matching
        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        {
            // *text* - contains
            string searchText = pattern.Substring(1, pattern.Length - 2);
            return fileName.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.StartsWith("*"))
        {
            // *text - ends with
            string suffix = pattern.Substring(1);
            return fileName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.EndsWith("*"))
        {
            // text* - starts with
            string prefix = pattern.Substring(0, pattern.Length - 1);
            return fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // Handle more complex patterns if needed
        return false;
    }
}