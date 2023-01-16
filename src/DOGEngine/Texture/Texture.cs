using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace DOGEngine.Texture;
public interface ITexture
{
    public void Use();
}

public class Texture : ITexture
{
    public Texture(string texturePath)
    {
        Handle = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        StbImage.stbi_set_flip_vertically_on_load(1);
        ImageResult image = ImageResult.FromStream(File.OpenRead(texturePath), ColorComponents.RedGreenBlueAlpha);
        StbImage.stbi_set_flip_vertically_on_load(0);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
            PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
    }
    private int Handle { get; }

    public void Use(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
    public void Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }
}

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
public class RenderCharacter : ITexture
{
    private int Handle { get; init; }
    public Vector2 Size { get; init; }
    public Vector2 Bearing { get; init; }
    public uint Offset { get; init; }

    public void Use()
    {
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public RenderCharacter(int handle, Vector2 size, Vector2 bearing, uint offset)
    {
        Handle = handle;
        Size = size;
        Bearing = bearing;
        Offset = offset;
    }
}