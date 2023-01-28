using BulletSharp;
using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Properties.Mesh;
using DOGEngine.RenderObjects.Properties.Mesh.Collider;

namespace DOGEngine.Camera;

public class PhysicalPlayerController : GameObject
{
    public Camera Camera { get; }
    public Transform Transform { get; }
    private Collider Collider { get; }
    public float Speed { get; set; } = 5;
    public float Sensitivity { get; set; } = 0.2f;
    private Vector2? mousePosition { get; set; }

    public PhysicalPlayerController()
    {
        Camera = new Camera();
        Transform = new Transform();
        Collider = new Collider(PhysicsType.CreateActive(1000000), Transform, true, new CapsuleCollider(0.5f, 2));
        AddComponent(Transform);
        AddComponent(Collider);
        
    }

    public void Update(KeyboardState input, MouseState mouse, float deltaSecs)
    {
        Vector3 movementFront = Vector3.Normalize(Camera.Front with { Y = 0 });
        Vector3 movementRight = Vector3.Normalize(Vector3.Cross(movementFront, Vector3.UnitY));
        Vector3 movementUp = Vector3.Normalize(Vector3.Cross(movementRight, movementFront));

        var position = Transform.Position;
        if (input.IsKeyDown(Keys.W))
            position += movementFront * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.S))
            position -= movementFront * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.A))
            position -= movementRight * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.D))
            position += movementRight * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.LeftShift))
            position += movementUp * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.LeftControl))
            position -= movementUp * Speed * deltaSecs;

        Camera.Position = position + new Vector3(0, 0.8f, 0);
        Transform.Position = position;
        if (Collider.physicsObj is RigidBody rb) rb.LinearVelocity = new BulletSharp.Math.Vector3(0, rb.LinearVelocity.Y, 0);

        mousePosition ??= new Vector2(mouse.X, mouse.Y);
        var deltaX = mouse.X - mousePosition.Value.X;
        var deltaY = mouse.Y - mousePosition.Value.Y;
        mousePosition = new Vector2(mouse.X, mouse.Y);

        Camera.Yaw += deltaX * Sensitivity;
        Camera.Pitch -= deltaY * Sensitivity;
    }
}