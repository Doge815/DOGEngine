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
    private GameObject? scene;
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

        scene?.GetAllWithName("cube3").ForEach((obj =>
        {
            if(obj.TryGetComponent(out Transform? transform))
                transform!.Orientation = transform.Orientation with{Y = transform.Orientation.Y + 60 * (float)args.Time};
        }));
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

        scene = new GameObjectCollection(
            new GameObject[]
            {
                new GameObjSkybox(new[]
                {
                    "../../../../DOGEngine/Texture/Skybox/right.jpg",
                    "../../../../DOGEngine/Texture/Skybox/left.jpg",
                    "../../../../DOGEngine/Texture/Skybox/top.jpg",
                    "../../../../DOGEngine/Texture/Skybox/bottom.jpg",
                    "../../../../DOGEngine/Texture/Skybox/front.jpg",
                    "../../../../DOGEngine/Texture/Skybox/back.jpg",
                }),
                new(
                    new Shading(shader1),
                    new Mesh(Mesh.Cube),
                    new Transform(new Vector3(-1, -1, -5))
                ),
                new(
                    new Shading(shader4),
                    new Mesh(Mesh.Cube),
                    new Transform(new Vector3(1, 1, -5))
                ),
                new(
                    new Shading(shader3),
                    new Mesh(Mesh.Cube),
                    new Transform(new Vector3(0, 4, -5), new Vector3(0, 1, 0), new Vector3(2, 3, 4),
                        new Vector3(1, 0, 1)),
                    new Name("cube3")
                ),
                new(
                    new Shading(shader2),
                    new Mesh(Mesh.FromFile("../../../../DOGEngine/RenderObjects/Models/Pawn.obj")),
                    new Transform(new Vector3(0, -2, -7))
                ),
            });
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
            mesh.Draw(view, projection);
        

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