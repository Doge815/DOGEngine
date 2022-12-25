using OpenTK.Graphics.OpenGL4;

namespace DOGEngine;

public class TextureShader : Shader, IVertexShader, ITextureCoordShader
{
    public TextureShader(Texture texture) : base("../../../../DOGEngine/Shaders/TextureShader.vert", "../../../../DOGEngine/Shaders/TextureShader.frag")
    {
        Texture = texture;
        SetInt("texture0", 0);
    }

    public Texture Texture { get; }

    public int Stride => Attributes.Sum(x => x.Size);
    public static VertexShaderAttribute Vertex { get; } = new(0, "aPosition");
    public static TextureCoordShaderAttribute TextureCoord { get; } = new(3, "aTexCoord");

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex, TextureCoord };
    public override void Use()
    {
        base.Use();
        Texture.Use(TextureUnit.Texture0);
    }
}