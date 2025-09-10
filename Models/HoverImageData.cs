public class HoverImageData
{
    public string Normal { get; set; }
    public string Hover { get; set; }

    public static readonly HoverImageData Settings = new() { Normal="/Assets/NavButtonSettings.png", Hover="/Assets/NavButtonSettings-Hov.png" };
    public static readonly HoverImageData Minimize = new() { Normal="/Assets/NavButtonMin.png", Hover="/Assets/NavButtonMin-Hov.png" };
    public static readonly HoverImageData Maximize = new() { Normal="/Assets/NavButtonMax.png", Hover="/Assets/NavButtonMax-Hov.png" };
    public static readonly HoverImageData Close = new() { Normal="/Assets/NavButtonClose.png", Hover="/Assets/NavButtonClose-Hov.png" };
}