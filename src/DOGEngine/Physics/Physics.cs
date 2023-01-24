using BulletSharp;
using BulletSharp.Math;
using DOGEngine.RenderObjects;

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
            if(o is RigidBody { UserObject: Action<Matrix> action } collisionObject)
                action(collisionObject.MotionState.WorldTransform);
        }
    }

    public void CreateDynamic(float mass, )
    {
        var shape = new BoxShape(0.5f);
        var inertia = shape.CalculateLocalInertia(mass);
    }
}