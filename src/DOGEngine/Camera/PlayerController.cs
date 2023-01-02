using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine.Camera;

public enum PlayerControllerMode
{
    Free,
    Flat,
}

public class PlayerController : Camera
{
    public PlayerControllerMode MovementMode { get; set; } = PlayerControllerMode.Flat;

    public Vector3 movementFront => MovementMode switch
    {
        PlayerControllerMode.Flat => Vector3.Normalize(Front with{Y = 0}),
        PlayerControllerMode.Free => Front,
        _ => throw new ArgumentOutOfRangeException("unknown value")
    };
    public Vector3 movementRight => Vector3.Normalize(Vector3.Cross(movementFront, Vector3.UnitY));
    public Vector3 movementUp => Vector3.Normalize(Vector3.Cross(movementRight, movementFront));
    public float Speed { get; set; } = 5;
    public float Sensitivity { get; set; } = 0.2f;
    private Vector2? mousePosition { get; set; }
    public void Update(KeyboardState input, MouseState mouse, float deltaSecs)
    {
        if (input.IsKeyDown(Keys.W))
            Position += movementFront * Speed * deltaSecs; 
        if (input.IsKeyDown(Keys.S))
            Position -= movementFront * Speed * deltaSecs; 
        if (input.IsKeyDown(Keys.A))
            Position -= movementRight * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.D))
            Position += movementRight * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.LeftShift))
            Position += movementUp * Speed * deltaSecs; 
        if (input.IsKeyDown(Keys.LeftControl))
            Position -= movementUp * Speed * deltaSecs;

        mousePosition ??= new Vector2(mouse.X, mouse.Y);
        var deltaX = mouse.X - mousePosition.Value.X;
        var deltaY = mouse.Y - mousePosition.Value.Y;
        mousePosition= new Vector2(mouse.X, mouse.Y);
        
        Yaw += deltaX * Sensitivity; 
        Pitch -= deltaY * Sensitivity; 
    }
}