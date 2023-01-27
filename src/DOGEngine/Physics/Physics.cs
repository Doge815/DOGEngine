using BulletSharp;
using BulletSharp.Math;
using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
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
            if (o is RigidBody { UserObject: Action<Matrix> action } collisionObject)
                action(collisionObject.MotionState.WorldTransform);
        }
    }

    internal void Create(CollisionShape shape, bool dynamic, float mass, TransformData translation, Action<Matrix>? setTrans)
    {
        RigidBodyConstructionInfo bodyInfo = dynamic
            ? new RigidBodyConstructionInfo(mass, null, shape, shape.CalculateLocalInertia(mass))
            : new RigidBodyConstructionInfo(mass, null, shape){StartWorldTransform = translation.CreateSelectedModelMatrix(false, true, true, true).Convert()};
        if(dynamic) 
            bodyInfo.MotionState = new DefaultMotionState(translation.CreateSelectedModelMatrix(false, true, true, true).Convert());
        
        var body = new RigidBody(bodyInfo);
        world.AddRigidBody(body);
        body.UserObject = setTrans;
        bodyInfo.Dispose();
    }
}