using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace DOGEngine;

public class Cube : VertexRenderObject
{
    private static readonly VertexDataBundle data = new VertexDataBundle(new Dictionary<Type, float[]>()
    {
        { typeof(VertexShaderAttribute), new[]
            {
                -0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, 0.5f, -0.5f, 
                0.5f, 0.5f, -0.5f, 
                -0.5f, 0.5f, -0.5f, 
                -0.5f, -0.5f, -0.5f, 

                -0.5f, -0.5f, 0.5f, 
                0.5f, -0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, 0.5f, 
                -0.5f, -0.5f, 0.5f, 

                -0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, -0.5f, 
                -0.5f, -0.5f, -0.5f, 
                -0.5f, -0.5f, -0.5f, 
                -0.5f, -0.5f, 0.5f, 
                -0.5f, 0.5f, 0.5f, 

                0.5f, 0.5f, 0.5f, 
                0.5f, 0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 

                -0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, -0.5f, 
                0.5f, -0.5f, 0.5f, 
                0.5f, -0.5f, 0.5f, 
                -0.5f, -0.5f, 0.5f, 
                -0.5f, -0.5f, -0.5f, 

                -0.5f, 0.5f, -0.5f, 
                0.5f, 0.5f, -0.5f, 
                0.5f, 0.5f, 0.5f, 
                0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, 0.5f, 
                -0.5f, 0.5f, -0.5f, 
            }
        },
        { typeof(TextureCoordShaderAttribute), new[]
            {
                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,

                0.0f, 0.0f,
                1.0f, 0.0f,
                1.0f, 1.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,

                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,
                1.0f, 0.0f,

                1.0f, 0.0f,
                1.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 1.0f,
                0.0f, 0.0f,
                1.0f, 0.0f,

                0.0f, 1.0f,
                1.0f, 1.0f,
                1.0f, 0.0f,
                1.0f, 0.0f,
                0.0f, 0.0f,
                0.0f, 1.0f,

                0.0f, 1.0f,
                1.0f, 1.0f,
                1.0f, 0.0f,
                1.0f, 0.0f,
                0.0f, 0.0f,
                0.0f, 1.0f
            }
        },
    }, 36);
    public Cube(TextureShader shader, Vector3? position = null, Vector3? rotation = null) : base(shader, position, rotation, data)
        => Shader = shader;

    public override TextureShader Shader { get; }

    public override void OnLoad()
    {
        base.OnLoad();
        interpretVertexDataFloat(TextureShader.Vertex, Shader.Stride);
        interpretVertexDataFloat(TextureShader.TextureCoord, Shader.Stride);
    }

}