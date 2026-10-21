using RainbusToolbox.Services.RepositoryServices;

namespace RainbusToolbox.Tests.Services.RepositoryServices;

public class KeywordProcessingServiceTests
{
    [Theory]
    [InlineData(
        "<sprite name=\\\"ParryingResultDown\\\"><color=#e30000><u><link=\\\"ParryingResultDown\\\">Понижение силы в столкновении</link></u></color>",
        "[ParryingResultDown:*Понижение силы в столкновении*]")]
    [InlineData("<sprite name=\\\"Agility\\\"><color=#f8c200><u><link=\\\"Agility\\\">Спешки</link></u></color>",
        "[Agility:*Спешки*]")]
    [InlineData(
        "<sprite name=\\\"Breath\\\"><color=#f8c200><u><link=\\\"Breath\\\"><noparse>Дыхание</noparse>  </link></u></color>",
        "[Breath:*Дыхание*]")]
    public void GetType_RevertMeshToTag_ReturnsProperTag(string mesh, string expected)
    {
        var result = KeywordProcessingService.RevertMeshToTag(mesh);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("RandomShit")]
    [InlineData("[Agility]")] // tags without translation text are not converted
    [InlineData(
        "<sprite name=\\\"Breath\\\"><color=#f8c200><u><link=\\\"Suffocation\\\"><noparse>Дыхание</noparse></link></u></color>")] // returns the same cuz link and sprite are different
    public void GetType_RevertMeshToTag_ReturnsOriginal(string mesh)
    {
        var result = KeywordProcessingService.RevertMeshToTag(mesh);
        Assert.Equal(mesh, result);
    }
}