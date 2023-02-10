using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Properties;
using OpenTK.Mathematics;

namespace DOGEngine.Test.Scripts;

public class FlickeringLightScript : Script
{
    private static readonly Random random = new Random();
    private Lightning? light;
    private AudioSource? audio;
    private bool on;
    private double switchAfter;
    private double current;

    protected override void Start()
    {
        Parent.TryGetComponent(out light);
        Parent.TryGetComponent(out audio);
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
            if(audio is not null)
                audio.Play();
        }
    }
}