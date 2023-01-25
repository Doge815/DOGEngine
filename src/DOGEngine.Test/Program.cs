using DOGEngine;
using DOGEngine.Camera;
using DOGEngine.Physics;
using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Properties.Mesh;
using DOGEngine.RenderObjects.Text;
using DOGEngine.Shader;
using DOGEngine.Texture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = DOGEngine.Window;

GameObjectCollection scene = new GameObjectCollection();
PlayerController camera = new PlayerController(){Yaw = -90, Pitch = 1.53f};
int hitCounter = 0;
bool focused = true;

void OnLoad(Window window)
{
    var wallTexture = new Texture("Texture/Textures/wall.jpg");
    var woodTexture = new Texture("Texture/Textures/wood.jpg");
    var carpetTexture = new Texture("Texture/Textures/carpet.jpg");
    var metalTexture = new PbrTextureCollection("Texture/Textures/Metal");

    var shader1 = new TextureShader(wallTexture);
    var shader2 = new TextureShader(woodTexture);
    var shader3 = new TextureShader(carpetTexture);
    var shader4 = new PlainColorShader(new Vector3(1, 1, 1));
    var shader5 = new PlainColorShader(new Vector3(0.8f, 0.2f, 0.2f));
    var shader6 = new PbrShader(metalTexture);

    var cubeMesh = new TriangleMesh(TriangleMesh.Cube);

    var font = new Font(Font.DejaVuSans, 50);

    scene.AddComponent(new Physics());
    scene.AddComponent(new Skybox("Texture/Skybox"));
    scene.CollectionAddComponents(
        new(
            new Shading(shader1),
            new Mesh(TriangleMesh.Triangle),
            new Transform(new Vector3(0, 0, 5)),
            new Name("testTriangle")
        ),
        new(
            new Shading(shader3),
            new Mesh(cubeMesh, new Collider(PhysicsType.CreateActive(1))),
            new Transform(new Vector3(-6, 8, -3), new Vector3(0, 45, 0), new Vector3(2, 1, 2) )
        ),
        new(
            new Shading(shader3),
            new Mesh(cubeMesh, new Collider(PhysicsType.CreateActive(1))),
            new Transform(new Vector3(-5.3f, 13, -3) )
        ),
        new(
            new Shading(shader3),
            new Mesh(cubeMesh, new Collider(PhysicsType.CreateActive(1))),
            new Transform(new Vector3(-7, 13, -3.5f) )
        ),
        new(
            new Shading(shader2),
            new Mesh(cubeMesh, new Collider(PhysicsType.CreatePassive())),
            new Transform(new Vector3(-6, -3, -3), null, new Vector3(5, 1, 5))
        ),
        new(
            new Shading(shader1),
            new Mesh(cubeMesh),
            new Transform(new Vector3(-1, -1, -5)),
            new Name("hitCube")
        ),
        new(
            new Shading(shader4),
            new Mesh(cubeMesh),
            new Transform(new Vector3(1, 1, 10)),
            new Lightning()
        ),
        new(
            new Shading(shader5),
            new Mesh(cubeMesh),
            new Transform(new Vector3(10, 1, 1)),
            new Lightning()
        ),
        new(
            new Shading(shader3),
            new Mesh(cubeMesh),
            new Transform(new Vector3(0, 4, -5), new Vector3(0, 1, 0), new Vector3(2, 3, 4),
                new Vector3(1, 0, 1)),
            new Name("cube3")
        ),
        new(
            new Shading(shader2),
            new Mesh(TriangleMesh.FromFile("Models/Pawn"), new Collider("Models/PawnLowPoly")),
            new Transform(new Vector3(0, -2, -7))
        ),
        new(
            new Shading(shader6),
            new Mesh(TriangleMesh.FromFile("Models/Sphere"), new Collider("Models/SphereLowPoly")),
            new Transform(new Vector3(1, 1, -3))
        ),
        new (
            new RenderText(font, "", new Vector2(100, -100), Corner.TopLeft, new Vector3(1, 1 ,0), 1),
            new Name("fpsText")),
        new (
            new RenderText(font, "", new Vector2(100, -200), Corner.TopLeft, new Vector3(1, 1 ,0), 1),
            new Name("hitText")),
        new (
            new RenderText(font, "", new Vector2(100, 50), Corner.BottomLeft, new Vector3(1, 0 ,1), 1),
            new Name("rotationText"))
        
    );
    
    scene.InitializeAll();
    window.GrabCursor(true);
}

void OnUpdate(Window window, FrameEventArgs frameEventArgs)
{
    if (focused)
    {
        camera.Update(window.KeyboardState, window.MouseState, (float)frameEventArgs.Time);
        if (window.IsFocused)
        {
            window.GrabCursor(true);
        }
        if (window.MouseState.IsButtonPressed(MouseButton.Button1))
        {
            var x = scene.CastRay(camera.Position, camera.Front);
            if (x is not null && x.Parent.Parent.TryGetComponent(out Name? name) && name!.ObjName == "hitCube")
                hitCounter++;
        }
    }

    if (window.IsFocused && !focused && window.MouseState.IsButtonPressed(MouseButton.Button1))
    {
        focused = true;
    }

    if (window.IsFocused && window.KeyboardState.IsKeyPressed(Keys.E))
    {
        focused = false;
        window.GrabCursor(false);
    }
    

    Transform? cube = null;
    scene.GetAllWithName("cube3").ForEach((obj =>
    {
        if (obj.TryGetComponent(out Transform? transform)) cube = transform!;
    }));
    if (cube is not null)
    {
        cube.Orientation = cube.Orientation with{Y = (cube.Orientation.Y + 60 * (float)frameEventArgs.Time)%360};
        scene.GetAllWithName("rotationText").ForEach(obj =>
        {
            if (obj.TryGetComponent(out RenderText? renderText))
                renderText!.Text = $"Rotation: {cube.Orientation.Y:F0}°";
        });
    }
    
    scene.GetAllWithName("fpsText").ForEach(obj =>
    {
        if (obj.TryGetComponent(out RenderText? renderText))
            renderText!.Text = $"FPS: {1/frameEventArgs.Time:F2}";
    });
    scene.GetAllWithName("hitText").ForEach(obj =>
    {
        if (obj.TryGetComponent(out RenderText? renderText))
            renderText!.Text = $"Cube hits: {hitCounter}";
    });

}
_ = new Window(800, 800, "TestApp",
    (window) =>
    {
        Window.BasicLoad();
        OnLoad(window);
    },
    (_, frameEventArgs) => Window.BasicRender(scene, camera, frameEventArgs),
    (window, frameEventArgs) =>
    {
        Window.BasicUpdate(scene, window, frameEventArgs);
        OnUpdate(window, frameEventArgs);
    },
    (_, resizeArgs) => Window.BasicResize(resizeArgs, camera)

);