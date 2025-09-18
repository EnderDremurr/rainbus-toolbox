using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace RainbusToolbox.Services;

public class FileMergingService
{
    public async Task<int[]> PullFilesFromTheGameAsync(string pathToLocalization, string pathToReferenceLocalization, 
        CancellationToken cancellationToken = default, IProgress<string> progress = null)
    {
        return await Task.Run(() => 
        {
            var newFiles = 0;
            var expandedFiles = 0;
            var checkedFiles = 0;
        
            progress?.Report("Starting file processing...");
            Console.WriteLine("Starting file processing...");
            
            var localizationFiles = Directory.GetFiles(pathToLocalization, "*.json", SearchOption.AllDirectories).ToList();
            var referenceFiles = Directory.GetFiles(pathToReferenceLocalization, "*.json", SearchOption.AllDirectories).ToList();
            
            var message = $"Found {localizationFiles.Count} localization files and {referenceFiles.Count} reference files";
            progress?.Report(message);
            Console.WriteLine(message);
            
            // Debug: Log all filenames to see what's happening
            Console.WriteLine("\n=== DEBUGGING DUPLICATE FILES ===");
            var fileNames = new List<string>();
            var duplicateCheck = new Dictionary<string, List<string>>();
            
            foreach (var file in localizationFiles)
            {
                var fileName = Path.GetFileName(file);
                var fullPath = file;
                
                fileNames.Add(fileName);
                
                if (!duplicateCheck.ContainsKey(fileName))
                {
                    duplicateCheck[fileName] = new List<string>();
                }
                duplicateCheck[fileName].Add(fullPath);
                
                Console.WriteLine($"File: '{fileName}' -> Path: '{fullPath}'");
            }
            
            // Check for actual duplicates
            var actualDuplicates = duplicateCheck.Where(kvp => kvp.Value.Count > 1).ToList();
            if (actualDuplicates.Any())
            {
                Console.WriteLine("\n=== FOUND ACTUAL DUPLICATES ===");
                foreach (var duplicate in actualDuplicates)
                {
                    Console.WriteLine($"Duplicate filename: '{duplicate.Key}'");
                    foreach (var path in duplicate.Value)
                    {
                        Console.WriteLine($"  -> {path}");
                    }
                }
            }
            else
            {
                Console.WriteLine("No actual duplicates found. Checking for other issues...");
                
                // Check for null/empty filenames
                var emptyNames = localizationFiles.Where(f => string.IsNullOrEmpty(Path.GetFileName(f))).ToList();
                if (emptyNames.Any())
                {
                    Console.WriteLine("Found files with empty/null names:");
                    emptyNames.ForEach(f => Console.WriteLine($"  -> {f}"));
                }
                
                // Check for special characters or encoding issues
                var suspiciousFiles = localizationFiles.Where(f => 
                {
                    var name = Path.GetFileName(f);
                    return name.Contains('\0') || name.Length != name.Trim().Length || name.Contains("  ");
                }).ToList();
                
                if (suspiciousFiles.Any())
                {
                    Console.WriteLine("Found files with suspicious characters:");
                    suspiciousFiles.ForEach(f => Console.WriteLine($"  -> '{Path.GetFileName(f)}' in {f}"));
                }
            }
            Console.WriteLine("=== END DEBUGGING ===\n");
            
            // Create a dictionary for faster lookup, with detailed error handling
            var localizationFileMap = new Dictionary<string, string>();
            foreach (var file in localizationFiles)
            {
                var fileName = Path.GetFileName(file);
                
                if (string.IsNullOrEmpty(fileName))
                {
                    Console.WriteLine($"Skipping file with null/empty name: {file}");
                    continue;
                }
                
                if (localizationFileMap.ContainsKey(fileName))
                {
                    Console.WriteLine($"ERROR: Duplicate key '{fileName}' detected!");
                    Console.WriteLine($"  Existing: {localizationFileMap[fileName]}");
                    Console.WriteLine($"  New: {file}");
                    Console.WriteLine($"  Using existing file and skipping new one.");
                }
                else
                {
                    localizationFileMap[fileName] = file;
                }
            }
        
            foreach (var referenceFile in referenceFiles)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                checkedFiles++;
                
                // Add progress reporting
                if (checkedFiles % 50 == 0)
                {
                    var progressMessage = $"Processed {checkedFiles}/{referenceFiles.Count} files... (Added: {newFiles}, Merged: {expandedFiles})";
                    progress?.Report(progressMessage);
                    Console.WriteLine(progressMessage);
                    
                    // Yield control to prevent UI freezing
                    Thread.Sleep(1);
                }
            
                var referenceFileNameNoPrefix = Path.GetFileName(GetPathWithoutPrefix(referenceFile));
            
                // Use dictionary lookup instead of FirstOrDefault for better performance
                if (!localizationFileMap.TryGetValue(referenceFileNameNoPrefix, out var existingFilePath))
                {
                    try
                    {
                        CopyFileFromTo(referenceFile, pathToLocalization, pathToReferenceLocalization);
                        newFiles++;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = $"Error copying file {referenceFile}: {ex.Message}";
                        progress?.Report(errorMessage);
                        Console.WriteLine(errorMessage);
                    }
                    continue;
                }
            
                try
                {
                    CastToJsonAndMerge(existingFilePath, referenceFile);
                    expandedFiles++;
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error merging file {referenceFile}: {ex.Message}";
                    progress?.Report(errorMessage);
                    Console.WriteLine(errorMessage);
                }
            }

            var finalMessage = $"Completed! Added {newFiles} files, merged {expandedFiles} files. Total files processed: {checkedFiles}.";
            progress?.Report(finalMessage);
            Console.WriteLine(finalMessage);
            
            return new int[] { newFiles, expandedFiles, checkedFiles };
            
        }, cancellationToken);
    }

    private string GetPathWithoutPrefix(string path)
    {
        var fileName = Path.GetFileName(path);
        var cleanFileName = fileName.StartsWith("EN_") ? fileName.Substring(3) : fileName;
        return Path.Combine(Path.GetDirectoryName(path) ?? "", cleanFileName);
    }

    private void CastToJsonAndMerge(string destinationPath, string sourcePath)
    {
        try
        {
            // Read files with explicit encoding
            var destinationContent = File.ReadAllText(destinationPath, new UTF8Encoding(false));
            var sourceContent = File.ReadAllText(sourcePath, new UTF8Encoding(false));
            
            // Quick validation - skip empty files
            if (string.IsNullOrWhiteSpace(sourceContent))
            {
                Console.WriteLine($"Skipping empty source file: {sourcePath}");
                return;
            }
            
            // Check for Git merge conflict markers and offer to clean them
            var gitConflictMarkers = new[] { "<<<<<<<", "=======", ">>>>>>>" };
            bool hasSourceConflicts = gitConflictMarkers.Any(marker => sourceContent.Contains(marker));
            bool hasDestinationConflicts = gitConflictMarkers.Any(marker => destinationContent.Contains(marker));
            
            if (hasSourceConflicts)
            {
                Console.WriteLine($"Source file has Git merge conflict markers: {sourcePath}");
                Console.WriteLine("Attempting to auto-clean conflict markers...");
                sourceContent = CleanGitConflictMarkers(sourceContent);
                Console.WriteLine("Source file cleaned. Please verify the result manually.");
            }
            
            if (hasDestinationConflicts)
            {
                Console.WriteLine($"Destination file has Git merge conflict markers: {destinationPath}");
                Console.WriteLine("Attempting to auto-clean conflict markers...");
                destinationContent = CleanGitConflictMarkers(destinationContent);
                
                // Write the cleaned content back to the file
                try
                {
                    File.WriteAllText(destinationPath, destinationContent, new UTF8Encoding(false));
                    Console.WriteLine($"Cleaned and saved destination file: {destinationPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save cleaned destination file: {ex.Message}");
                    return;
                }
            }
            
            // Check for HTML/XML content that might be corrupting the JSON
            if (sourceContent.TrimStart().StartsWith("<") && !sourceContent.Contains("<<<<<<<"))
            {
                Console.WriteLine($"Skipping file that appears to contain HTML/XML instead of JSON: {sourcePath}");
                Console.WriteLine($"First 200 characters: {sourceContent.Substring(0, Math.Min(200, sourceContent.Length))}");
                return;
            }
            
            // Additional validation for common JSON corruption indicators
            if (sourceContent.Contains("<!DOCTYPE") || sourceContent.Contains("<html") || sourceContent.Contains("<?xml"))
            {
                Console.WriteLine($"Skipping file that contains HTML/XML markers: {sourcePath}");
                return;
            }
            
            JObject deserializedDestination;
            JObject deserializedSource;
            
            try
            {
                deserializedDestination = JObject.Parse(destinationContent);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse destination JSON file {destinationPath}: {ex.Message}");
                Console.WriteLine($"First 200 characters of destination: {destinationContent.Substring(0, Math.Min(200, destinationContent.Length))}");
                throw;
            }
            
            try
            {
                deserializedSource = JObject.Parse(sourceContent);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to parse source JSON file {sourcePath}: {ex.Message}");
                Console.WriteLine($"First 500 characters of source: {sourceContent.Substring(0, Math.Min(500, sourceContent.Length))}");
                Console.WriteLine($"Last 200 characters of source: {sourceContent.Substring(Math.Max(0, sourceContent.Length - 200))}");
                
                // Try to find the problematic character
                var lines = sourceContent.Split('\n');
                for (int i = 0; i < Math.Min(10, lines.Length); i++)
                {
                    if (lines[i].Contains('<'))
                    {
                        Console.WriteLine($"Found '<' character on line {i + 1}: {lines[i]}");
                    }
                }
                throw;
            }
        
            // Try both "DataList" and "dataList" to handle case sensitivity
            var sourceDataList = (deserializedSource["DataList"] as JArray) ?? (deserializedSource["dataList"] as JArray);
            var destinationDataList = (deserializedDestination["DataList"] as JArray) ?? (deserializedDestination["dataList"] as JArray);

            if (sourceDataList == null)
            {
                Console.WriteLine($"Warning: No DataList or dataList found in source file: {sourcePath}");
                return;
            }
        
            if (destinationDataList == null)
            {
                destinationDataList = new JArray();
                
                // Use the same case as the source file
                if (deserializedSource["dataList"] != null)
                {
                    deserializedDestination["dataList"] = destinationDataList;
                }
                else
                {
                    deserializedDestination["DataList"] = destinationDataList;
                }
            }
            
            // Create a HashSet for faster ID lookup
            var existingIds = new HashSet<string>(
                destinationDataList
                    .Where(item => item["id"] != null)
                    .Select(item => item["id"].ToString())
            );
        
            foreach (var jToken in sourceDataList)
            {
                var sourceItem = (JObject)jToken;
                var sourceId = sourceItem["id"]?.ToString();
            
                if (string.IsNullOrEmpty(sourceId))
                    continue;

                if (existingIds.Contains(sourceId))
                {
                    // Find and merge existing item
                    var existingItem = destinationDataList
                        .FirstOrDefault(item => item["id"]?.ToString() == sourceId) as JObject;
                        
                    if (existingItem != null)
                    {
                        foreach (JProperty property in sourceItem.Properties())
                        {
                            if (existingItem[property.Name] == null)
                            {
                                existingItem.Add(property.Name, property.Value?.DeepClone());
                            }
                        }
                    }
                }
                else
                {
                    // Add new item
                    destinationDataList.Add(sourceItem.DeepClone());
                    existingIds.Add(sourceId);
                }
            }
        
            // Write with explicit encoding and better formatting
            var file = JsonConvert.SerializeObject(deserializedDestination, Formatting.Indented);
            File.WriteAllText(destinationPath, file, new UTF8Encoding(false));
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error in files {destinationPath} or {sourcePath}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error processing {destinationPath}: {ex.Message}");
            throw;
        }
    }

    private string CleanGitConflictMarkers(string content)
    {
        var lines = content.Split('\n').ToList();
        var cleanedLines = new List<string>();
        bool skipUntilEnd = false;
        bool inConflict = false;
        
        for (int i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            
            if (line.Contains("<<<<<<<"))
            {
                // Start of conflict - keep HEAD version (our changes)
                inConflict = true;
                continue;
            }
            else if (line.Contains("======="))
            {
                // Separator - start skipping until end marker
                skipUntilEnd = true;
                continue;
            }
            else if (line.Contains(">>>>>>>"))
            {
                // End of conflict
                inConflict = false;
                skipUntilEnd = false;
                continue;
            }
            
            // Keep lines that are not part of the "theirs" section
            if (!skipUntilEnd)
            {
                cleanedLines.Add(line);
            }
        }
        
        return string.Join('\n', cleanedLines);
    }

    private void CopyFileFromTo(string pathToFileToCopy, string destinationRoot, string referenceRoot)
    {
        try
        {
            string absoluteFileToCopy = Path.GetFullPath(pathToFileToCopy);
            string absoluteReferenceRoot = Path.GetFullPath(referenceRoot);
            string absoluteDestinationRoot = Path.GetFullPath(destinationRoot);

            if (!absoluteFileToCopy.StartsWith(absoluteReferenceRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File {absoluteFileToCopy} is not inside the reference root {absoluteReferenceRoot}.");
            }

            string relativePath = Path.GetRelativePath(absoluteReferenceRoot, absoluteFileToCopy);
            
            // Remove EN_ prefix from the filename in the relative path
            var fileName = Path.GetFileName(relativePath);
            var cleanFileName = fileName.StartsWith("EN_") ? fileName.Substring(3) : fileName;
            var directory = Path.GetDirectoryName(relativePath) ?? "";
            relativePath = Path.Combine(directory, cleanFileName);

            string destinationPath = Path.Combine(absoluteDestinationRoot, relativePath);

            string destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            File.Copy(absoluteFileToCopy, destinationPath, overwrite: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error copying file from {pathToFileToCopy}: {ex.Message}");
            throw;
        }
    }
}