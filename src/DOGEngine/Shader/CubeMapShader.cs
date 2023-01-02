namespace DOGEngine.Shader;

public class CubeMapShader : Shader, IVertexShader
{
    public CubeMapShader() : base("../../../../DOGEngine/Shader/Shaders/CubeMapShader.vert", "../../../../DOGEngine/Shader/Shaders/CubeMapShader.frag") { }

    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex };
    public static VertexShaderAttribute Vertex { get; } = new VertexShaderAttribute(0, "aPosition");
}