using DOGEngine.RenderObjects;
using DOGEngine.RenderObjects.Properties;
using DOGEngine.RenderObjects.Properties.Mesh;
using DOGEngine.RenderObjects.Text;
using DOGEngine.Shader;

namespace DOGEngine;

public static class Renderer
{
    public static void RenderSkybox(this GameObjectCollection scene, Matrix4 view, Matrix4 projection) => scene.GetAllInChildren<Skybox>().FirstOrDefault()?.Draw(view, projection);
    public static void SetLights(this GameObjectCollection scene)
    {
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
    }

    public static void RenderMeshes(this GameObjectCollection scene, Matrix4 view, Matrix4 projection, Vector3 position)
    {
        foreach (Mesh mesh in scene.GetAllInChildren<Mesh>())
            mesh.Draw(view, projection, position);
    }

    public static void RenderText(this GameObjectCollection scene, int width, int height)
    {
        foreach (RenderText text in scene.GetAllInChildren<RenderText>())
            text.Draw(width, height);
    }
    public static void RenderSprites(this GameObjectCollection scene, int width, int height)
    {
        foreach (Sprite2D sprite2D in scene.GetAllInChildren<Sprite2D>())
            sprite2D.Draw();
    }
}