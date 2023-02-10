using DOGEngine.GameObjects.Properties;
using DOGEngine.Physics;
using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Text;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine.Test.Scripts;

public class CubeClickedScript : Script
{
    private int hitCounter;
    private RenderText? hitCounterText;

    protected override void Start()
    {
        Scene.GetAllWithName("hitText").FirstOrDefault()?.TryGetComponent(out hitCounterText);
    }

    public override void Update()
    {
        if (MouseState.IsButtonReleased(MouseButton.Button1))
        {
            var x = Scene.CastRay(Camera.Position, Camera.Front);
            if (x is not null && x.Parent.Parent.TryGetComponent(out Name? name))
            {
                if(name!.ObjName == "hitCube")
                    hitCounter++;
                if (name.ObjName == "physicsCube")
                    Scene.CollectionRemoveComponents(true, x.Parent.Parent);
            }
        }

        if(hitCounterText is not null)
            hitCounterText.Text = $"Cube hits: {hitCounter}";
    }
}