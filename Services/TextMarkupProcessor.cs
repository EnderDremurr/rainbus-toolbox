using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace RainbusToolbox.Services;

public class TextMarkupProcessor
{
    public static List<Inline> ConvertRawToRich(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return new List<Inline>();

        try
        {
            // Step 1: Process bracket tags [Key:Value] and [SimpleTag]
            var processed = ProcessBracketTags(raw);

            // Step 2: Convert to Inline objects
            return ParseToInlines(processed);
        }
        catch (Exception ex)
        {
            // Return error message as plain text on parsing failure
            return new List<Inline>
            {
                new Run($"Parse Error: {ex.Message}") { Foreground = Brushes.Red }
            };
        }
    }

    private static string ProcessBracketTags(string text)
    {
        // Handle [Key:Value] format first
        var keyValuePattern = @"\[([^:]+):([^\]]+)\]";
        text = Regex.Replace(text, keyValuePattern, match =>
        {
            var key = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value.Trim();
            return FormatByKey(key, value);
        });

        // Handle simple [Tag] format
        var simpleTagPattern = @"\[([^\]:]+)\]";
        text = Regex.Replace(text, simpleTagPattern, match =>
        {
            var tag = match.Groups[1].Value.Trim();
            return FormatSimpleTag(tag);
        });

        return text;
    }

    private static string FormatByKey(string key, string value)
    {
        // Define formatting rules based on key type
        return key.ToLower() switch
        {
            "skillname" => $"<color=#4A9EFF><b>{value}</b></color>",
            "damage" => $"<color=#FF4444><b>{value}</b></color>",
            "heal" => $"<color=#44FF44><b>{value}</b></color>",
            "rarity" => $"<color=#FFD700><i>{value}</i></color>",
            "effect" => $"<color=#88CCFF>{value}</color>",
            "number" => $"<color=#FFAA44><b>{value}</b></color>",
            _ => $"<color=#CCCCCC>{value}</color>" // Default formatting
        };
    }

    private static string FormatSimpleTag(string tag)
    {
        // Format simple tags as styled headers
        return tag.ToLower() switch
        {
            "endskillhead" => "<color=#FF8844><b>[END SKILL]</b></color>",
            "onsucceedattack" => "<color=#88FF44><b>[ON HIT]</b></color>",
            "endskill" => "<color=#4488FF><b>[PASSIVE]</b></color>",
            "tabexplain" => "", // Remove this tag
            _ => $"<color=#AAAAAA><b>[{tag.ToUpper()}]</b></color>"
        };
    }

    private static List<Inline> ParseToInlines(string text)
    {
        var inlines = new List<Inline>();
        var position = 0;

        // Track current formatting state
        var formatStack = new Stack<TextFormat>();
        var currentFormat = new TextFormat();
        var currentText = "";

        while (position < text.Length)
            if (text[position] == '[' && HasBracketTag(text, position))
            {
                // Handle [Key] and [Key:`Value`] tags
                if (!string.IsNullOrEmpty(currentText))
                {
                    inlines.Add(CreateRun(currentText, currentFormat));
                    currentText = "";
                }

                var bracketInfo = ExtractBracketTag(text, position);
                position = bracketInfo.EndPosition;

                // TODO: Implement proper bracket tag processing later
                // For now, just add as purple colored text
                inlines.Add(new Run(bracketInfo.Tag) { Foreground = new SolidColorBrush(Color.Parse("#800080")) });
            }
            else if (text[position] == '<' && HasAngleBracketTag(text, position))
            {
                // Add any accumulated text before processing tag
                if (!string.IsNullOrEmpty(currentText))
                {
                    inlines.Add(CreateRun(currentText, currentFormat));
                    currentText = "";
                }

                // Find and process the tag
                var tagInfo = ExtractAngleBracketTag(text, position);
                position = tagInfo.EndPosition;

                ProcessAngleBracketTag(tagInfo.Tag, formatStack, ref currentFormat, inlines);
            }
            else if (text[position] == '\n' ||
                     (text[position] == '\r' && position + 1 < text.Length && text[position + 1] == '\n'))
            {
                // Handle newlines
                if (!string.IsNullOrEmpty(currentText))
                {
                    inlines.Add(CreateRun(currentText, currentFormat));
                    currentText = "";
                }

                inlines.Add(new LineBreak());

                // Skip \r\n sequence
                if (text[position] == '\r')
                    position += 2;
                else
                    position++;
            }
            else
            {
                currentText += text[position];
                position++;
            }

        // Add any remaining text
        if (!string.IsNullOrEmpty(currentText)) inlines.Add(CreateRun(currentText, currentFormat));

        return inlines;
    }

    private static bool HasBracketTag(string text, int position)
    {
        var tagEnd = text.IndexOf(']', position);
        return tagEnd != -1 && tagEnd > position + 1;
    }

    private static (string Tag, int EndPosition) ExtractBracketTag(string text, int startPos)
    {
        var tagEnd = text.IndexOf(']', startPos);
        if (tagEnd == -1)
            return ("", startPos + 1);

        var tag = text.Substring(startPos, tagEnd - startPos + 1); // Include brackets
        return (tag, tagEnd + 1);
    }

    private static bool HasAngleBracketTag(string text, int position)
    {
        var tagEnd = text.IndexOf('>', position);
        return tagEnd != -1 && tagEnd > position + 1;
    }

