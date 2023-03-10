using DOGEngine.Texture;

namespace DOGEngine.Shader;

public class TextShader : Shader, I2DPositionShader, ITextureCoordShader
{
    public TextShader() : base("Shader/Shaders/TextShader.vert", "Shader/Shaders/TextShader.frag")
    {
        SetInt("text", 0);
    }
    public static Position2DShaderAttribute Position2D { get; } = new(0, "pos");
    public static TextureCoordShaderAttribute TextureCoord { get; } = new(2, "tex");

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Position2D, TextureCoord };

    public void UseTextShader(ITexture? texture = null, Vector3? color = null, Matrix4? projection = null)
    {
        base.Use();
        if (texture is not null) texture.Use();
        if(color is not null) SetVector3("textColor", color.Value);
        if(projection is not null) SetMatrix4("projection", projection.Value);
    }
}