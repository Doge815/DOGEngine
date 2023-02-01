using DOGEngine.Texture;

namespace DOGEngine.Shader;

public class Sprite2DShader : Shader, I2DPositionShader, ITextureCoordShader
{
    public Sprite2DShader() : base("Shader/Shaders/Sprite2DShader.vert", "Shader/Shaders/Sprite2DShader.frag")
    {
        SetInt("texture0", 0);
    }
    public static Position2DShaderAttribute Position2D { get; } = new(0, "pos");
    public static TextureCoordShaderAttribute TextureCoord { get; } = new(2, "tex");

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Position2D, TextureCoord };

    public void UseTextShader(ITexture? texture = null, Matrix4? projection = null)
    {
        base.Use();
        if (texture is not null) texture.Use();
        if(projection is not null) SetMatrix4("projection", projection.Value);
    }
}