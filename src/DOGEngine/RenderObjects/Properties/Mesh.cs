using DOGEngine.Shader;

namespace DOGEngine.RenderObjects.Properties;

public interface IColliderVertexDataSupplier
{
    public float[] ColliderVertexData { get; }
}
public partial class Mesh : GameObject, IPostInitializedGameObject, IColliderVertexDataSupplier
{
    private int VAO; 
    private int triangles;

    private readonly VertexDataBundle tempData;
    public Mesh(VertexDataBundle data)
    {
        tempData = data;
        VAO = -1;
    }
    
    public float[] ColliderVertexData {
        get
        {
            if (Parent.TryGetComponent(out Collider? collider))
                return collider!.ColliderVertexData;
            return tempData.Data[typeof(VertexShaderAttribute)];
        }
    }

    public bool NotInitialized { get; set; } = true;

    public void InitFunc()
    {
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
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

    public void Draw(Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
    {
        if(VAO == -1) return;
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        shader.Use();

        if (shader is IModelShader modelShader)
            modelShader.SetModel(CreateModelMatrix());
        if(shader is IViewShader viewShader)
            viewShader.SetView(view);
        if(shader is IProjectionShader projectionShader)
            projectionShader.SetProjection(projection);
        if(shader is ICameraPosShader cameraPosShader)
            cameraPosShader.SetCameraPos(cameraPosition);

        GL.BindVertexArray(VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, triangles);
    }

    internal (Vector3 Position, Vector3 Orientation, Vector3 Scale, Vector3 OrientationOffset) GetTransformData()
    {
        if (Parent.TryGetComponent(out Transform? transform))
            return (transform!.Position, transform.Orientation, transform.Scale, transform.OrientationOffset);
        return (Vector3.Zero, Vector3.Zero, Vector3.One, Vector3.Zero);
    }

    internal Matrix4 CreateModelMatrix()
    {
        (Vector3 Position, Vector3 Orientation, Vector3 Scale, Vector3 OrientationOffset)  = GetTransformData();
        return Matrix4.CreateScale(Scale)
               * Matrix4.CreateTranslation(OrientationOffset)
               * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Orientation.X))
               * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Orientation.Y))
               * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Orientation.Z))
               * Matrix4.CreateTranslation(-OrientationOffset)
               * Matrix4.CreateTranslation(Position);
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