    private static (string Tag, int EndPosition) ExtractAngleBracketTag(string text, int startPos)
    {
        var tagEnd = text.IndexOf('>', startPos);
        if (tagEnd == -1)
            return ("", startPos + 1);

        var tag = text.Substring(startPos + 1, tagEnd - startPos - 1);
        return (tag, tagEnd + 1);
    }

    private static void ProcessAngleBracketTag(string tag, Stack<TextFormat> formatStack,
        ref TextFormat currentFormat, List<Inline> inlines)
    {
        if (string.IsNullOrEmpty(tag)) return;

        if (tag.StartsWith("/"))
        {
            // Closing tag - restore previous format
            if (formatStack.Count > 0) currentFormat = formatStack.Pop();
        }
        else if (tag.StartsWith("color="))
        {
            // Color tag - push current format and apply color
            formatStack.Push(currentFormat.Clone());
            var colorValue = tag.Substring(6);
            if (TryParseColor(colorValue, out var color)) currentFormat.Color = color;
        }
        else if (tag.StartsWith("sprite name="))
        {
            // Extract sprite name from quotes
            var match = Regex.Match(tag, @"sprite name=""([^""]+)""");
            if (match.Success)
            {
                var spriteName = match.Groups[1].Value;
                var spriteInline = CreateSpriteInline(spriteName);
                if (spriteInline != null) inlines.Add(spriteInline);
            }
        }
        else if (tag.StartsWith("link="))
        {
            // TODO: Implement proper link functionality later (tooltips, click handlers, etc.)
            // For now, just remove the link tags and show content as normal text
            // The content between <link="..."> and </link> will be processed normally
            // but the link tags themselves are ignored
        }
        else if (tag == "u")
        {
            formatStack.Push(currentFormat.Clone());
            currentFormat.IsUnderlined = true;
        }
        else if (tag.StartsWith("style="))
        {
            // Extract style name from quotes
            var match = Regex.Match(tag, @"style=""([^""]+)""");
            if (match.Success)
            {
                var styleName = match.Groups[1].Value;
                formatStack.Push(currentFormat.Clone());
                ApplyStyle(styleName, ref currentFormat);
            }
        }
    }

    private static void ApplyStyle(string styleName, ref TextFormat format)
    {
        // Define available styles
        switch (styleName.ToLower())
        {
            case "highlight":
                format.Color = Color.Parse("#FFFF00"); // Yellow
                format.IsUnderlined = true;
                break;
            // TODO: Add more styles as needed
        }
    }

    private static Run CreateRun(string text, TextFormat format)
    {
        var run = new Run(text);

        if (format.Color.HasValue)
            run.Foreground = new SolidColorBrush(format.Color.Value);

        if (format.IsBold)
            run.FontWeight = FontWeight.Bold;

        if (format.IsItalic)
            run.FontStyle = FontStyle.Italic;

        if (format.IsUnderlined)
            run.TextDecorations = TextDecorations.Underline;

        // TODO: Link functionality would need to be handled at TextBlock level
        // Individual Run elements don't support cursor or click events
        // You would need to implement this in the parent TextBlock that contains the inlines

        return run;
    }

    private static InlineUIContainer? CreateSpriteInline(string spriteName)
    {
        try
        {
            // Try to load the specific sprite first
            var bitmap = LoadSprite(spriteName);
            if (bitmap == null)
                // If not found, try placeholder
                bitmap = LoadSprite("Placeholder");

            if (bitmap != null)
            {
                var image = new Image
                {
                    Source = bitmap,
                    Width = 18,
                    Height = 18,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(1, 0)
                };

                return new InlineUIContainer(image);
            }
        }
        catch
        {
            // Ignore sprite loading errors and fall through to fallback
        }

        // Final fallback: return text representation
        return new InlineUIContainer(new TextBlock
        {
            Text = $"[{spriteName}]",
            Foreground = Brushes.Orange,
            FontSize = 12
        });
    }

    private static Bitmap? LoadSprite(string spriteName)
    {
        try
        {
            // Try to load from /Assets/Icons/
            var path = $"avares://YourApp/Assets/Icons/{spriteName}.png";
            var uri = new Uri(path);
            return new Bitmap(AssetLoader.Open(uri));
        }
        catch
        {
            // Sprite loading failed
            return null;
        }
    }

    private static bool TryParseColor(string colorStr, out Color color)
    {
        color = default;

        if (string.IsNullOrEmpty(colorStr))
            return false;

        // Handle hex colors
        if (colorStr.StartsWith("#"))
            try
            {
                color = Color.Parse(colorStr);
                return true;
            }
            catch
            {
                return false;
            }

        // Handle named colors
        try
        {
            color = Color.Parse(colorStr);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Helper class to track formatting state
    private class TextFormat
    {
        public Color? Color { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderlined { get; set; }
        public bool IsLink { get; set; }
        public string LinkTarget { get; set; } = "";

        public TextFormat Clone()
        {
            return new TextFormat
            {
                Color = Color,
                IsBold = IsBold,
                IsItalic = IsItalic,
                IsUnderlined = IsUnderlined,
                IsLink = IsLink,
                LinkTarget = LinkTarget
            };
        }
    }
}