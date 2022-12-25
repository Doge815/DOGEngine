using OpenTK.Graphics.OpenGL4;

namespace DOGEngine.Shader;

public class TextureShader : Shader, IVertexShader, ITextureCoordShader
{
    public TextureShader(Texture.Texture texture) : base("../../../../DOGEngine/Shader/Shaders/BaseShader.vert", "../../../../DOGEngine/Shader/Shaders/TextureShader.frag")
    {
        Texture = texture;
        SetInt("texture0", 0);
    }

    public Texture.Texture Texture { get; }

    public static VertexShaderAttribute Vertex { get; } = new(0, "aPosition");
    public static TextureCoordShaderAttribute TextureCoord { get; } = new(3, "aTexCoord");

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex, TextureCoord };
    public override void Use()
    {
        base.Use();
        Texture.Use(TextureUnit.Texture0);
    }
}