using OpenTK.Mathematics;

namespace DOGEngine.Shader;

public class PlainColorShader : Shader, IVertexShader
{
    public PlainColorShader(Vector3 color) : base("../../../../DOGEngine/Shader/Shaders/BaseShader.vert",
        "../../../../DOGEngine/Shader/Shaders/PlainColorShader.frag")
        => SetVector3("aColor", color);

    public static VertexShaderAttribute Vertex { get; } = new(0, "aPosition");
    public override IShaderAttribute[] Attributes { get; } = new IShaderAttribute[] { Vertex };
}