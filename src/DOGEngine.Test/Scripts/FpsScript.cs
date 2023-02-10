using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Text;
using DOGEngine.GameObjects.Properties;

namespace DOGEngine.Test.Scripts;

public class FpsScript : Script
{
    private RenderText? FPS;

    protected override void Start() => Scene.GetAllWithName("fpsText").FirstOrDefault()?.TryGetComponent(out FPS);

    public override void Update()
    {
        if (FPS is not null) 
            FPS.Text = $"FPS: {1/deltaTime:F2}";
        
    }
}