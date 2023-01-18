using DOGEngine.Shader;

namespace DOGEngine.RenderObjects.Properties;

public class Collider : GameObject, IColliderVertexDataSupplier
{
    public Collider(string file)
    {
        ColliderVertexData = Mesh.FromFile(file, true).Data[typeof(VertexShaderAttribute)];
    }
    public float[] ColliderVertexData { get; }
}