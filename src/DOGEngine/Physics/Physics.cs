using BulletSharp;
using BulletSharp.Math;
using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Properties.Mesh.Collider;
using Vector3 = BulletSharp.Math.Vector3;

namespace DOGEngine.Physics;

public class Physics : GameObject
{
    private readonly DiscreteDynamicsWorld world;
    
    public Physics()
    {
        CollisionConfiguration collisionConfiguration = new DefaultCollisionConfiguration();
        CollisionDispatcher dispatcher = new(collisionConfiguration);
        BroadphaseInterface broadphase = new DbvtBroadphase();
        world = new DiscreteDynamicsWorld(dispatcher, broadphase, null, collisionConfiguration);
    }

    public void Update(float time)
    {
        world.StepSimulation(time);
        foreach (var o in world.CollisionObjectArray)
        {
            if (o is RigidBody { UserObject: (_,Action<Matrix> action) } collisionObject)
                action(collisionObject.MotionState.WorldTransform);
        }
    }

    internal void Remove(CollisionObject obj)
    {
        world.RemoveCollisionObject(obj);
    }

    internal RigidBody Create(CollisionShape shape, bool dynamic, float mass, TransformData translation, (Collider, Action<Matrix>? setTrans) userObj, bool keepActive)
    {
        RigidBodyConstructionInfo bodyInfo = dynamic
            ? new RigidBodyConstructionInfo(mass, null, shape, shape.CalculateLocalInertia(mass))
            : new RigidBodyConstructionInfo(mass, null, shape){StartWorldTransform = translation.CreateSelectedModelMatrix(false).Convert()};
        if(dynamic) 
            bodyInfo.MotionState = new DefaultMotionState(translation.CreateSelectedModelMatrix(false).Convert());
        
        var body = new RigidBody(bodyInfo);
        if (keepActive) body.ActivationState = ActivationState.DisableDeactivation;
        world.AddRigidBody(body);
        body.UserObject = userObj;
        bodyInfo.Dispose();
        return body;
    }

    public IEnumerable<Collider> GetAllColliding(Collider collider)
    {
        if(collider.physicsObj is null) yield break;
        HashSet<Collider> found = new () {collider};
        int numManifolds = world.Dispatcher.NumManifolds;
        for (int i = 0; i < numManifolds; i++)
        {
            PersistentManifold contactManifold = world.Dispatcher.GetManifoldByIndexInternal(i);

            if (contactManifold.Body0 is RigidBody { UserObject: (Collider col1, _) } &&
                contactManifold.Body1 is RigidBody { UserObject: (Collider col2, _) })
            {
                if (col1 == collider && !found.Contains(col2))
                {
                    found.Add(col2);
                    yield return col2;
                }

                if (col2 == collider && !found.Contains(col1))
                {
                    found.Add(col1);
                    yield return col1;
                }
            } 
        }
    }
}