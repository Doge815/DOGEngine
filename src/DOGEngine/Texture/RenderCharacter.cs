namespace DOGEngine.Texture;

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