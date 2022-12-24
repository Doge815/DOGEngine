using OpenTK.Graphics.OpenGL4;

namespace DOGEngine;

public class TextureShader : Shader
{
    public TextureShader(Texture texture) : base("../../../../DOGEngine/Shaders/TextureShader.vert", "../../../../DOGEngine/Shaders/TextureShader.frag")
    {
        Texture = texture;
        SetInt("texture0", 0);
    }

    public Texture Texture { get; }

    public virtual int Stride => 5;
    public virtual ShaderAttribute Vertex => new() { AttributeName = "aPosition", Offset = 0, Size = 3 };
    public virtual ShaderAttribute TextureCoord => new() { AttributeName = "aTexCoord", Offset = 3, Size = 2 };

    public override void Use()
    {
        base.Use();
        Texture.Use(TextureUnit.Texture0);
    }
}