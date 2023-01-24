using DOGEngine.Shader;

namespace DOGEngine.RenderObjects.Properties;

public partial class Mesh : GameObject, IPostInitializedGameObject
{
    private int VAO; 
    private int triangles;

    private readonly VertexDataBundle tempData;
    private bool createCollider;
    public Mesh(VertexDataBundle data, bool createColliderIfNotExistent = true)
    {
        tempData = data;
        VAO = -1;
        createCollider = createColliderIfNotExistent;
    }
    
    internal float[] VertexData => tempData.Data[typeof(VertexShaderAttribute)];

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

        if (!Parent.TryGetComponent(out Collider _))
        {
            Parent.AddComponent(new Collider());
        }
    }

    public void Draw(Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
    {
        if(VAO == -1) return;
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        shader.Use();

        if (shader is IModelShader modelShader)
            modelShader.SetModel(GetModel(this));
        if(shader is IViewShader viewShader)
            viewShader.SetView(view);
        if(shader is IProjectionShader projectionShader)
            projectionShader.SetProjection(projection);
        if(shader is ICameraPosShader cameraPosShader)
            cameraPosShader.SetCameraPos(cameraPosition);

        GL.BindVertexArray(VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, triangles);
    }

    private static readonly Matrix4 defaultModel =
        Transform.CreateModelMatrix(Vector3.One, Vector3.Zero, Vector3.Zero, Vector3.Zero);
    internal static Matrix4 GetModel(GameObject obj) => obj.Parent.TryGetComponent(out Transform? transform) ? transform!.Model : defaultModel;

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