using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine.Camera;

public class PlayerController : Camera
{
    public float Speed { get; set; } = 5;
    public float Sensitivity = 0.2f;
    private Vector2? mousePosition { get; set; }
    public void Update(KeyboardState input, MouseState mouse, float deltaSecs)
    {
        if (input.IsKeyDown(Keys.W))
            Position += Front * Speed * deltaSecs; 
        if (input.IsKeyDown(Keys.S))
            Position -= Front * Speed * deltaSecs; 
        if (input.IsKeyDown(Keys.A))
            Position -= Right * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.D))
            Position += Right * Speed * deltaSecs;
        if (input.IsKeyDown(Keys.LeftShift))
            Position += Up * Speed * deltaSecs; 
        if (input.IsKeyDown(Keys.LeftControl))
            Position -= Up * Speed * deltaSecs;

        mousePosition ??= new Vector2(mouse.X, mouse.Y);
        var deltaX = mouse.X - mousePosition.Value.X;
        var deltaY = mouse.Y - mousePosition.Value.Y;
        mousePosition= new Vector2(mouse.X, mouse.Y);
        
        Yaw += deltaX * Sensitivity; 
        Pitch -= deltaY * Sensitivity; 
    }
}