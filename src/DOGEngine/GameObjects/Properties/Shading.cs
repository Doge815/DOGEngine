using DOGEngine.GameObjects;

namespace DOGEngine.GameObjects.Properties;

public class Shading : GameObject
{
    public Shader.Shader Shader { get; set; }
    public Shading(Shader.Shader shader) 
    {
        Shader = shader;
    }
}