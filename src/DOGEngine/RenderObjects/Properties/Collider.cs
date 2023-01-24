using DOGEngine.Shader;

namespace DOGEngine.RenderObjects.Properties;

public enum PhysicsSimulationType
{
    None = 0,
    Static = 1,
    Dynamic = 2
}

public struct PhysicsType //I want rust enums with values
{
    public PhysicsSimulationType Type { get; init; }
    public float Mass { get; init; }

    public static PhysicsType CreateNone() => new PhysicsType() { Type = PhysicsSimulationType.None};
    public static PhysicsType CreatePassive() => new PhysicsType() { Type = PhysicsSimulationType.Static };
    public static PhysicsType CreateActive(float mass) => new PhysicsType() { Type = PhysicsSimulationType.Dynamic, Mass = mass};
}


public class Collider : GameObject, IPostInitializedGameObject
{
    public PhysicsType PhysicsType { get; }
    public Collider(string file, PhysicsType? physicsType = null)
    {
        colliderVertexData = Mesh.FromFile(file, true).Data[typeof(VertexShaderAttribute)];
        PhysicsType = physicsType ?? PhysicsType.CreateNone();
        addToPhysicsEngine();
    }

    public Collider(PhysicsType? physicsType = null) //use the mesh as the collider
    {
        colliderVertexData = null;
        PhysicsType = physicsType ?? PhysicsType.CreateNone();
        addToPhysicsEngine();
    }

    private void addToPhysicsEngine()
    {
        if (Root.TryGetComponent(out Physics.Physics? physics))
        {
            if (PhysicsType.Type == PhysicsSimulationType.Dynamic)
            {
                physics!.CreateDynamic(PhysicsType.Mass);
            }
            else
            {
                
            }
        }
    }

    private float[]? colliderVertexData;
    public float[] ColliderVertexData => colliderVertexData!;
    public bool NotInitialized { get; set; } = true;
    public void InitFunc() => colliderVertexData ??= Parent.GetComponent<Mesh>().VertexData;
}