using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Text;

namespace DOGEngine.Test.Scripts;

public class FpsScript : Script
{
    private RenderText? FPS;

    public override void Start() => Scene.GetAllWithName("fpsText").FirstOrDefault()?.TryGetComponent(out FPS);

    public override void Update()
    {
        if (FPS is not null) 
            FPS.Text = $"FPS: {1/deltaTime:F2}";
        
    }
}