using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace RainbusToolbox.Services;

public class FileMergingService
{
    //TODO:
    // I have tried to implement this shit with ai, but they are too fucking retarded, so i should rely less on it and write backend myself, frontend could be easily fixed at least in this matter
    // i just have wasted more time punching ai to do this than i would doing this shit myself
    


    public int[] PullFilesFromTheGameAsync(string pathToLocalization, string pathToReferenceLocalization)
    {
        var newFiles = 0;
        var expandedFiles = 0;
        var checkedFiles = 0;
        
        
        var localizationFiles = Directory.GetFiles(pathToLocalization).ToList();
        var referenceFiles = Directory.GetFiles(pathToReferenceLocalization).ToList();
        
        //Here would be foreach cycle at gamefiles,
        // 3 things, if file exist at both places, check for difference, merge
        // TODO: if fine in loc but not in game fart at the user (idk if this is needed tho), tho it will be separate loop. For now forget, will be a tod0
        
        // if file ingame but not here, just copy

        foreach (var referenceFile in referenceFiles)
        {
            checkedFiles++;
            
            var referenceFileNameNoPrefix = Path.GetFileName(GetPathWithoutPrefix(referenceFile));
            var existingFileName = localizationFiles.FirstOrDefault(f => f==referenceFileNameNoPrefix);

            if (string.IsNullOrEmpty(existingFileName))
            {
                //Fun starts. a new file was added into the game, time to add it;
                CopyFileFromTo(referenceFile, pathToLocalization, pathToReferenceLocalization);
                newFiles++;
                continue;
            }
            
            // The hardest part, json fuckery to merge properly. need to research, something about generic jsons? all content is inside quotes so
            // should be no problem with < or [
            var existingFilePath = localizationFiles.First(p => Path.GetFileName(p) == referenceFileNameNoPrefix);
            CastToJsonAndMerge(existingFilePath, referenceFile);
        }

        Console.WriteLine($"Added {newFiles} files, merged {expandedFiles} files.\n\n\nTotal files {checkedFiles}.");
        return [newFiles, expandedFiles, checkedFiles];
    }

    private string GetPathWithoutPrefix(string path)
    {
        var sanitised = Path.Combine(
            Path.GetDirectoryName(path) ?? Path.DirectorySeparatorChar.ToString(),
            Path.GetFileName(path).StartsWith("EN_")
                ? Path.GetFileName(path).Substring(3)
                : Path.GetFileName(path));
        return sanitised;
    }

    private void CastToJsonAndMerge(string destinationPath, string sourcePath)
    {
        // technically path of source shouldn't matter here?
        /*
         * What do i actually have
         * Each json has DataList as root property, with different shit inside
         * all files must be json
         * 
         *
         *
         * 
         */
        //ok so after a bit of thought i have this structure:
        /*
         * each json is basically array DataList with different properties
         * so i do need to get all items inside DataLists of both, and then every object inside data list has ID property, so if theres no
         * matching id inside, i just copy entire shite
         */
        var deserializedDestination = JObject.Parse(File.ReadAllText(destinationPath));
        var deserializedSource = JObject.Parse(File.ReadAllText(sourcePath));
        
        var sourceDataList = deserializedSource["DataList"] as JArray;
        var destinationDataList = deserializedDestination["DataList"] as JArray;

        if (sourceDataList == null)
        {
            App.Current.ShowErrorNotificationAsync(
                "FOR SOME FUCKING REASON THERE ARE EMPTY FILES INSIDE THE GAAAAMEEEEE, PLEASE SEND THIS TO DEV",
                "THE FUCK");
            return;
        }
        
        
        if (destinationDataList == null)
            destinationDataList = sourceDataList;
        else
            foreach (var jToken in sourceDataList)
            {
                var sourceItem = (JObject)jToken;
                var sourceId = sourceItem["id"]?.ToString();
                
                if (string.IsNullOrEmpty(sourceId))
                    continue;

                if (destinationDataList
                        .FirstOrDefault(item => item["id"]?.ToString() == sourceId) is JObject existingItem)
                    foreach (JProperty property in sourceItem.Properties())
                    {
                        if (existingItem[property.Name] == null)
                        {
                            existingItem.Add(property.Name, property.Value);
                        }
                    }
                
                else
                    destinationDataList.Add(sourceItem);
            }
        
        var file = JsonConvert.SerializeObject(deserializedDestination, Formatting.Indented);
        File.WriteAllText(destinationPath, file, new UTF8Encoding(false));
    }

    private void CopyFileFromTo(string pathToFileToCopy, string destinationRoot, string referenceRoot)
    {
        string absoluteFileToCopy = Path.GetFullPath(pathToFileToCopy);
        string absoluteReferenceRoot = Path.GetFullPath(referenceRoot);
        string absoluteDestinationRoot = Path.GetFullPath(destinationRoot);

        if (!absoluteFileToCopy.StartsWith(absoluteReferenceRoot))
        {
            throw new InvalidOperationException("File is not inside the reference root.");
        }

        string relativePath = Path.GetRelativePath(absoluteReferenceRoot, absoluteFileToCopy);

        string destinationPath = Path.Combine(absoluteDestinationRoot, relativePath);

        string destinationDirectory = Path.GetDirectoryName(destinationPath);
        if (!Directory.Exists(destinationDirectory))
            Directory.CreateDirectory(destinationDirectory);

        File.Copy(absoluteFileToCopy, destinationPath, overwrite: true);
    }

}