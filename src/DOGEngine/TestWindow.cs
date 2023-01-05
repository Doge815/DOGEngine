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
    //private Skybox skybox;
    //private OldObj? cube1;
    //private OldObj? cube2;
    //private OldObj? cube3;
    //private OldObj? pawn;
    private Scene? scene;
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

        //Todo
        //cube3!.Orientation = cube3.Orientation with { Y = cube3.Orientation.Y + 60 * (float)args.Time };
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.Enable(EnableCap.DepthTest);
        GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
        
        var wallTexture = new Texture.Texture("../../../../DOGEngine/Texture/Textures/wall.jpg");
        var woodTexture = new Texture.Texture("../../../../DOGEngine/Texture/Textures/wood.jpg");
        var carpetTexture = new Texture.Texture("../../../../DOGEngine/Texture/Textures/carpet.jpg");

        var shader1 = new TextureShader(wallTexture);
        var shader2 = new TextureShader(woodTexture);
        var shader3 = new TextureShader(carpetTexture);
        var shader4 = new PlainColorShader(new Vector3(1, 0, 0));

        scene = new Scene(
            new GameObject[]
            {
                new GameObjSkybox(new string[]
        {
            "../../../../DOGEngine/Texture/Skybox/right.jpg",
            "../../../../DOGEngine/Texture/Skybox/left.jpg",
            "../../../../DOGEngine/Texture/Skybox/top.jpg",
            "../../../../DOGEngine/Texture/Skybox/bottom.jpg",
            "../../../../DOGEngine/Texture/Skybox/front.jpg",
            "../../../../DOGEngine/Texture/Skybox/back.jpg",
        }),
                new GameObject(new GameObject[]
                {
                    new Shading(shader1),
                    new Mesh(Cube.data),
                    new Transform(new Vector3(-1, -1, -5)),
                })
            });

        //cube1 = new Cube(shader1, new Vector3(-1, -1, -5));
        //cube2 = new Cube(shader4, new Vector3(1, 1, -5));
        //cube3 = new Cube(shader3,new Vector3(0, 4, -5), new Vector3(0, 1, 0), new Vector3(2, 3, 4), new Vector3(1, 0, 1));
        //pawn = new ParsedModel( "../../../../DOGEngine/RenderObjects/Models/Pawn.obj", shader2, new Vector3(0, 0, -7));
        
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        
        var view = camera.ViewMatrix;
        var projection = camera.ProjectionMatrix;

        var skyBoxes = scene!.GetAllInChildren<GameObjSkybox>().ToArray();
        if(skyBoxes.Any()) skyBoxes.First().Draw(view, projection);

        foreach (Mesh mesh in scene.GetAllInChildren<Mesh>())
        {
            mesh.Draw(view, projection);
        }
        
        //skybox.Draw(view, projection);

        //cube1!.Draw(view, projection);
        //cube2!.Draw(view, projection);
        //cube3!.Draw(view, projection);
        //pawn!.Draw(view, projection);

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