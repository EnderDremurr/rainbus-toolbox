using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Models.Managers;

namespace RainbusToolbox.Services;

public class KeyWordConversionService
{
    private const string FileUrl =
        "https://raw.githubusercontent.com/kimght/LimbusCompanyRuMTL/main/data/build/keyword_colors.txt";

    private readonly Regex tagRegex = new(@"\[[^\]:]+:`[^`]+`\]", RegexOptions.Compiled);
    private readonly Regex spriteRegex = new(@"<sprite[^>]+>", RegexOptions.Compiled);
    private readonly PersistentDataManager _dataManager;

    public Dictionary<string, string> KeywordToColorMap { get; private set; } = new();

    public KeyWordConversionService(PersistentDataManager dataManager)
    {
        _dataManager = dataManager;
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
            KeywordToColorMap = new Dictionary<string, string>();
        }
    }


    private string CleanJsonContentForParsing(string jsonContent)
    {
        // Replace [Keyword:`...`] tags with placeholders
        var tagMatches = tagRegex.Matches(jsonContent);
        for (var i = 0; i < tagMatches.Count; i++)
        {
            var placeholder = $"__TAG_PLACEHOLDER_{i}__";
            jsonContent = jsonContent.Replace(tagMatches[i].Value, $"\"{placeholder}\"");
        }

        // Replace <sprite ...> meshes with placeholders too
        var spriteRegex = new Regex(@"<sprite[^>]+>", RegexOptions.Compiled);
        var spriteMatches = spriteRegex.Matches(jsonContent);
        for (var i = 0; i < spriteMatches.Count; i++)
        {
            var placeholder = $"__SPRITE_PLACEHOLDER_{i}__";
            jsonContent = jsonContent.Replace(spriteMatches[i].Value, $"\"{placeholder}\"");
        }

        return jsonContent;
    }


    private void MergeDataIntoRepository(Dictionary<string, JObject> gameData, string repositoryPath)
    {
        foreach (var kvp in gameData)
        {
            var relativePath = kvp.Key;
            var gameJson = kvp.Value;

            var repositoryFilePath = Path.Combine(repositoryPath, relativePath);
            var repositoryDir = Path.GetDirectoryName(repositoryFilePath);

            // Ensure directory exists
            if (!Directory.Exists(repositoryDir))
                Directory.CreateDirectory(repositoryDir);

            if (File.Exists(repositoryFilePath))
            {
                MergeJsonFiles(repositoryFilePath, gameJson);
            }
            else
            {
                Console.WriteLine($"Creating new file: {relativePath}");
                File.WriteAllText(repositoryFilePath, gameJson.ToString(Formatting.Indented), Encoding.UTF8);
            }
        }
    }


    private void MergeJsonFiles(string repositoryFilePath, JObject gameJson)
    {
        try
        {
            var existingContent = File.ReadAllText(repositoryFilePath, Encoding.UTF8);
            var repositoryJson = JObject.Parse(existingContent);

            var changed = MergeJsonObjectsAdditive(repositoryJson, gameJson);

            if (changed)
            {
                Console.WriteLine($"Updated file: {Path.GetFileName(repositoryFilePath)}");
                File.WriteAllText(repositoryFilePath, repositoryJson.ToString(Formatting.Indented), Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error merging {repositoryFilePath}: {ex.Message}");
        }
    }

    private bool MergeJsonObjectsAdditive(JObject target, JObject source)
    {
        var changed = false;

        foreach (var property in source.Properties())
            if (target.ContainsKey(property.Name))
            {
                var targetProp = target[property.Name];
                var sourceProp = property.Value;

                if (targetProp.Type == JTokenType.Object && sourceProp.Type == JTokenType.Object)
                {
                    // Recursively merge nested objects
                    if (MergeJsonObjectsAdditive((JObject)targetProp, (JObject)sourceProp))
                        changed = true;
                }
                else if (targetProp.Type == JTokenType.Array && sourceProp.Type == JTokenType.Array)
                {
                    // Merge arrays additively
                    if (MergeJsonArraysAdditive((JArray)targetProp, (JArray)sourceProp))
                        changed = true;
                }
                // Primitive values are preserved (no overwrite)
            }
            else
            {
                // Add missing property
                target[property.Name] = property.Value.DeepClone();
                changed = true;
                Console.WriteLine($"✅ Added new property: {property.Name}");
            }

        return changed;
    }


    private bool MergeJsonArraysAdditive(JArray targetArray, JArray sourceArray)
    {
        var changed = false;

        foreach (var sourceItem in sourceArray)
        {
            var found = false;

            if (sourceItem.Type == JTokenType.Object)
            {
                var sourceObj = (JObject)sourceItem;
                var identifier = GetObjectIdentifier(sourceObj);

                foreach (var targetItem in targetArray.OfType<JObject>())
                {
                    var targetId = GetObjectIdentifier(targetItem);
                    if (!string.IsNullOrEmpty(identifier) && identifier == targetId)
                    {
                        found = true;
                        // Merge nested object
                        if (MergeJsonObjectsAdditive(targetItem, sourceObj))
                            changed = true;
                        break;
                    }

                    // No identifier? Check full object equality
                    if (identifier == null && JToken.DeepEquals(targetItem, sourceObj))
                    {
                        found = true;
                        break;
                    }
                }
            }
            else
            {
                // Primitive values
                if (targetArray.Any(t => JToken.DeepEquals(t, sourceItem)))
                    found = true;
            }

            if (!found)
            {
                targetArray.Add(sourceItem.DeepClone());
                changed = true;

                var preview = sourceItem.Type == JTokenType.Object
                    ? sourceItem.ToString(Formatting.None)
                    : sourceItem.ToString();

                Console.WriteLine($"✅ Added new array item: {preview.Substring(0, Math.Min(50, preview.Length))}...");
            }
        }

        return changed;
    }


    private bool MergeJsonObjects(JObject target, JObject source)
    {
        var changed = false;

        foreach (var property in source.Properties())
            if (target.ContainsKey(property.Name))
            {
                var targetProp = target[property.Name];
                var sourceProp = property.Value;

                if (targetProp.Type == JTokenType.Object && sourceProp.Type == JTokenType.Object)
                {
                    if (MergeJsonObjects((JObject)targetProp, (JObject)sourceProp))
                        changed = true;
                }
                else if (targetProp.Type == JTokenType.Array && sourceProp.Type == JTokenType.Array)
                {
                    if (MergeJsonArrays((JArray)targetProp, (JArray)sourceProp))
                        changed = true;
                }
                // Primitive values are kept as-is, do not overwrite
            }
            else
            {
                target[property.Name] = property.Value.DeepClone();
                changed = true;
                Console.WriteLine($"Added new property: {property.Name}");
            }

        return changed;
    }

    private bool MergeJsonArrays(JArray targetArray, JArray sourceArray)
    {
        var changed = false;

        foreach (var sourceItem in sourceArray)
        {
            var found = false;

            if (sourceItem.Type == JTokenType.Object)
            {
                var sourceObj = (JObject)sourceItem;
                var identifier = GetObjectIdentifier(sourceObj);

                foreach (var targetItem in targetArray.OfType<JObject>())
                {
                    var targetId = GetObjectIdentifier(targetItem);

                    if (!string.IsNullOrEmpty(identifier) && identifier == targetId)
                    {
                        // Merge objects with matching identifiers
                        found = true;
                        if (MergeJsonObjects(targetItem, sourceObj))
                            changed = true;
                        break;
                    }

                    if (identifier == null && JToken.DeepEquals(targetItem, sourceObj))
                    {
                        // No identifier: check full object equality
                        found = true;
                        break;
                    }
                }
            }
            else
            {
                // Primitive values
                if (targetArray.Any(t => JToken.DeepEquals(t, sourceItem)))
                    found = true;
            }

            if (!found)
            {
                targetArray.Add(sourceItem.DeepClone());
                changed = true;

                var preview = sourceItem.Type == JTokenType.Object
                    ? sourceItem.ToString(Formatting.None)
                    : sourceItem.ToString();

                Console.WriteLine($"Added new array item: {preview.Substring(0, Math.Min(50, preview.Length))}...");
            }
        }

        return changed;
    }


    private string? GetObjectIdentifier(JObject obj)
    {
        string[] idProperties = { "id", "ID", "Id", "name", "Name", "key", "Key", "guid", "GUID" };

        foreach (var prop in idProperties)
            if (obj.ContainsKey(prop) && obj[prop] != null)
                return obj[prop]?.ToString();

        return null;
    }


    private void ReplaceTagsInRepositoryFiles(string repositoryPath)
    {
        // Process non-JSON files
        ReplaceEveryTagWithMesh(repositoryPath);

        // Process JSON files with special handling
        ReplaceTagsInJsonFiles(repositoryPath);
    }

    public string GetMeshFromTag(string tag)
    {
        // Validate the tag format
        if (string.IsNullOrEmpty(tag) || tag.Length < 2 || tag[0] != '[' || tag[tag.Length - 1] != ']')
            throw new ArgumentException("Invalid tag format");

        var inner = tag.Substring(1, tag.Length - 2); // strip [ and ]
        string tagKeyword;
        string tagFill;

        // Check for the backtick pattern
        var backtickIndex = inner.IndexOf(":`");
        if (backtickIndex >= 0)
        {
            tagKeyword = inner.Substring(0, backtickIndex);
            // Grab everything between the backticks
            var endBacktick = inner.IndexOf('`', backtickIndex + 2);
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
        var actualKey = KeywordToColorMap.Keys
            .FirstOrDefault(k => string.Equals(k, tagKeyword, StringComparison.OrdinalIgnoreCase));

        // If no color found, return original tag unchanged
        if (actualKey == null || !KeywordToColorMap.ContainsKey(actualKey)) return tag;

        var color = KeywordToColorMap[actualKey];

        var mesh =
            $"<sprite name=\\\"{actualKey}\\\"><color={color}><u><link=\\\"{actualKey}\\\">{tagFill}</link></u></color>";

        return mesh;
    }

    public string GetTagFromMesh(string mesh)
    {
        if (string.IsNullOrEmpty(mesh))
            throw new ArgumentException("Mesh cannot be null or empty");

        // Find the tagKeyword from <sprite name="...">
        var spriteStart = mesh.IndexOf("<sprite name=\"");
        if (spriteStart < 0)
            throw new ArgumentException("Invalid mesh format: missing <sprite>");

        spriteStart += "<sprite name=\"".Length;
        var spriteEnd = mesh.IndexOf("\">", spriteStart);
        if (spriteEnd < 0)
            throw new ArgumentException("Invalid mesh format: malformed <sprite>");

        var tagKeyword = mesh.Substring(spriteStart, spriteEnd - spriteStart);

        // Find the tagFill from <link="...">...</link>
        var linkStart = mesh.IndexOf("<link=\"", spriteEnd);
        if (linkStart < 0)
            return $"[{tagKeyword}]"; // no fill detected, return simple tag

        linkStart += "<link=\"".Length;
        var linkEnd = mesh.IndexOf("\">", linkStart);
        if (linkEnd < 0)
            throw new ArgumentException("Invalid mesh format: malformed <link>");

        var fillStart = linkEnd + 2;
        var fillEnd = mesh.IndexOf("</link>", fillStart);
        if (fillEnd < 0)
            throw new ArgumentException("Invalid mesh format: missing </link>");

        var tagFill = mesh.Substring(fillStart, fillEnd - fillStart);

        // Return the original tag format
        if (tagFill == tagKeyword)
            return $"[{tagKeyword}]"; // no fill needed
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

        // Exclude JSON files from general processing to avoid corruption
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
            try
            {
                // Preserve original encoding and timestamp
                var encoding = GetFileEncoding(file);
                var originalTime = File.GetLastWriteTime(file);

                var content = File.ReadAllText(file, encoding);

                var replacedContent = tagRegex.Replace(content, match => { return GetMeshFromTag(match.Value); });

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

    public void ReplaceTagsInJsonFiles(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        var files = Directory.GetFiles(path, "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
            try
            {
                var originalTime = File.GetLastWriteTime(file);

                var json = File.ReadAllText(file, Encoding.UTF8);
                var root = JToken.Parse(json);

                var changed = ReplaceTagsInToken(root);

                if (changed)
                {
                    // Use indented formatting for better readability
                    var output = root.ToString(Formatting.Indented);
                    File.WriteAllText(file, output, Encoding.UTF8);
                    File.SetLastWriteTime(file, originalTime);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing JSON {file}: {ex.Message}");
            }
    }

    private bool ReplaceTagsInToken(JToken token)
    {
        var changed = false;

        if (token.Type == JTokenType.String)
        {
            var str = token.ToString();
            var replaced = tagRegex.Replace(str, match =>
            {
                var meshValue = GetMeshFromTag(match.Value);
                // Ensure JSON-safe content by properly escaping quotes
                return meshValue?.Replace("\"", "\\\"") ?? match.Value;
            });

            if (str != replaced)
            {
                ((JValue)token).Value = replaced;
                changed = true;
            }
        }
        else if (token.HasValues)
        {
            foreach (var child in token.Children())
                if (ReplaceTagsInToken(child))
                    changed = true;
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
        var generated = GetMeshFromTag(testTag);

        Console.WriteLine($"Original: {testTag}");
        Console.WriteLine($"Generated: {generated}");
        Console.WriteLine($"Generated bytes: {string.Join(",", Encoding.UTF8.GetBytes(generated))}");
    }
}