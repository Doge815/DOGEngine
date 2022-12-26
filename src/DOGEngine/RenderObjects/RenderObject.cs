using DOGEngine.Shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace DOGEngine.RenderObjects;

public abstract class RenderObject
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }

    protected RenderObject(Shader.Shader shader, Vector3? position, Vector3? rotation)
    {
        Shader = shader;
        Position = position ?? Vector3.Zero;
        Rotation = rotation ?? Vector3.Zero;
    }

    public virtual Shader.Shader Shader { get; }

    public abstract void OnLoad();
    public abstract void Draw(Matrix4 view, Matrix4 projection);

    protected void interpretVertexData(string attribute, int size, VertexAttribPointerType type, int stride, int offset)
    {
        int index = Shader.GetAttributeLocation(attribute);
        if (index != -1)
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, VertexAttribPointerType.Float, false, stride, offset);
        }
    }

    protected void interpretVertexDataFloat(IShaderAttribute attribute, int stride)
    {
        interpretVertexData(attribute.AttributeName, attribute.Size, VertexAttribPointerType.Float,
            stride * sizeof(float),
            attribute.Offset * sizeof(float));
    }
}

public abstract class VertexRenderObject : RenderObject
{
    private int vertexArrayObj;
    private readonly float[] vertices;
    private readonly int triangles;
    
    public override void OnLoad()
    {
        int vertexBufferObj = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObj);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        vertexArrayObj = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObj);

        foreach (IShaderAttribute attribute in Shader.Attributes)
            interpretVertexDataFloat(attribute, Shader.Stride);
    }
    public override void Draw(Matrix4 view, Matrix4 projection)
    {
        Shader.Use();
        Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Rotation.X))
                        * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Rotation.Y))
                        * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Rotation.Z))
                        * Matrix4.CreateTranslation(Position);

        Shader.SetMatrix4("model", model);
        Shader.SetMatrix4("view", view);
        Shader.SetMatrix4("projection", projection);

        GL.BindVertexArray(vertexArrayObj);
        GL.DrawArrays(PrimitiveType.Triangles, 0, triangles);
    }

    protected VertexRenderObject(Shader.Shader shader, Vector3? position, Vector3? rotation, VertexDataBundle? data) : base(shader, position, rotation)
    {
        vertices = data?.CreateVertices(shader) ?? Array.Empty<float>();
        triangles = data?.Rows ?? 0;
    }
}

public readonly struct VertexDataBundle
{
    private Dictionary<Type, float[]> Data { get; }
    public int Rows { get; }
    public float[] CreateVertices(Shader.Shader shader)
    {
        int columns = 0;
        foreach (IShaderAttribute attribute in shader.Attributes)
        {
            columns += attribute.Size;
        }

        float[] verts = new float[columns * Rows];
        foreach (IShaderAttribute attribute in shader.Attributes)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j <  attribute.Size; j++)
                {
                    int writePosition = i * columns + j + attribute.Offset;
                    if (Data.TryGetValue(attribute.GetType(), out var attributeData))
                        verts[writePosition] = attributeData[i * attribute.Size + j];
                    else
                        verts[writePosition] = 0;
                }
            }
        }
        return verts;
    }

    public VertexDataBundle(Dictionary<Type, float[]> data, int rows)
    {
        Data = data;
        Rows = rows;
    }
}