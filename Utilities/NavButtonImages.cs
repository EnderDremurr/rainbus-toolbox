namespace RainbusTools.Converters;

public class NavButtonImages
{
    public string NormalImage { get; set; } = "";
    public string HoverImage { get; set; } = "";

    public static NavButtonImages Close => new() { NormalImage="/Assets/NavButtonClose.png", HoverImage="/Assets/NavButtonCloseHov.png" };
    public static NavButtonImages Minimize => new() { NormalImage="/Assets/NavButtonMin.png", HoverImage="/Assets/NavButtonMinHov.png" };
    public static NavButtonImages Maximize => new() { NormalImage="/Assets/NavButtonMax.png", HoverImage="/Assets/NavButtonMaxHov.png" };
    public static NavButtonImages Settings => new() { NormalImage="/Assets/NavButtonSettings.png", HoverImage="/Assets/NavButtonSettingsHov.png" };
}