using DOGEngine.Camera;
using DOGEngine.RenderObjects;
using DOGEngine.Shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine;

public class TestWindow : GameWindow
{
    private Skybox skybox;
    private RenderObject? cube1;
    private RenderObject? cube2;
    private RenderObject? cube3;
    private RenderObject? pawn;
    private readonly PlayerController camera;

    public TestWindow(int width, int height, string title) : base(GameWindowSettings.Default,
        new NativeWindowSettings() { Size = (width, height), Title = title })
        => camera = new PlayerController() 
            {Width = width, Height = height, Yaw = -90, Pitch = 1.53f};
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);

        if (KeyboardState.IsKeyDown(Keys.Escape))
            Close();
        if(IsFocused)
            camera.Update(KeyboardState, MouseState, (float)args.Time);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

        skybox = new Skybox(new string[]
        {
            "../../../../DOGEngine/Texture/Skybox/right.jpg",
            "../../../../DOGEngine/Texture/Skybox/left.jpg",
            "../../../../DOGEngine/Texture/Skybox/top.jpg",
            "../../../../DOGEngine/Texture/Skybox/bottom.jpg",
            "../../../../DOGEngine/Texture/Skybox/front.jpg",
            "../../../../DOGEngine/Texture/Skybox/back.jpg",
        });

        var wallTexture = new Texture.Texture("../../../../DOGEngine/Texture/Textures/wall.jpg");
        var woodTexture = new Texture.Texture("../../../../DOGEngine/Texture/Textures/wood.jpg");
        var carpetTexture = new Texture.Texture("../../../../DOGEngine/Texture/Textures/carpet.jpg");

        var shader1 = new TextureShader(wallTexture);
        var shader2 = new TextureShader(woodTexture);
        var shader3 = new TextureShader(carpetTexture);
        var shader4 = new PlainColorShader(new Vector3(1, 0, 0));

        cube1 = new Cube(shader1);
        cube1.OnLoad();
        cube1.Position = new Vector3(-1, -1, -5);

        cube2 = new Cube(shader4);
        cube2.OnLoad();
        cube2.Position = new Vector3(1, 1, -5);
        
        cube3 = new Cube(shader3);
        cube3.OnLoad();
        cube3.Position = new Vector3(0, 4, -5);

        pawn = new ParsedModel( "../../../../DOGEngine/RenderObjects/Models/SmoothPawn.obj", shader2);
        pawn.OnLoad();
        pawn.Position = new Vector3(0, 0, -7);
        
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix;
        
        skybox.Draw(view, projection);

        cube1!.Draw(view, projection);
        cube2!.Draw(view, projection);
        cube3!.Draw(view, projection);
        pawn!.Draw(view, projection);

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        camera.Width = e.Width;
        camera.Height = e.Height;
    }
}