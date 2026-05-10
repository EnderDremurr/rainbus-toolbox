using System.Collections.Generic;
using System.Text.RegularExpressions;
using RainbusToolbox.Services.ExternalServices;

namespace RainbusToolbox.Utilities;

public class SpellcheckEngine(SpellCheckerService service)
{
    private static readonly Regex WordRegex =
        new(@"\p{L}+");

    private static readonly Regex TagsRegex =
        new(@"<[^>]+>|\[[^\]]+\]");

    public List<(int start, int length)> GetErrors(string text)
    {
        var errors = new List<(int, int)>();

        var cleanText = TagsRegex.Replace(text, match =>
            new string(' ', match.Length));

        foreach (var (start, length, word) in GetWords(cleanText))
            if (!service.Check(word))
                errors.Add((start, length));

        return errors;
    }

    public IEnumerable<(int start, int length, string word)> GetWords(string text)
    {
        foreach (Match m in WordRegex.Matches(text))
        {
            var raw = m.Value;
            var cleaned = raw.Trim(' ', '.', ',', '!', '?', ':', ';', '"', '\'', '(', ')');

            yield return (m.Index, m.Length, cleaned);
        }
    }
}