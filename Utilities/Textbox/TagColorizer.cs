using System.Text.RegularExpressions;
using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

public class TagColorizer : DocumentColorizingTransformer
{
    private static readonly Regex YellowTags =
        new(@"\[[^\[\]:]+\]");

    private static readonly Regex PurpleTags =
        new(@"\[[^\]]+:\*[^\]]+\*\]");

    private static readonly Regex GrayTags =
        new(@"\<(?!ruby=|/ruby)[^>]+\>");

    private static readonly Regex RubyOpenTag =
        new(@"(\<ruby=)([^>]+)(\>)");

    private static readonly Regex RubyCloseTag =
        new(@"\</ruby\>");

    protected override void ColorizeLine(DocumentLine line)
    {
        var text = CurrentContext.Document.GetText(line);

        Apply(text, line, PurpleTags, Brushes.DarkGoldenrod);
        Apply(text, line, YellowTags, Brushes.LawnGreen);
        Apply(text, line, GrayTags, Brushes.Gray);

        ApplyRubyTags(text, line);
    }

    private void Apply(
        string text,
        DocumentLine line,
        Regex regex,
        IBrush brush)
    {
        foreach (Match match in regex.Matches(text)) Paint(line, match.Index, match.Length, brush);
    }

    private void ApplyRubyTags(string text, DocumentLine line)
    {
        foreach (Match match in RubyOpenTag.Matches(text))
        {
            Paint(
                line,
                match.Groups[1].Index,
                match.Groups[1].Length,
                Brushes.Red);

            Paint(
                line,
                match.Groups[3].Index,
                match.Groups[3].Length,
                Brushes.Red);
        }

        foreach (Match match in RubyCloseTag.Matches(text)) Paint(line, match.Index, match.Length, Brushes.Red);
    }

    private void Paint(
        DocumentLine line,
        int index,
        int length,
        IBrush brush)
    {
        ChangeLinePart(
            line.Offset + index,
            line.Offset + index + length,
            element => { element.TextRunProperties.SetForegroundBrush(brush); });
    }
}