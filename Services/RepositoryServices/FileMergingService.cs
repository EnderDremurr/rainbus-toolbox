using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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

            progress.Report("Starting file processing...");
            Log.Debug("Starting file processing...");

            var localizationFiles =
                Directory.GetFiles(pathToLocalization, "*.json", SearchOption.AllDirectories).ToList();
            var referenceFiles = Directory.GetFiles(pathToReferenceLocalization, "*.json", SearchOption.AllDirectories)
                .ToList();

            var message =
                $"Found {localizationFiles.Count} localization files and {referenceFiles.Count} reference files";
            progress?.Report(message);
            Log.Debug(message);

            // Debug: Log all filenames to see what's happening
            Log.Debug("\n=== DEBUGGING DUPLICATE FILES ===");
            var fileNames = new List<string>();
            var duplicateCheck = new Dictionary<string, List<string>>();

            foreach (var file in localizationFiles)
            {
                var fileName = Path.GetFileName(file);
                var fullPath = file;

                fileNames.Add(fileName);

                if (!duplicateCheck.ContainsKey(fileName)) duplicateCheck[fileName] = new List<string>();
                duplicateCheck[fileName].Add(fullPath);

                Log.Debug($"File: '{fileName}' -> Path: '{fullPath}'");
            }

            // Check for actual duplicates
            var actualDuplicates = duplicateCheck.Where(kvp => kvp.Value.Count > 1).ToList();
            if (actualDuplicates.Any())
            {
                Log.Debug("\n=== FOUND ACTUAL DUPLICATES ===");
                foreach (var duplicate in actualDuplicates)
                {
                    Log.Debug($"Duplicate filename: '{duplicate.Key}'");
                    foreach (var path in duplicate.Value) Log.Debug($"  -> {path}");
                }
            }
            else
            {
                Log.Debug("No actual duplicates found. Checking for other issues...");

                // Check for null/empty filenames
                var emptyNames = localizationFiles.Where(f => string.IsNullOrWhiteSpace(Path.GetFileName(f))).ToList();
                if (emptyNames.Any())
                {
                    Log.Debug("Found files with empty/null names:");
                    emptyNames.ForEach(f => Log.Debug($"  -> {f}"));
                }

                // Check for special characters or encoding issues
                var suspiciousFiles = localizationFiles.Where(f =>
                {
                    var name = Path.GetFileName(f);
                    return name.Contains('\0') || name.Length != name.Trim().Length || name.Contains("  ");
                }).ToList();

                if (suspiciousFiles.Any())
                {
                    Log.Debug("Found files with suspicious characters:");
                    suspiciousFiles.ForEach(f => Log.Debug($"  -> '{Path.GetFileName(f)}' in {f}"));
                }
            }

            Log.Debug("=== END DEBUGGING ===\n");

            // Create a dictionary for faster lookup, with detailed error handling
            var localizationFileMap = new Dictionary<string, string>();
            foreach (var file in localizationFiles)
            {
                var fileName = Path.GetFileName(file);

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    Log.Debug($"Skipping file with null/empty name: {file}");
                    continue;
                }

                if (localizationFileMap.ContainsKey(fileName))
                {
                    Log.Debug($"ERROR: Duplicate key '{fileName}' detected!");
                    Log.Debug($"  Existing: {localizationFileMap[fileName]}");
                    Log.Debug($"  New: {file}");
                    Log.Debug("  Using existing file and skipping new one.");
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
                if (checkedFiles % 67 == 0 || checkedFiles == referenceFiles.Count)
                {
                    var progressMessage =
                        $"Processed {checkedFiles}/{referenceFiles.Count} files... (Added: {newFiles}, Merged: {expandedFiles})";
                    progress?.Report(progressMessage);
                    Log.Debug(progressMessage);

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
                        Log.Debug(errorMessage);
                    }

                    continue;
                }

                try
                {
                    var isMerged = CastToJsonAndMerge(existingFilePath, referenceFile);
                    if (isMerged) expandedFiles++;
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Error merging file {referenceFile}: {ex.Message}";
                    progress?.Report(errorMessage);
                    Log.Debug(errorMessage);
                }
            }


            var finalMessage =
                $"Completed! Added {newFiles} files, merged {expandedFiles} files. Total files processed: {checkedFiles}.";
            progress?.Report(finalMessage);
            Log.Debug(finalMessage);

            return new[] { newFiles, expandedFiles, checkedFiles };
        }, cancellationToken);
    }

    private string GetPathWithoutPrefix(string path)
    {
        var fileName = Path.GetFileName(path);
        var cleanFileName = fileName.StartsWith("EN_") ? fileName.Substring(3) : fileName;
        return Path.Combine(Path.GetDirectoryName(path) ?? "", cleanFileName);
    }

    private bool CastToJsonAndMerge(string destinationPath, string sourcePath)
    {
        var isDirty = false;
        try
        {
            // Read files with explicit encoding
            var destinationContent = File.ReadAllText(destinationPath, new UTF8Encoding(false));
            var sourceContent = File.ReadAllText(sourcePath, new UTF8Encoding(false));

            // Quick validation - skip empty files
            if (string.IsNullOrWhiteSpace(sourceContent))
            {
                Log.Debug($"Skipping empty source file: {sourcePath}");
                return false;
            }

            // Check for Git merge conflict markers and offer to clean them
            var gitConflictMarkers = new[] { "<<<<<<<", "=======", ">>>>>>>" };
            var hasSourceConflicts = gitConflictMarkers.Any(marker => sourceContent.Contains(marker));
            var hasDestinationConflicts = gitConflictMarkers.Any(marker => destinationContent.Contains(marker));

            if (hasSourceConflicts)
            {
                Log.Debug($"Source file has Git merge conflict markers: {sourcePath}");
                Log.Debug("Attempting to auto-clean conflict markers...");
                sourceContent = CleanGitConflictMarkers(sourceContent);
                Log.Debug("Source file cleaned. Please verify the result manually.");
            }

            if (hasDestinationConflicts)
            {
                Log.Debug($"Destination file has Git merge conflict markers: {destinationPath}");
                Log.Debug("Attempting to auto-clean conflict markers...");
                destinationContent = CleanGitConflictMarkers(destinationContent);

                // Write the cleaned content back to the file
                try
                {
                    File.WriteAllText(destinationPath, destinationContent, new UTF8Encoding(false));
                    Log.Debug($"Cleaned and saved destination file: {destinationPath}");
                }
                catch (Exception ex)
                {
                    Log.Debug($"Failed to save cleaned destination file: {ex.Message}");
                    return false;
                }
            }

            // Check for HTML/XML content that might be corrupting the JSON
            if (sourceContent.TrimStart().StartsWith("<") && !sourceContent.Contains("<<<<<<<"))
            {
                Log.Debug($"Skipping file that appears to contain HTML/XML instead of JSON: {sourcePath}");
                Log.Debug(
                    $"First 200 characters: {sourceContent.Substring(0, Math.Min(200, sourceContent.Length))}");
                return false;
            }

            // Additional validation for common JSON corruption indicators
            if (sourceContent.Contains("<!DOCTYPE") || sourceContent.Contains("<html") ||
                sourceContent.Contains("<?xml"))
            {
                Log.Debug($"Skipping file that contains HTML/XML markers: {sourcePath}");
                return false;
            }

            JObject deserializedDestination;
            JObject deserializedSource;

            try
            {
                deserializedDestination = JObject.Parse(destinationContent);
            }
            catch (JsonException ex)
            {
                Log.Debug($"Failed to parse destination JSON file {destinationPath}: {ex.Message}");
                Log.Debug(
                    $"First 200 characters of destination: {destinationContent.Substring(0, Math.Min(200, destinationContent.Length))}");
                throw;
            }

            try
            {
                deserializedSource = JObject.Parse(sourceContent);
            }
            catch (JsonException ex)
            {
                Log.Debug($"Failed to parse source JSON file {sourcePath}: {ex.Message}");
                Log.Debug(
                    $"First 500 characters of source: {sourceContent.Substring(0, Math.Min(500, sourceContent.Length))}");
                Log.Debug(
                    $"Last 200 characters of source: {sourceContent.Substring(Math.Max(0, sourceContent.Length - 200))}");

                // Try to find the problematic character
                var lines = sourceContent.Split('\n');
                for (var i = 0; i < Math.Min(10, lines.Length); i++)
                    if (lines[i].Contains('<'))
                        Log.Debug($"Found '<' character on line {i + 1}: {lines[i]}");

                throw;
            }

            // Try both "DataList" and "dataList" to handle case sensitivity
            var sourceDataList = deserializedSource["DataList"] as JArray ?? deserializedSource["dataList"] as JArray;
            var destinationDataList = deserializedDestination["DataList"] as JArray ??
                                      deserializedDestination["dataList"] as JArray;

            if (sourceDataList == null)
            {
                Log.Debug($"Warning: No DataList or dataList found in source file: {sourcePath}");
                return false;
            }

            if (destinationDataList == null)
            {
                destinationDataList = new JArray();

                // Use the same case as the source file
                if (deserializedSource["dataList"] != null)
                    deserializedDestination["dataList"] = destinationDataList;
                else
                    deserializedDestination["DataList"] = destinationDataList;
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

                if (string.IsNullOrWhiteSpace(sourceId))
                    continue;

                if (existingIds.Contains(sourceId))
                {
                    // Find and merge existing item
                    var existingItem = destinationDataList
                        .FirstOrDefault(item => item["id"]?.ToString() == sourceId) as JObject;

                    if (existingItem != null)
                        foreach (var property in sourceItem.Properties())
                            if (existingItem[property.Name] == null)
                            {
                                existingItem.Add(property.Name, property.Value?.DeepClone());
                                isDirty = true;
                            }
                }
                else
                {
                    // Add new item
                    destinationDataList.Add(sourceItem.DeepClone());
                    existingIds.Add(sourceId);
                    isDirty = true;
                }
            }

            // Write with explicit encoding and better formatting
            var file = JsonConvert.SerializeObject(deserializedDestination, Formatting.Indented);
            File.WriteAllText(destinationPath, file, new UTF8Encoding(false));
        }
        catch (JsonException ex)
        {
            Log.Debug($"JSON parsing error in files {destinationPath} or {sourcePath}: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Log.Debug($"Unexpected error processing {destinationPath}: {ex.Message}");
            throw;
        }

        return isDirty;
    }

    private string CleanGitConflictMarkers(string content)
    {
        var lines = content.Split('\n').ToList();
        var cleanedLines = new List<string>();
        var skipUntilEnd = false;
        var inConflict = false;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (line.Contains("<<<<<<<"))
            {
                // Start of conflict - keep HEAD version (our changes)
                inConflict = true;
                continue;
            }

            if (line.Contains("======="))
            {
                // Separator - start skipping until end marker
                skipUntilEnd = true;
                continue;
            }

            if (line.Contains(">>>>>>>"))
            {
                // End of conflict
                inConflict = false;
                skipUntilEnd = false;
                continue;
            }

            // Keep lines that are not part of the "theirs" section
            if (!skipUntilEnd) cleanedLines.Add(line);
        }

        return string.Join('\n', cleanedLines);
    }

    private void CopyFileFromTo(string pathToFileToCopy, string destinationRoot, string referenceRoot)
    {
        try
        {
            var absoluteFileToCopy = Path.GetFullPath(pathToFileToCopy);
            var absoluteReferenceRoot = Path.GetFullPath(referenceRoot);
            var absoluteDestinationRoot = Path.GetFullPath(destinationRoot);

            if (!absoluteFileToCopy.StartsWith(absoluteReferenceRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                    $"File {absoluteFileToCopy} is not inside the reference root {absoluteReferenceRoot}.");

            var relativePath = Path.GetRelativePath(absoluteReferenceRoot, absoluteFileToCopy);

            // Remove EN_ prefix from the filename in the relative path
            var fileName = Path.GetFileName(relativePath);
            var cleanFileName = fileName.StartsWith("EN_") ? fileName.Substring(3) : fileName;
            var directory = Path.GetDirectoryName(relativePath) ?? "";
            relativePath = Path.Combine(directory, cleanFileName);

            var destinationPath = Path.Combine(absoluteDestinationRoot, relativePath);

            var destinationDirectory = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectory) && !Directory.Exists(destinationDirectory))
                Directory.CreateDirectory(destinationDirectory);

            File.Copy(absoluteFileToCopy, destinationPath, true);
        }
        catch (Exception ex)
        {
            Log.Debug($"Error copying file from {pathToFileToCopy}: {ex.Message}");
            throw;
        }
    }
}