namespace DOGEngine.RenderObjects.Properties;

public abstract class Script : GameObject, IPostInitializedGameObject
{
    public static KeyboardState KeyboardState { get; internal set; }
    public static MouseState MouseState { get; internal set; }
    public static GameObjectCollection Scene { get; internal set; }
    public static double deltaTime { get; internal set; }
    public static Camera.Camera MainCamera { get; internal set; }
    public virtual void Start() { }
    public virtual void Update() { }
    public bool NotInitialized { get; set; } = true;
    public void InitFunc() => Start();
}