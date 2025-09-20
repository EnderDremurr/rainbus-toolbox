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
            // Direct parsing without text preprocessing - this prevents corruption
            return ParseToInlines(raw);
        }
        catch (Exception ex)
        {
            // Return error message as plain text on parsing failure
            return new List<Inline>
            {
                new Run($"Parse Error: {ex.Message}") 
                { 
                    Foreground = Brushes.Red,
                    FontFamily = new FontFamily("avares://RainbusToolbox/Assets/Fonts/Pretendard.ttf#Pretendard")
                }
            };
        }
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
        {
            if (text[position] == '[' && HasBracketTag(text, position))
            {
                // Add any accumulated text before processing bracket tag
                if (!string.IsNullOrEmpty(currentText))
                {
                    inlines.Add(CreateRun(currentText, currentFormat));
                    currentText = "";
                }

                var bracketInfo = ExtractBracketTag(text, position);
                position = bracketInfo.EndPosition;

                // Process bracket tag and add as inline
                var bracketInline = ProcessBracketTag(bracketInfo.Tag);
                inlines.Add(bracketInline);
            }
            else if (text[position] == '<' && HasAngleBracketTag(text, position))
            {
                // Add any accumulated text before processing angle bracket tag
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
            else if (text[position] == '\n' || (text[position] == '\r' && position + 1 < text.Length && text[position + 1] == '\n'))
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
        }

        // Add any remaining text
        if (!string.IsNullOrEmpty(currentText)) 
            inlines.Add(CreateRun(currentText, currentFormat));

        return inlines;
    }

    private static Run ProcessBracketTag(string fullTag)
    {
        // Check if it's a key:value format [Key:`Value`]
        var keyValueMatch = Regex.Match(fullTag, @"^\[([^:]+):`([^`]+)`\]$");
        if (keyValueMatch.Success)
        {
            var key = keyValueMatch.Groups[1].Value.Trim();
            var value = keyValueMatch.Groups[2].Value.Trim();
            return CreateFormattedBracketRun(key, value, true);
        }

        // Check if it's a simple tag [Tag]
        var simpleMatch = Regex.Match(fullTag, @"^\[([^\]]+)\]$");
        if (simpleMatch.Success)
        {
            var tag = simpleMatch.Groups[1].Value.Trim();
            return CreateFormattedBracketRun(tag, tag, false);
        }

        // Fallback: return as purple text
        return new Run(fullTag)
        {
            Foreground = new SolidColorBrush(Color.Parse("#800080")),
            FontFamily = new FontFamily("avares://RainbusToolbox/Assets/Fonts/Pretendard.ttf#Pretendard")
        };
    }

    private static Run CreateFormattedBracketRun(string key, string displayText, bool isKeyValue)
    {
        var color = GetBracketTagColor(key.ToLower());
        var formattedText = FormatBracketTag(key.ToLower(), displayText, isKeyValue);

        return new Run(formattedText)
        {
            Foreground = new SolidColorBrush(color),
            FontFamily = new FontFamily("avares://RainbusToolbox/Assets/Fonts/Pretendard.ttf#Pretendard"),
            FontWeight = FontWeight.Bold
        };
    }

    private static Color GetBracketTagColor(string key)
    {
        return key switch
        {
            "cantidentify" => Color.Parse("#FF6B6B"),
            "beforeattack" => Color.Parse("#4ECDC4"),
            "endskill" => Color.Parse("#45B7D1"),
            "onsucceedattack" => Color.Parse("#96CEB4"),
            "tabexplain" => Color.Parse("#FFEAA7"),
            
            
            // Default color for unknown tags
            _ => Color.Parse("#CCCCCC")
        };
    }

    private static string FormatBracketTag(string key, string displayText, bool isKeyValue)
    {
        if (isKeyValue)
        {
            // For [Key:`Value`] format, show the value in a styled way
            return key switch
            {
                _ => displayText
            };
        }
        else
        {
            // For simple [Tag] format, show formatted tag name
            return key switch
            {
                "cantidentify" => "[НЕОПОЗНАН]",
                "beforeattack" => "[ПЕРЕД АТАКОЙ]",
                "endskill" => "[КОНЕЦ НАВЫКА]",
                "onsucceedattack" => "[ПРИ ПОПАДАНИИ]",
                "tabexplain" => "", // Remove completely
                _ => $"[{displayText.ToUpper()}]"
            };
        }
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
            if (formatStack.Count > 0) 
                currentFormat = formatStack.Pop();
        }
        else if (tag.StartsWith("color="))
        {
            // Color tag - push current format and apply color
            formatStack.Push(currentFormat.Clone());
            var colorValue = tag.Substring(6);
            if (TryParseColor(colorValue, out var color)) 
                currentFormat.Color = color;
        }
        else if (tag.StartsWith("sprite name="))
        {
            // Extract sprite name from quotes
            var match = Regex.Match(tag, @"sprite name=""([^""]+)""");
            if (match.Success)
            {
                var spriteName = match.Groups[1].Value;
                var spriteInline = CreateSpriteInline(spriteName);
                if (spriteInline != null) 
                    inlines.Add(spriteInline);
            }
        }
        else if (tag.StartsWith("link="))
        {
            // TODO: Implement proper link functionality later
            // For now, just ignore link tags
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
        switch (styleName.ToLower())
        {
            case "highlight":
                format.Color = Color.Parse("#FFFF00"); // Yellow
                format.IsUnderlined = true;
                break;
        }
    }

    private static Run CreateRun(string text, TextFormat format)
    {
        var run = new Run(text)
        {
            FontFamily = new FontFamily("avares://RainbusToolbox/Assets/Fonts/Pretendard.ttf#Pretendard"),
            Foreground = format.Color.HasValue 
                ? new SolidColorBrush(format.Color.Value) 
                : Brushes.White
        };

        if (format.IsBold)
            run.FontWeight = FontWeight.Bold;

        if (format.IsItalic)
            run.FontStyle = FontStyle.Italic;

        if (format.IsUnderlined)
            run.TextDecorations = TextDecorations.Underline;

        return run;
    }

    private static InlineUIContainer? CreateSpriteInline(string spriteName)
    {
        try
        {
            var bitmap = LoadSprite(spriteName);
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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load sprite '{spriteName}': {ex.Message}");
        }

        try
        {
            var placeholderBitmap = LoadSprite("Placeholder");
            if (placeholderBitmap != null)
            {
                var placeholderImage = new Image
                {
                    Source = placeholderBitmap,
                    Width = 18,
                    Height = 18,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(1, 0),
                    Opacity = 0.7
                };

                return new InlineUIContainer(placeholderImage);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load placeholder sprite: {ex.Message}");
        }

        return new InlineUIContainer(new TextBlock
        {
            Text = $"[{spriteName}]",
            Foreground = Brushes.Orange,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("avares://RainbusToolbox/Assets/Fonts/Pretendard.ttf#Pretendard")
        });
    }

    private static Bitmap? LoadSprite(string spriteName)
    {
        var possiblePaths = new[]
        {
            $"avares://RainbusToolbox/Assets/Icons/{spriteName}.png",
            $"avares://RainbusToolbox/Assets/Sprites/{spriteName}.png", 
            $"avares://RainbusToolbox/Assets/{spriteName}.png",
            $"avares://RainbusToolbox/Icons/{spriteName}.png"
        };

        foreach (var path in possiblePaths)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Trying to load sprite from: {path}");
                var uri = new Uri(path);
                var stream = AssetLoader.Open(uri);
                var bitmap = new Bitmap(stream);
                System.Diagnostics.Debug.WriteLine($"Successfully loaded sprite from: {path}");
                return bitmap;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load from {path}: {ex.Message}");
                continue;
            }
        }

        System.Diagnostics.Debug.WriteLine($"Failed to load sprite '{spriteName}' from any path");
        return null;
    }

    private static bool TryParseColor(string colorStr, out Color color)
    {
        color = default;

        if (string.IsNullOrEmpty(colorStr))
            return false;

        if (colorStr.StartsWith("#"))
        {
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