using System.Data.Common;
using DOGEngine.RenderObjects;
using DOGEngine.Shader;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

    public static Action BasicLoad { get; } = () =>
    {
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
    };
    public static Action<Window> BasicUpdate { get; } = (window) =>
    {
        if (window.KeyboardState.IsKeyDown(Keys.Escape))
            window.Close();
    };
    public static Action<GameObject, Camera.Camera> BasicRender { get; } = (scene, camera) =>
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix;

        var skyBoxes = scene.GetAllInChildren<GameObjSkybox>().ToArray();
        if(skyBoxes.Any()) skyBoxes.First().Draw(view, projection);

        List<Lightning> lights = new List<Lightning>();
        foreach (Lightning light in scene.GetAllInChildren<Lightning>())
            lights.Add(light);
        
        foreach (Shading shading in scene.GetAllInChildren<Shading>())
        {
            if(shading.Shader is PbrShader pbrShader)
                foreach ((Lightning item, int index) in lights.WithIndex())
                {
                    pbrShader.SetVector3($"lightPositions[{index}]", item.GetPosition);
                    pbrShader.SetVector3($"lightColors[{index}]", item.GetColor);
                }
        }
            
        foreach (Mesh mesh in scene.GetAllInChildren<Mesh>())
            mesh.Draw(view, projection, camera.Position);
    };
    public static Action<ResizeEventArgs, Camera.Camera> BasicResize { get; } = (args, camera) =>
    {
        GL.Viewport(0,0,args.Width, args.Height);
        camera.Width = args.Width;
        camera.Height = args.Height;
    };
}