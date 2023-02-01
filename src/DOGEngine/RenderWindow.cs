using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Properties.Mesh;
using DOGEngine.RenderObjects.Text;
using DOGEngine.Shader;

namespace DOGEngine;

internal class RenderWindow : GameWindow
{
    internal Action OnLoadAction { get; set; }
    internal Action<FrameEventArgs> OnUpdateFrameAction { get; set; }
    internal Action<FrameEventArgs> OnRenderFrameAction { get; set; }
    internal Action<ResizeEventArgs> OnResizeAction { get; set; }

    public RenderWindow(int width, int height, string title, Action onLoadAction,Action<FrameEventArgs> onRenderFrameAction,
        Action<FrameEventArgs> onUpdateFrameAction, Action<ResizeEventArgs> onResizeAction) : base(GameWindowSettings.Default,
        new NativeWindowSettings() { Size = (width, height), Title = title })
    {
        OnLoadAction =  onLoadAction;
        OnRenderFrameAction = onRenderFrameAction;
        OnUpdateFrameAction = onUpdateFrameAction;
        OnResizeAction = onResizeAction;

    }

    protected override void OnLoad()
    {
        base.OnLoad();
        OnLoadAction();
        Focus();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        OnUpdateFrameAction(args);
    }


    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        OnRenderFrameAction(args);
        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        OnResizeAction(e);
    }
}


public class Window
{
    private readonly RenderWindow window;

    public Window(int width, int height, string title, Action<Window> onLoad, Action<Window, FrameEventArgs> onRender, Action<Window, FrameEventArgs>? onUpdate = null, Action<Window, ResizeEventArgs>? onResize = null)
    {
        window = new RenderWindow(width, height, title, 
            () => onLoad(this), 
            (frameEventArgs) => onRender(this, frameEventArgs), 
            onUpdate is not null? (frameEventArgs) =>onUpdate(this, frameEventArgs): (_) => {},
            onResize is not null? (frameEventArgs) =>onResize(this, frameEventArgs): (_) => {}
        );
        window.Run();
    }
    public string Title
    {
        get => window.Title;
        set => window.Title = value;
    }


    public KeyboardState KeyboardState => window.KeyboardState;
    public MouseState MouseState => window.MouseState;
    public bool IsFocused => window.IsFocused;
    public void Close() => window.Close();

    public void GrabCursor(bool grab) => window.CursorState = grab ? CursorState.Grabbed : CursorState.Normal;

    public static Action BasicLoad { get; } = () =>
    {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
    };
    public static Action<GameObjectCollection, Window, FrameEventArgs> BasicUpdate { get; } = (scene, window, args) =>
    {
        if (window.KeyboardState.IsKeyDown(Keys.Escape))
            window.Close();
        if (scene.TryGetComponent(out Physics.Physics? physics))
            physics!.Update((float)args.Time);
    };
    public static Action<GameObjectCollection, Camera.Camera, FrameEventArgs> BasicRender { get; } = (scene, camera, args) =>
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix;

        //scene.RenderSkybox(view, projection);
        //scene.SetLights();
        //scene.RenderMeshes(view, projection, camera.Position);
        //scene.RenderText(camera.Width, camera.Height);
        scene.RenderSprites(camera.Width, camera.Height);
    };
    public static Action<ResizeEventArgs, Camera.Camera> BasicResize { get; } = (args, camera) =>
    {
        GL.Viewport(0,0,args.Width, args.Height);
        camera.Width = args.Width;
        camera.Height = args.Height;
    };
}