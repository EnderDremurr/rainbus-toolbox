using Avalonia.Media;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;

namespace RainbusToolbox.Utilities;

public class SpellcheckColorizer(SpellcheckEngine engine) : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        var text = CurrentContext.Document.GetText(line);
        var errors = engine.GetErrors(text);

        foreach (var (start, length) in errors)
            ChangeLinePart(
                line.Offset + start,
                line.Offset + start + length,
                x =>
                {
                    x.TextRunProperties.SetTextDecorations(
                    [
                        new TextDecoration
                        {
                            Location = TextDecorationLocation.Underline,
                            Stroke = Brushes.Red,
                            StrokeThickness = 2
                        }
                    ]);
                });
    }
}