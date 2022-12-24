using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace DOGEngine;

public class TestWindow : GameWindow
{
    
        private Cube? cube2;
        private Cube? cube1;
        public TestWindow(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) { }
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        protected override void OnLoad()
        {
            GL.Enable(EnableCap.DepthTest);
            base.OnLoad();
            GL.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);

            var wallTexture = new Texture("../../../../DOGEngine/Textures/wall.jpg");
            var woodTexture = new Texture("../../../../DOGEngine/Textures/wood.jpg");

            var shader1 = new TextureShader(wallTexture);
            var shader2 = new TextureShader(woodTexture);

            cube1 = new Cube(shader1);
            cube1.OnLoad();

            cube2 = new Cube(shader2);
            cube2.OnLoad();



        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Matrix4 view = Matrix4.CreateTranslation(0, 0, -3f);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 1, 0.1f, 100.0f);
            
            cube1!.Position = new Vector3(-1f, -1f, -5f);
            cube1!.Draw(view, projection);

            cube2!.Position = new Vector3(1f, 1f, -5f);
            cube2!.Draw(view, projection);

            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, e.Width, e.Height);
        }
}