using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Properties;

namespace DOGEngine.Test.Scripts;

public class PushingCubeScript : Script
{
    private Transform? cubeTransform;
    protected override void Start() => Parent.TryGetComponent(out cubeTransform);
    public override void Update()
    {
        if(cubeTransform is not  null)
            cubeTransform.Position = cubeTransform.Position with
            {
                Z = cubeTransform.Position.Z - (float)deltaTime
            };
    }
}