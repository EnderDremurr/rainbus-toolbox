using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RainbusToolbox.Services;

public class KeyWordConversionService
{
    private const string FileUrl =
        "https://raw.githubusercontent.com/kimght/LimbusCompanyRuMTL/main/data/build/keyword_colors.txt";

    public Dictionary<string, string> KeywordToColorMap { get; private set; } = new();

    public KeyWordConversionService()
    {
        InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        try
        {
            using var http = new HttpClient();
            var content = await http.GetStringAsync(FileUrl);

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using var reader = new StringReader(content);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('¤', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    if (!dict.ContainsKey(key))
                        dict[key] = value;
                }
            }

            KeywordToColorMap = dict;
        }
        catch
        {
            // govno
            KeywordToColorMap = new Dictionary<string, string>();
        }
    }
    
    public string GetMeshFromTag(string tag)
    {
        // Validate the tag format
        if (string.IsNullOrEmpty(tag) || tag.Length < 2 || tag[0] != '[' || tag[tag.Length - 1] != ']')
            throw new ArgumentException("Invalid tag format");

        string inner = tag.Substring(1, tag.Length - 2); // strip [ and ]
        string tagKeyword;
        string tagFill;

        // Check for the backtick pattern
        int backtickIndex = inner.IndexOf(":`");
        if (backtickIndex >= 0)
        {
            tagKeyword = inner.Substring(0, backtickIndex);
            // Grab everything between the backticks
            int endBacktick = inner.IndexOf('`', backtickIndex + 2);
            if (endBacktick < 0)
                throw new ArgumentException("Invalid tag format: missing closing backtick");

            tagFill = inner.Substring(backtickIndex + 2, endBacktick - (backtickIndex + 2));
        }
        else
        {
            tagKeyword = inner;
            tagFill = inner;
        }

        // Case-insensitive lookup
        string actualKey = KeywordToColorMap.Keys
                               .FirstOrDefault(k => string.Equals(k, tagKeyword, StringComparison.OrdinalIgnoreCase));

        // If no color found, return original tag unchanged
        if (actualKey == null || !KeywordToColorMap.ContainsKey(actualKey))
        {
            return tag;
        }

        var color = KeywordToColorMap[actualKey];

        var mesh = $"<sprite name=\\\"{actualKey}\\\"><color={color}><u><link=\\\"{actualKey}\\\">{tagFill}</link></u></color>";

        return mesh;
    }

    public string GetTagFromMesh(string mesh)
    {
        // i have no fucking clue about what gpt made here but i hope it works and i wont need to rewrite it later cuz IM FUCKING STOOOOOPIIIIID
        if (string.IsNullOrEmpty(mesh))
            throw new ArgumentException("Mesh cannot be null or empty");

        // Find the tagKeyword from <sprite name="...">
        int spriteStart = mesh.IndexOf("<sprite name=\"");
        if (spriteStart < 0)
            throw new ArgumentException("Invalid mesh format: missing <sprite>");

        spriteStart += "<sprite name=\"".Length;
        int spriteEnd = mesh.IndexOf("\">", spriteStart);
        if (spriteEnd < 0)
            throw new ArgumentException("Invalid mesh format: malformed <sprite>");

        string tagKeyword = mesh.Substring(spriteStart, spriteEnd - spriteStart);

        // Find the tagFill from <link="...">...</link>
        int linkStart = mesh.IndexOf("<link=\"", spriteEnd);
        if (linkStart < 0)
            return $"[{tagKeyword}]"; // no fill detected, return simple tag

        linkStart += "<link=\"".Length;
        int linkEnd = mesh.IndexOf("\">", linkStart);
        if (linkEnd < 0)
            throw new ArgumentException("Invalid mesh format: malformed <link>");

        int fillStart = linkEnd + 2;
        int fillEnd = mesh.IndexOf("</link>", fillStart);
        if (fillEnd < 0)
            throw new ArgumentException("Invalid mesh format: missing </link>");

        string tagFill = mesh.Substring(fillStart, fillEnd - fillStart);

        // Return the original tag format
        if (tagFill == tagKeyword)
            return $"[{tagKeyword}]";  // no fill needed
        else
            return $"[{tagKeyword}:`{tagFill}`]";
    }

    public string EncloseInTag(string text, string tag)
    {
        return "";
    }

    public void ReplaceEveryTagWithMesh(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        var tagRegex = new Regex(@"\[[^\]:]+:`[^`]+`\]", RegexOptions.Compiled);

        foreach (var file in files)
        {
            try
            {
                // Preserve original encoding and timestamp
                var encoding = GetFileEncoding(file);
                var originalTime = File.GetLastWriteTime(file);
                
                string content = File.ReadAllText(file, encoding);

                string replacedContent = tagRegex.Replace(content, match =>
                {
                    return GetMeshFromTag(match.Value);
                });

                // Only write if content actually changed
                if (content != replacedContent)
                {
                    // Write with UTF-8 no BOM to avoid encoding issues
                    File.WriteAllText(file, replacedContent, new UTF8Encoding(false));
                    // Restore original timestamp
                    File.SetLastWriteTime(file, originalTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {file}: {ex.Message}");
            }
        }
    }

    public void ReplaceTagsInJsonFiles(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            try
            {
                var originalTime = File.GetLastWriteTime(file);
                
                string json = File.ReadAllText(file, new UTF8Encoding(false));
                JToken root = JToken.Parse(json);
                
                bool changed = ReplaceTagsInToken(root);
                
                if (changed)
                {
                    // Preserve original formatting by using minimal formatting
                    string output = root.ToString(Newtonsoft.Json.Formatting.None);
                    File.WriteAllText(file, output, new UTF8Encoding(false));
                    File.SetLastWriteTime(file, originalTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing JSON {file}: {ex.Message}");
            }
        }
    }

    private bool ReplaceTagsInToken(JToken token)
    {
        bool changed = false;
        var tagRegex = new Regex(@"\[[^\]:]+:`[^`]+`\]", RegexOptions.Compiled);

        if (token.Type == JTokenType.String)
        {
            var str = token.ToString();
            var replaced = tagRegex.Replace(str, match => GetMeshFromTag(match.Value));
            if (str != replaced)
            {
                ((JValue)token).Value = replaced;
                changed = true;
            }
        }
        else if (token.HasValues)
        {
            foreach (var child in token.Children())
            {
                if (ReplaceTagsInToken(child))
                    changed = true;
            }
        }

        return changed;
    }

    private static Encoding GetFileEncoding(string filename)
    {
        using var reader = new StreamReader(filename, true);
        reader.Peek(); // Force encoding detection
        return reader.CurrentEncoding;
    }

    // Debug method to compare manual vs generated tags
    public void DebugCompareTags(string testTag)
    {
        string generated = GetMeshFromTag(testTag);
        
        Console.WriteLine($"Original: {testTag}");
        Console.WriteLine($"Generated: {generated}");
        Console.WriteLine($"Generated bytes: {string.Join(",", Encoding.UTF8.GetBytes(generated))}");
    }
}