using BulletSharp;
using BulletSharp.Math;
using DOGEngine.RenderObjects.Properties.Mesh;
using DOGEngine.Shader;
using TriangleMesh = BulletSharp.TriangleMesh;
using Vector3 = BulletSharp.Math.Vector3;

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


public enum ColliderType
{
    Box,
    Mesh,
}
public class Collider : GameObject, IPostInitializedGameObject
{
    public PhysicsType PhysicsType { get; }
    public Collider(string file, PhysicsType? physicsType = null, params IColliderWrapper[] colliders)
    {
        colliderVertexData = Mesh.TriangleMesh.FromFile(file, true).Data[typeof(VertexShaderAttribute)];
        PhysicsType = physicsType ?? PhysicsType.CreateNone();
        physicsColliders = colliders;

    }

    public Collider(PhysicsType? physicsType = null, params IColliderWrapper[] colliders) //use the mesh as the collider
    {
        colliderVertexData = null;
        PhysicsType = physicsType ?? PhysicsType.CreateNone();
        physicsColliders = colliders;
    }

    private void addToPhysicsEngine()
    {
        void SetTranslation(Transform transform, Matrix translation) => transform.TransformData.SetModel(translation.Convert(), false, true, true);

        CollisionShape CreateCollider()
        {
            Parent.Parent.TryGetComponent(out Transform? transform);
            var scale = (transform?.TransformData ?? TransformData.Default).Scale;
            if (physicsColliders.Length == 0)
                return IColliderWrapper.Combine(scale, new MeshCollider(ColliderVertexData));
            return IColliderWrapper.Combine(scale, physicsColliders);
        }
        if (Root.TryGetComponent(out Physics.Physics? physics))
        {
            if (PhysicsType.Type == PhysicsSimulationType.Dynamic)
            {
                if(Parent.Parent.TryGetComponent(out Transform? transform)) 
                    physics!.Create(CreateCollider(), true, PhysicsType.Mass, transform!.TransformData, matrix => SetTranslation(transform, matrix));
                else
                    physics!.Create(CreateCollider(), true, PhysicsType.Mass, TransformData.Default, null);
            }
            else if (PhysicsType.Type == PhysicsSimulationType.Static)
            {
                if(Parent.Parent.TryGetComponent(out Transform? transform)) 
                    physics!.Create(CreateCollider(), false, 0, transform!.TransformData, null);
                else
                    physics!.Create(CreateCollider(), false, 0, TransformData.Default, null);
            }
        }
    }

    private float[]? colliderVertexData;
    private readonly IColliderWrapper[] physicsColliders;
    public float[] ColliderVertexData => colliderVertexData!;
    public bool NotInitialized { get; set; } = true;
    public void InitFunc()
    {
        if (colliderVertexData is null)
        {
            if (Parent is Mesh.Mesh mesh) colliderVertexData = mesh.MeshData.VertexData;
            else throw new AggregateException("Parent must be mesh or vertex data must be supplied");
        }
        addToPhysicsEngine();
    }
}