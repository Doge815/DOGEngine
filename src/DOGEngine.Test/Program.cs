using DOGEngine;
using DOGEngine.Camera;
using DOGEngine.GameObjects;
using DOGEngine.GameObjects.Properties;
using DOGEngine.GameObjects.Properties.Mesh;
using DOGEngine.GameObjects.Properties.Mesh.Collider;
using DOGEngine.GameObjects.Text;
using DOGEngine.Physics;
using DOGEngine.Shader;
using DOGEngine.Test.Scripts;
using DOGEngine.Texture;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using TriangleMesh = DOGEngine.GameObjects.Properties.Mesh.TriangleMesh;
using Window = DOGEngine.Window;

GameObjectCollection scene3D = new GameObjectCollection();
PhysicalPlayerController camera3D = new PhysicalPlayerController();
GameObjectCollection sceneFPS = new GameObjectCollection();
Camera cameraFPS = new Camera();
bool focused = true;

void OnLoad(Window window)
{
    var wallTexture = new Texture("Texture/Textures/wall.jpg");
    var woodTexture = new Texture("Texture/Textures/wood.jpg");
    var carpetTexture = new Texture("Texture/Textures/carpet.jpg");
    var crosshairTexture = new Texture("Texture/Textures/Crosshair.png");
    var metalTexture = new PbrTextureCollection("Texture/Textures/Metal");

    var shader1 = new TextureShader(wallTexture);
    var shader2 = new TextureShader(woodTexture);
    var shader3 = new TextureShader(carpetTexture);
    var shader4 = new PlainColorShader(new Vector3(1, 1, 1));
    var shader5 = new PlainColorShader(new Vector3(0.8f, 0.2f, 0.2f));
    var shader6 = new PbrShader(metalTexture);
    var shader7 = new TextureShader(woodTexture, new Vector2(50f, 50f));
    var shader8 = new PbrShader(metalTexture, new Vector2(10,10));
    
    var ding = new AudioFile("Sounds/ding.mp3");

    var cubeMesh = new TriangleMesh(TriangleMesh.Cube);

    var font = new Font(Font.DejaVuSans, 50);

    scene3D.AddComponent(new Physics());
    scene3D.CollectionAddComponents(
        new Physics(),
        new Skybox("Texture/Skybox"),
        new CubeClickedScript(),
        new FpsScript(),
        camera3D,
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
            new Shading(shader7),
            new Mesh(cubeMesh, new Collider(PhysicsType.CreatePassive())),
            new Transform(new Vector3(0, -5, 0), null, new Vector3(50, 1, 50))
        ),
        new(
            new Shading(shader1),
            new Mesh(cubeMesh, new Collider(PhysicsType.CreatePassive(), null, true, new CubeCollider())),
            new Transform(new Vector3(15, -3, 0), null, new Vector3(3,3,1)),
            new PushingCubeScript()
        ),
        new(
            new Shading(shader1),
            new Mesh(cubeMesh),
            new Transform(new Vector3(-5, -1, -5)),
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
            new Lightning(),
            new FlickeringLightScript(),
            new AudioSource(ding)
        ),
        new(
            new Shading(shader3),
            new Mesh(cubeMesh),
            new Transform(new Vector3(0, 4, -5), new Vector3(0, 1, 0), new Vector3(2, 3, 4),
                new Vector3(1, 0, 1)),
            new RotatingCubeScript()
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
        new(
            new Shading(shader8),
            new Mesh(TriangleMesh.FromFile("Models/Cube")),
            new Transform(new Vector3(1, -2, -5f), null, new Vector3(2,2,2)),
            new RotatingCubeScript()
        ),
        new (
            new Sprite2D(crosshairTexture, new Vector2(-25, -25), new Vector2(50, 50), Corner.Center)),
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

    const int count = 5;
    for (int x = 0; x < count; x++)
    {
        for (int y = 0; y < count; y++)
        {
            for (int z = 0; z < count; z++)
            {
                GameObject cube = new (
                    new Shading(shader3),
                    new Mesh(cubeMesh, new Collider(PhysicsType.CreateActive(1), null, false, new CubeCollider())),
                    new Transform(new Vector3(10 + x, 10 + y, -10 - z)),
                    new Name("physicsCube"));
                scene3D.CollectionAddComponents(cube);
            }
        }
    }

    sceneFPS = new GameObjectCollection();
    sceneFPS.CollectionAddComponents(
        new GameObject(
            new Shading(shader1),
            new Mesh(cubeMesh),
            new Transform(new Vector3(4, -2, 2)),
            new AnimationController(),
            new FpsGunScript()
        ));
    
    window.GrabCursor(true);
}

void OnUpdate(Window window, FrameEventArgs frameEventArgs)
{
    if (focused)
    {
        camera3D.Update(window.KeyboardState, window.MouseState, (float)frameEventArgs.Time);
        if (window.IsFocused)
        {
            window.GrabCursor(true);
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
}
_ = new Window(800, 800, "TestApp",
    (window) =>
    {
        Window.BasicLoad();
        OnLoad(window);
    },
    (_, frameEventArgs) => Window.BasicRender(new []{(scene3D, camera3D.Camera),(sceneFPS, cameraFPS)}, frameEventArgs),
    (window, frameEventArgs) =>
    {
        Window.BasicUpdate(new []{(scene3D, camera3D.Camera), (sceneFPS, cameraFPS)}, window, frameEventArgs);
        OnUpdate(window, frameEventArgs);
    },
    (_, resizeArgs) => Window.BasicResize(resizeArgs, new []{camera3D.Camera, cameraFPS})

);