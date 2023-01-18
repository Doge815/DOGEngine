namespace DOGEngine.RenderObjects.Properties;

public class Name : GameObject
{
    public string ObjName { get; }

    public Name(string objName) => ObjName = objName;
}