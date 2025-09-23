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

    private readonly Regex tagRegex = new(@"\[[^:\]]+:[`*'][^`*']+[`*']\]", RegexOptions.Compiled);
    private readonly PersistentDataManager _dataManager;

    private bool _isInitialized;
    public Dictionary<string, string> KeywordToColorMap { get; private set; } = new();

    public KeyWordConversionService(PersistentDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await InitializeAsync();
        _isInitialized = true;
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


    public string GetMeshFromTag(string tag)
    {
        // Validate the tag format
        if (string.IsNullOrEmpty(tag) || tag.Length < 2 || tag[0] != '[' || tag[tag.Length - 1] != ']')
            throw new ArgumentException("Invalid tag format");

        var inner = tag.Substring(1, tag.Length - 2); // strip [ and ]
        string tagKeyword;
        string tagFill;

        // Check for delimiter patterns (backtick, asterisk, or single quote)
        var delimiterIndex = inner.IndexOfAny(['`', '*', '\'']);
        if (delimiterIndex >= 0 && inner[delimiterIndex - 1] == ':')
        {
            var delimiter = inner[delimiterIndex];
            tagKeyword = inner.Substring(0, delimiterIndex - 1);

            // Find the closing delimiter
            var endDelimiter = inner.IndexOf(delimiter, delimiterIndex + 1);
            if (endDelimiter < 0)
                throw new ArgumentException($"Invalid tag format: missing closing {delimiter}");

            tagFill = inner.Substring(delimiterIndex + 1, endDelimiter - (delimiterIndex + 1));
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

        var color = KeywordToColorMap[actualKey].Trim('`', ' '); // Remove backticks and spaces
        var mesh =
            $"<sprite name=\\\"{actualKey}\\\"><color={color}><u><link=\\\"{actualKey}\\\"><noparse>{tagFill}</noparse></link></u></color>";

        return mesh;
    }


    public async Task ReplaceEveryTagWithMesh(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        await EnsureInitializedAsync();


        // Exclude JSON files from general processing to avoid corruption
        var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

        foreach (var file in files)
            try
            {
                // Preserve original encoding and timestamp
                var encoding = GetFileEncoding(file);
                var originalTime = File.GetLastWriteTime(file);

                var content = File.ReadAllText(file, encoding);
                var matches = tagRegex.Matches(content);
                if (matches.Count > 0)
                    Console.WriteLine($"Found {matches.Count} matches in {Path.GetFileName(file)}");
                var replacedContent = tagRegex.Replace(content, match =>
                {
                    Console.WriteLine($"Processing match: '{match.Value}'");
                    var result = GetMeshFromTag(match.Value);
                    Console.WriteLine($"Original: '{match.Value}'");
                    Console.WriteLine($"Result:   '{result}'");
                    Console.WriteLine($"Are they equal? {match.Value == result}");
                    return result;
                });

                Console.WriteLine($"Content changed: {content != replacedContent}");
                Console.WriteLine($"Original content length: {content.Length}");
                Console.WriteLine($"Replaced content length: {replacedContent.Length}");

// Let's also check character-by-character if they're close in length
                if (Math.Abs(content.Length - replacedContent.Length) < 100)
                    for (var i = 0; i < Math.Min(content.Length, replacedContent.Length); i++)
                        if (content[i] != replacedContent[i])
                        {
                            Console.WriteLine($"First difference at position {i}:");
                            Console.WriteLine(
                                $"Original: '{content.Substring(Math.Max(0, i - 10), Math.Min(20, content.Length - i + 10))}'");
                            Console.WriteLine(
                                $"Replaced: '{replacedContent.Substring(Math.Max(0, i - 10), Math.Min(20, replacedContent.Length - i + 10))}'");
                            break;
                        }

                if (content != replacedContent)
                {
                    Console.WriteLine($"Writing changes to {Path.GetFileName(file)}");
                    File.WriteAllText(file, replacedContent, new UTF8Encoding(false));
                    File.SetLastWriteTime(file, originalTime);
                }
                else
                {
                    Console.WriteLine($"No changes detected in {Path.GetFileName(file)} - content comparison failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {file}: {ex.Message}");
            }
    }


    private static Encoding GetFileEncoding(string filename)
    {
        using var reader = new StreamReader(filename, true);
        reader.Peek(); // Force encoding detection
        return reader.CurrentEncoding;
    }
}