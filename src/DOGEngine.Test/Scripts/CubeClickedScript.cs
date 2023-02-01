using DOGEngine.Physics;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Text;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine.Test.Scripts;

public class CubeClickedScript : Script
{
    private int hitCounter = 0;
    private RenderText? hitCounterText;
    public override void Start()
    {
        Scene.GetAllWithName("hitText").FirstOrDefault()?.TryGetComponent(out hitCounterText);
    }

    public override void Update()
    {
        if (MouseState.IsButtonReleased(MouseButton.Button1))
        {
            var x = Scene.CastRay(MainCamera.Position, MainCamera.Front);
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