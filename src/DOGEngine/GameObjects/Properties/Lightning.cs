using DOGEngine.GameObjects;
using DOGEngine.GameObjects.Properties;
using DOGEngine.Shader;

namespace DOGEngine.GameObjects.Properties;

public class Lightning : GameObject
{
    public Vector3? Color { get; set; }
    public Vector3? Position { get; set; }

    public Vector3 GetColor
    {
        get => Color ?? parentColor() ?? Vector3.Zero;
        set => Color = value;
    }
    public Vector3 GetPosition
    {
        get => Position ?? parentPosition() ?? Vector3.Zero;
        set => Position = value;
    }

    public Lightning(Vector3? color = null, Vector3? position = null)
    {
        Color = color;
        Position = position;
    }

    private Vector3? parentColor()  
    {
        if(Parent.TryGetComponent<Shading>(out var shading) && shading!.Shader is PlainColorShader plainColorShader)
            return plainColorShader.Color * 255;
        return null;
    }
    private Vector3? parentPosition()  
    {
        if(Parent.TryGetComponent<Transform>(out var transform))
            return transform!.Position;
        return null;
    }
}