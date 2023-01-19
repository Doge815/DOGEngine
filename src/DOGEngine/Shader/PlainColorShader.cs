namespace DOGEngine.Shader;

public class PlainColorShader : Shader, IVertexShader, IModelShader, IViewShader, IProjectionShader
{
    private Vector3 _color;
    public Vector3 Color
    {
        get => _color;
        set
        {
            _color = value;
            SetVector3("aColor", value);
        }
    }
    public PlainColorShader(Vector3 color) : base("Shader/Shaders/BaseShader.vert",
        "Shader/Shaders/PlainColorShader.frag")
        => Color = color;

    public static VertexShaderAttribute Vertex { get; } = new(0, "aPosition");
    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex };
    
    public void SetModel(Matrix4 model) => SetMatrix4("model", model);
    public void SetView(Matrix4 view) => SetMatrix4("view", view);
    public void SetProjection(Matrix4 projection) => SetMatrix4("projection", projection);
}