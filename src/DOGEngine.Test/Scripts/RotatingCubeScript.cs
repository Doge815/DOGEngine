using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Text;

namespace DOGEngine.Test.Scripts;

public class RotatingCubeScript : Script
{
    private Transform? cubeTransform;
    private RenderText? rotationText;
    public override void Start()
    {
        Parent.TryGetComponent(out cubeTransform);
        Scene.GetAllWithName("rotationText").FirstOrDefault()?.TryGetComponent(out rotationText);
    }

    public override void Update()
    {
        if (cubeTransform is not null)
        {
            cubeTransform.Orientation = cubeTransform.Orientation with
            {
                Y = (cubeTransform.Orientation.Y + 60 * (float)deltaTime) % 360
            };
            if (rotationText is not null)
                rotationText.Text = $"Rotation: {cubeTransform.Orientation.Y:F0}°";
        }
    }
}