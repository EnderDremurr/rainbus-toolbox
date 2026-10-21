using RainbusToolbox.Models.LimbusSerialization;
using RainbusToolbox.Utilities;

namespace RainbusToolbox.Tests.Utilities;

public class FileToObjectCasterTests
{
    private static readonly Dictionary<string, string> _mockFileMap = new()
    {
        { "Skills", "skill" },
        { "Egos", "ego" }
    };

    [Theory]
    [InlineData("BattleHint.json", typeof(NormalBattleHintLocalizationFile))]
    [InlineData("BattleHint.JSON", typeof(NormalBattleHintLocalizationFile))]
    [InlineData("/JuicyPussy/BattleHint.json", typeof(NormalBattleHintLocalizationFile))]
    public void GetType_BattleHintFile_ReturnsHardcodedType(string filePath, Type? expected)
    {
        var result = FileToObjectCaster.GetType(filePath, new Dictionary<string, string>());
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("BattleHint.mp3", null)]
    [InlineData("BattleHint", null)]
    [InlineData("BattleHintFarts.json", null)]
    public void GetType_BattleHintFile_ReturnsNull(string filePath, Type? expected)
    {
        var result = FileToObjectCaster.GetType(filePath, new Dictionary<string, string>());
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("StoryData/S06718.json", typeof(StoryDataFile))]
    [InlineData("/localize/StoryData/S06718.json", typeof(StoryDataFile))]
    public void GetType_StoryFile_ReturnsHardcodedType(string filePath, Type? expected)
    {
        var result = FileToObjectCaster.GetType(filePath, new Dictionary<string, string>());
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("S06718.json", null)]
    [InlineData("AO3FanFic.json", null)]
    [InlineData("StoryData/BlackPencil.png", null)]
    [InlineData("FR/CH/JP/EN/StoryData/Amogus.mp4", null)]
    public void GetType_StoryFile_ReturnsNull(string filePath, Type? expected)
    {
        var result = FileToObjectCaster.GetType(filePath, new Dictionary<string, string>());
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Skills.json", typeof(SkillLocalizationFile))]
    [InlineData("Egos.json", typeof(EgoLocalizationFile))]
    public void GetType_TranslationFile_ReturnsTypeFromMap(string filePath, Type? expected)
    {
        var result = FileToObjectCaster.GetType(filePath, _mockFileMap);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Skills.govno", null)]
    [InlineData("Egos", null)]
    [InlineData(" dsfasdfoasidhfuasofd", null)]
    [InlineData("----0231-312-3123-12", null)]
    [InlineData("Penis.json", null)]
    public void GetType_TranslationFile_ReturnsNullWithMap(string filePath, Type? expected)
    {
        var result = FileToObjectCaster.GetType(filePath, _mockFileMap);
        Assert.Equal(expected, result);
    }
}