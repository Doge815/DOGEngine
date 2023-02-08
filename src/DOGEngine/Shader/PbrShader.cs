using DOGEngine.Texture;

namespace DOGEngine.Shader;

public class PbrShader : Shader, IVertexShader, INormalShader, ITextureCoordShader, IModelShader, IViewShader, IProjectionShader, ICameraPosShader, ITextureScaleShader
{
    public PbrShader(PbrTextureCollection texture, Vector2? scale = null) : base("Shader/Shaders/PbrShader.vert",
        "Shader/Shaders/PbrShader.frag")
    {
        Texture = texture;
        SetInt("albedoMap", 0);
        SetInt("normalMap", 1);
        SetInt("metallicMap", 2);
        SetInt("roughnessMap", 3);
        SetInt("aoMap", 4);
        Scale = scale ?? Vector2.One;
    }
    public ITexture Texture { get; }
    public static VertexShaderAttribute Vertex { get; } = new(0, "aPosition");
    public static NormalShaderAttribute Normal { get; } = new NormalShaderAttribute(3, "aNormal");
    public static TextureCoordShaderAttribute TextureCoord { get; } = new(6, "aTexCoord");

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex, Normal, TextureCoord };
    public override void Use()
    {
        base.Use();
        Texture.Use();
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
    
    public void SetModel(Matrix4 model) => SetMatrix4("model", model);
    public void SetView(Matrix4 view) => SetMatrix4("view", view);
    public void SetProjection(Matrix4 projection) => SetMatrix4("projection", projection);
    public void SetCameraPos(Vector3 cameraPos) => SetVector3("camPos", cameraPos);
}