using DOGEngine.RenderObjects.Properties;
using OpenTK.Mathematics;

namespace DOGEngine.Test.Scripts;

public class FlickeringLightScript : Script
{
    private static readonly Random random = new Random();
    private Lightning? light;
    private bool on;
    private double switchAfter;
    private double current;
    public override void Start()
    {
        Parent.TryGetComponent(out light);
    }

    public override void Update()
    {
        current += deltaTime;
        if (current > switchAfter)
        {
            current = 0;
            switchAfter = random.NextDouble() * 3 + 2;

            on = !on;
            if (light is not null)
            {
                if (on)
                    light.Color = null; //Todo: use color of plainColorShader, only works with this config
                else
                    light.Color = Vector3.Zero;
            }
        }
    }
}