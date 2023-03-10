using DOGEngine.GameObjects;
using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Properties.Mesh;
using DOGEngine.Shader;

namespace DOGEngine.GameObjects.Properties.Mesh;

public class Mesh : GameObject, IPostInitializedGameObject
{
    public IMeshSupplier MeshData { get; set; }
    private readonly bool createCollider;

    public Mesh(IMeshSupplier meshData, bool createColliderIfNotExistent = true)
    {
        MeshData = meshData;
        createCollider = createColliderIfNotExistent;
    }

    public Mesh(VertexDataBundle data, bool createColliderIfNotExistent = true)
    {
        MeshData = new TriangleMesh(data);
        createCollider = createColliderIfNotExistent;
    }

    public Mesh(IMeshSupplier meshData, Collider.Collider collider) : base(collider)
    {
        MeshData = meshData;
    }

    public Mesh(VertexDataBundle data, Collider.Collider collider) : base(collider)
    {
        MeshData = new TriangleMesh(data);
    }
    public bool Hidden { get; set; }

    public bool NotInitialized { get; set; } = true;

    public void InitFunc()
    {
        MeshData.Initialize(Parent.GetComponent<Shading>().Shader);

        if (!TryGetComponent(out Collider.Collider _) && createCollider)
            AddComponent(new Collider.Collider());
    }

    public void Draw(Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
    {
        if(Hidden) return;
        if (!MeshData.Initialized) return;
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        shader.Use();

        if (shader is IModelShader modelShader)
            modelShader.SetModel(GetModel(this));
        if (shader is IViewShader viewShader)
            viewShader.SetView(view);
        if (shader is IProjectionShader projectionShader)
            projectionShader.SetProjection(projection);
        if (shader is ICameraPosShader cameraPosShader)
            cameraPosShader.SetCameraPos(cameraPosition);

        MeshData.Draw();
    }

    internal static Matrix4 GetModel(GameObject obj) => obj.Parent.TryGetComponent(out Transform? transform)
        ? transform!.Model
        : TransformData.Default.Model;
}