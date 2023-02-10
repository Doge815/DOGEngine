using DOGEngine.GameObjects;
using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects;
using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Properties.Mesh;
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
    public static Action<IEnumerable<(GameObjectCollection, Camera.Camera)>, Window, FrameEventArgs> BasicUpdate { get; } = (sceneCameraPairs, window, args) =>
    {
        if (window.KeyboardState.IsKeyDown(Keys.Escape))
            window.Close();
        Script.KeyboardState = window.KeyboardState;
        Script.MouseState = window.MouseState;
        Script.deltaTime = args.Time;
        foreach (var pair in sceneCameraPairs)
        {
            Script.Scene = pair.Item1;
            Script.Camera = pair.Item2;
            if (pair.Item1.TryGetComponent(out Physics.Physics? physics))
                physics!.Update((float)args.Time);
            if (!pair.Item1.initializeChildren) pair.Item1.InitializeAll();
            foreach (var script in pair.Item1.GetAllInChildren<Script>().ToArray())
                script.Update();
            foreach (var audio in pair.Item1.GetAllInChildren<AudioSource>())
                audio.UpdateDirections(pair.Item2.Position);
        }
    };
    public static Action<IEnumerable<(GameObjectCollection, Camera.Camera)>, FrameEventArgs> BasicRender { get; } = (SceneCameraPairs, args) =>
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        foreach (var pair in SceneCameraPairs)
        {
            var view = pair.Item2.ViewMatrix;
            var projection = pair.Item2.ProjectionMatrix;

            pair.Item1.RenderSkybox(view, projection);
            pair.Item1.SetLights();
            pair.Item1.RenderMeshes(view, projection, pair.Item2.Position);
            pair.Item1.RenderText(pair.Item2.Width, pair.Item2.Height);
            pair.Item1.RenderSprites(pair.Item2.Width, pair.Item2.Height);
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }
    };
    public static Action<ResizeEventArgs, IEnumerable<Camera.Camera>> BasicResize { get; } = (args, cameras) =>
    {
        GL.Viewport(0,0,args.Width, args.Height);
        foreach (Camera.Camera camera in cameras)
        {
            camera.Width = args.Width;
            camera.Height = args.Height;
        }
    };
}