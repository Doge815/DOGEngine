using BulletSharp;
using BulletSharp.Math;
using DOGEngine.Shader;

namespace DOGEngine.RenderObjects.Properties.Mesh.Collider;

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
public class Collider : GameObject, IPostInitializedGameObject, IDeletableGameObject
{
    public PhysicsType PhysicsType { get; }
    public Collider(string file, PhysicsType? physicsType = null, Transform? transform = null, bool stayActive = false, params IColliderWrapper[] colliders)
    {
        ColliderVertexData = Properties.Mesh.TriangleMesh.FromFile(file, true).Data[typeof(VertexShaderAttribute)];
        PhysicsType = physicsType ?? PhysicsType.CreateNone();
        physicsColliders = colliders;
        suppliedTransform = transform;
        keepActive = stayActive;
    }

    public Collider(PhysicsType? physicsType = null,Transform? transform = null, bool stayActive = false, params IColliderWrapper[] colliders) //use the mesh as the collider
    {
        ColliderVertexData = null;
        PhysicsType = physicsType ?? PhysicsType.CreateNone();
        physicsColliders = colliders;
        suppliedTransform = transform;
        keepActive = stayActive;
    }

    private readonly Transform? suppliedTransform;

    private Transform? _transform
    {
        get
        {
            if (suppliedTransform is not null) return suppliedTransform;
            if (Parent.TryGetComponent(out Transform? transform1)) return transform1;
            if (Parent is Properties.Mesh.Mesh mesh && mesh.Parent.TryGetComponent(out Transform? transform2)) return transform2;
            return null;
        }
    }

    internal CollisionObject? physicsObj;

    public void DisablePhysics()
    {
        if (physicsObj is not null && Root.TryGetComponent(out Physics.Physics? physics))
        {
            foreach (var collisions in physics!.GetAllColliding(this))
                if (collisions.physicsObj is not null)
                {
                    collisions.physicsObj.Activate();
                }
            physics.Remove(physicsObj);
            physicsObj.Dispose();
            physicsObj = null;
        }

        if (_transform is not null)
        {
            try
            {
                _transform.TransformChanged -= updateColliderPosition;
            }
            catch
            {
                //new transform ig, idk
            }
        }
    }

    private readonly bool keepActive;
    public void EnablePhysics()
    {
        if (physicsObj is not null) return;
        void SetTranslation(Transform transform, Matrix translation) => transform.TransformData.SetModel(translation.Convert(), false, true, true);

        CollisionShape CreateCollider()
        {
            Parent.Parent.TryGetComponent(out Transform? transform);
            var scale = (transform?.TransformData ?? TransformData.Default).Scale;
            return physicsColliders.Length == 0
                ? ColliderVertexData is not null
                    ? IColliderWrapper.Combine(scale, new MeshCollider(ColliderVertexData))
                    : IColliderWrapper.Combine(scale)
                : IColliderWrapper.Combine(scale, physicsColliders);
        }
        if (Root.TryGetComponent(out Physics.Physics? physics))
        {
            if (PhysicsType.Type == PhysicsSimulationType.Dynamic) 
            {
                if (_transform is not  null)
                {
                    physicsObj = physics!.Create(CreateCollider(), true, PhysicsType.Mass, _transform!.TransformData,
                        (this, matrix => SetTranslation(_transform, matrix)), keepActive);
                    _transform.TransformChanged += updateColliderPosition;
                }
                else
                    physicsObj = physics!.Create(CreateCollider(), true, PhysicsType.Mass, TransformData.Default,
                        (this, null), keepActive);
            }
            else if (PhysicsType.Type == PhysicsSimulationType.Static)
            {
                if (_transform is not null)
                {
                    physicsObj = physics!.Create(CreateCollider(), false, 0, _transform!.TransformData, (this, null), keepActive);
                    _transform.TransformChanged += updateColliderPosition;
                }
                else
                {
                    physicsObj = physics!.Create(CreateCollider(), false, 0, TransformData.Default, (this, null), keepActive);
                }
            }
        }
    }

    private void updateColliderPosition(TransformData data)
    {
        if (physicsObj is RigidBody rigidBody)
        {
            ///Todo: This is a hack that implies that the center of mass is the center of the model
            ///Create a kinematic object instead
            rigidBody.CenterOfMassTransform = data.CreateSelectedModelMatrix(false).Convert();
        }
    }

    private readonly IColliderWrapper[] physicsColliders;
    public float[]? ColliderVertexData { get; private set; }
    public bool NotInitialized { get; set; } = true;
    public void InitFunc()
    {
        if (ColliderVertexData is null && Parent is Properties.Mesh.Mesh mesh)
            ColliderVertexData = mesh.MeshData.VertexData;
        EnablePhysics();
    }

    public bool NotDeleted { get; set; } = true;
    public void DeleteFunc()
    {
        DisablePhysics();
    }
}