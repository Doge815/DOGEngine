using DOGEngine.GameObjects.Properties;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine.Test.Scripts;

public class FpsGunScript : Script
{
    private static Animation shotAnimation = new Animation(new []
    {
        new AnimationState(0, new TransformData(Vector3.Zero, Vector3.Zero, Vector3.One, new Vector3(0.4f, 0.3f, 0))),
        new AnimationState(0.1, new TransformData(Vector3.Zero, new Vector3(0, 0, 45), Vector3.One, new Vector3(0.4f, 0.3f, 0))),
        new AnimationState(0.3, new TransformData(Vector3.Zero, Vector3.Zero, Vector3.One, new Vector3(0.4f, 0.3f, 0))),
    });
    private AnimationController? animation;

    protected override void Start()
    {
        Parent.TryGetComponent(out animation);
        
    }

    public override void Update()
    {
        if (MouseState.IsButtonReleased(MouseButton.Button1))
        {
            animation?.Play(shotAnimation);
        }
    }
}