using DOGEngine.Shader;
using OpenTK.Mathematics;

namespace DOGEngine.RenderObjects;

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
    public Cube(Shader.Shader shader, Vector3? position = null, Vector3? orientation = null, Vector3? scale = null, Vector3? orientationOffset = null) : base(shader, position, orientation, scale, orientationOffset, data) {}
}