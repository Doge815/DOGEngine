using DOGEngine.Shader;

namespace DOGEngine.RenderObjects.Properties.Mesh;

public interface IMeshSupplier
{
    public float[] VertexData { get; }
    public void Initialize(Shader.Shader shader);
    public bool Initialized { get; }
    public void Draw();
}

public partial class TriangleMesh : IMeshSupplier //simple mesh stored in a VAO (no EBO)
{
    private int VAO; 
    private int triangles;
    private readonly VertexDataBundle tempData;
    public bool Initialized { get; private set; }
    
    public TriangleMesh(VertexDataBundle data)
    {
        tempData = data;
        VAO = -1;
        Initialized = false;
    }

    public void Draw()
    {
        GL.BindVertexArray(VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, triangles);
    }
    
    public float[] VertexData => tempData.Data[typeof(VertexShaderAttribute)];

    public void Initialize(Shader.Shader shader)
    {
        if (Initialized) return;
        Initialized = true;
        
        var vertices = tempData.CreateVertices(shader);
        triangles = tempData.Rows;
        
        int vertexBufferObj = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObj);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);

        foreach (IShaderAttribute attribute in shader.Attributes)
            interpretVertexDataFloat(shader, attribute, shader.Stride);
    }
    private void interpretVertexData(Shader.Shader shader, string attribute, int size, VertexAttribPointerType type, int stride, int offset)
    {
        int index = shader.GetAttributeLocation(attribute);
        if (index != -1)
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, type, false, stride, offset);
        }
    }

    private void interpretVertexDataFloat(Shader.Shader shader, IShaderAttribute attribute, int stride)
    {
        interpretVertexData(shader, attribute.AttributeName, attribute.Size, VertexAttribPointerType.Float,
            stride * sizeof(float),
            attribute.Offset * sizeof(float));
    }
}