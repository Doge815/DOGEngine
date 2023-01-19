namespace DOGEngine.Shader;

public class TextureShader : Shader, IVertexShader, ITextureCoordShader, IModelShader, IViewShader, IProjectionShader
{
    public TextureShader(Texture.ITexture texture) : base("Shader/Shaders/BaseShader.vert", "Shader/Shaders/TextureShader.frag")
    {
        Texture = texture;
        SetInt("texture0", 0);
    }

    public Texture.ITexture Texture { get; }

    public static VertexShaderAttribute Vertex { get; } = new(0, "aPosition");
    public static TextureCoordShaderAttribute TextureCoord { get; } = new(3, "aTexCoord");

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex, TextureCoord };
    public override void Use()
    {
        base.Use();
        Texture.Use();
    }
    
    public void SetModel(Matrix4 model) => SetMatrix4("model", model);
    public void SetView(Matrix4 view) => SetMatrix4("view", view);
    public void SetProjection(Matrix4 projection) => SetMatrix4("projection", projection);
}