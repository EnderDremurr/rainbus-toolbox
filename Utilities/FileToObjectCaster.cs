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
        typeof(BuffsFile),
        typeof(AbDlgFile),
        typeof(SkillsEgoFile),
        typeof(SkillsFile),
        typeof(BattleHintsFile),
        typeof(StoryDataFile),
        typeof(PanicInfoFile),
        typeof(PassivesFile),
        typeof(EGOGiftFile),
        typeof(BattleAnnouncerFile),
        typeof(PersonalityVoiceFile),
        typeof(EGOVoiceFile),
        typeof(AbnormalityGuideFile),
        typeof(UnidentifiedFile)
    ];  
    
    public static Type? GetType(string pathToFile)
    {
        var type = LocalizationFileTypes.FirstOrDefault(t => 
            t.GetCustomAttribute<FilePatternAttribute>()?.Pattern is { } pattern && 
            MatchesPattern(pathToFile, pattern));

        return type;
    }
    
    private static bool MatchesPattern(string filePath, string pattern)
    {
        // Normalize path separators for cross-platform compatibility
        filePath = filePath.Replace('\\', '/');
        pattern = pattern.Replace('\\', '/');
        
        // Handle folder patterns like "Story/*"
        if (pattern.EndsWith("/*"))
        {
            string folderPattern = pattern.Substring(0, pattern.Length - 2);
            
            // Check if the file is in the specified folder
            var fileDirectory = Path.GetDirectoryName(filePath)?.Replace('\\', '/');
            
            if (folderPattern.Contains("*"))
            {
                // Handle patterns like "*/Story/*" or "Data/*/Story/*"
                return MatchesFolderPattern(fileDirectory, folderPattern);
            }
            else
            {
                // Simple folder check like "Story/*"
                return fileDirectory != null && 
                       (fileDirectory.Equals(folderPattern, StringComparison.OrdinalIgnoreCase) ||
                        fileDirectory.EndsWith("/" + folderPattern, StringComparison.OrdinalIgnoreCase));
            }
        }
        
        // For filename patterns, extract just the filename
        var fileName = Path.GetFileName(filePath);
        
        // Handle exact matches (no wildcards)
        if (!pattern.Contains("*"))
        {
            return string.Equals(fileName, pattern, StringComparison.OrdinalIgnoreCase);
        }

        // Convert wildcard pattern to regex-like matching for filenames
        if (pattern.StartsWith("*") && pattern.EndsWith("*") && !pattern.EndsWith("/*"))
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
        else if (pattern.EndsWith("*") && !pattern.EndsWith("/*"))
        {
            // text* - starts with
            string prefix = pattern.Substring(0, pattern.Length - 1);
            return fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
    
    private static bool MatchesFolderPattern(string? actualPath, string folderPattern)
    {
        if (actualPath == null) return false;
        
        // Split paths into segments
        var actualSegments = actualPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var patternSegments = folderPattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        // Simple wildcard matching for folder patterns
        int actualIndex = 0;
        int patternIndex = 0;
        
        while (patternIndex < patternSegments.Length && actualIndex < actualSegments.Length)
        {
            string patternSegment = patternSegments[patternIndex];
            
            if (patternSegment == "*")
            {
                // Wildcard matches any single segment
                actualIndex++;
                patternIndex++;
            }
            else if (patternSegment.Contains("*"))
            {
                // Handle wildcards within segment names
                if (MatchesSegmentPattern(actualSegments[actualIndex], patternSegment))
                {
                    actualIndex++;
                    patternIndex++;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // Exact segment match
                if (actualSegments[actualIndex].Equals(patternSegment, StringComparison.OrdinalIgnoreCase))
                {
                    actualIndex++;
                    patternIndex++;
                }
                else
                {
                    return false;
                }
            }
        }
        
        // Check if we matched all pattern segments
        return patternIndex == patternSegments.Length;
    }
    
    private static bool MatchesSegmentPattern(string segment, string pattern)
    {
        if (pattern == "*") return true;
        
        if (pattern.StartsWith("*") && pattern.EndsWith("*"))
        {
            string searchText = pattern.Substring(1, pattern.Length - 2);
            return segment.Contains(searchText, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.StartsWith("*"))
        {
            string suffix = pattern.Substring(1);
            return segment.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
        }
        else if (pattern.EndsWith("*"))
        {
            string prefix = pattern.Substring(0, pattern.Length - 1);
            return segment.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }
        
        return segment.Equals(pattern, StringComparison.OrdinalIgnoreCase);
    }
}