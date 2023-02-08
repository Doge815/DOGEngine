using Vector3 = BulletSharp.Math.Vector3;

namespace DOGEngine.Shader;

public class TextureShader : Shader, IVertexShader, ITextureCoordShader, IModelShader, IViewShader, IProjectionShader, ITextureScaleShader
{
    public TextureShader(Texture.ITexture texture, Vector2? scale = null) : base("Shader/Shaders/BaseShader.vert", "Shader/Shaders/TextureShader.frag")
    {
        Texture = texture;
        SetInt("texture0", 0);
        Scale = scale ?? Vector2.One;
    }

    private Vector2 _scale;

    public Vector2 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            SetVector2("scale", _scale);
        }
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