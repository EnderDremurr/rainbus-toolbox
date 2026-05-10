using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RainbusToolbox.Models.Managers;
using WeCantSpell.Hunspell;

namespace RainbusToolbox.Services.ExternalServices;

public class SpellCheckerService
{
    private readonly string _cspellPath;
    private HashSet<string> _customWords = new(StringComparer.OrdinalIgnoreCase);
    private WordList? _wordList;

    public SpellCheckerService(RepositoryManager repositoryManager)
    {
        _cspellPath = repositoryManager.PathToVSCodeSettings;
        LoadCSpell(_cspellPath);
        LoadEmbeddedDictionary();
    }

    public void LoadCSpell(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return;

        var json = File.ReadAllText(jsonPath);
        var root = JsonConvert.DeserializeObject<JObject>(json);

        if (root?["cSpell.words"] is not JArray wordsArray)
        {
            Log.Debug("cSpell.words property not found");
            return;
        }

        _customWords = new HashSet<string>(
            wordsArray
                .Select(t => t.Value<string>())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => Normalize(s!)),
            StringComparer.OrdinalIgnoreCase);
    }

    public void AddWordToCSpell(string word)
    {
        var normalized = Normalize(word);


        _customWords.Add(normalized);

        var json = File.Exists(_cspellPath) ? File.ReadAllText(_cspellPath) : "{}";
        var root = JsonConvert.DeserializeObject<JObject>(json) ?? new JObject();

        if (root["cSpell.words"] is not JArray wordsArray)
        {
            wordsArray = new JArray();
            root["cSpell.words"] = wordsArray;
        }

        var exists = wordsArray.Any(t =>
            string.Equals(t.Value<string>(), normalized, StringComparison.OrdinalIgnoreCase));
        if (!exists)
            wordsArray.Add(normalized);

        File.WriteAllText(_cspellPath, root.ToString(Formatting.Indented));
    }

    private void LoadEmbeddedDictionary()
    {
        var assembly = Assembly.GetExecutingAssembly();

        const string affName =
            "RainbusToolbox.Assets.Dictionaries.ru_RU.aff";

        const string dicName =
            "RainbusToolbox.Assets.Dictionaries.ru_RU.dic";

        using var affStream = assembly.GetManifestResourceStream(affName);
        using var dicStream = assembly.GetManifestResourceStream(dicName);

        if (affStream is null || dicStream is null)
            throw new FileNotFoundException(
                "Embedded Hunspell dictionary not found =(");

        _wordList = WordList.CreateFromStreams(dicStream, affStream);
    }

    public bool Check(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return true;

        if (_customWords.Contains(Normalize(word))) return true;


        return _wordList?.Check(word) ?? true;
    }

    public IEnumerable<string> Suggest(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
            return [];

        return _wordList?.Suggest(word) ?? [];
    }

    private static string Normalize(string word)
    {
        return word.Trim().ToLowerInvariant();
    }
}