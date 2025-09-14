using System;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class FilePatternAttribute : Attribute
{
    public string Pattern { get; }

    public FilePatternAttribute(string pattern)
    {
        Pattern = pattern;
    }
}