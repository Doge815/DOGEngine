namespace DOGEngine.RenderObjects.Text;

public struct Font
{
    public static string DejaVuSans { get; } = "/usr/share/fonts/TTF/DejaVuSans.ttf";
    public string FontFile { get; init; }
    public uint Height { get; set; }

    public Font(string fontFile, uint height)
    {
        FontFile = fontFile;
        Height = height;
    }
}