using OpenTK.Mathematics;

namespace DOGEngine.Shader;

public class CubeMapShader : Shader, IVertexShader, IModelShader, IViewShader, IProjectionShader
{
    public CubeMapShader() : base("Shader/Shaders/CubeMapShader.vert", "Shader/Shaders/CubeMapShader.frag") { }

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex };
    public static VertexShaderAttribute Vertex { get; } = new VertexShaderAttribute(0, "aPosition");
    
    public void SetModel(Matrix4 model) => SetMatrix4("model", model);
    public void SetView(Matrix4 view) => SetMatrix4("view", view);
    public void SetProjection(Matrix4 projection) => SetMatrix4("projection", projection);
}