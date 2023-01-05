using System.Runtime.CompilerServices;
using DOGEngine.Shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace DOGEngine.RenderObjects;

public interface IPostInitializedGameObject
{
    internal void Initialize();
}
public class GameObject
{
    private readonly Dictionary<Type, GameObject> children;
    public IReadOnlyCollection<GameObject> Children => children.Values;

    public T GetComponent<T>() where T : GameObject
    {
        if (children.TryGetValue(typeof(T), out var gameObject))
            return gameObject as T ?? throw new InvalidOperationException("Internal engine error");
        throw new ArgumentException("Can't find component");
    }

    public bool TryGetComponent<T>(out T? gameObject) where T : GameObject
    {
        if (children.TryGetValue(typeof(T), out GameObject? obj))
        {
            gameObject = obj as T ?? throw new InvalidOperationException("Internal engine error");
            return true;
        }

        gameObject = null;
        return false;
    }

    public virtual IEnumerable<T> GetAllInChildren<T>() where T : GameObject
    {
        foreach (GameObject child in Children)
        foreach (var childComponent in child.GetAllInChildren<T>())
            yield return childComponent;

        if (GetType() == typeof(T)) yield return this as T ?? throw new SystemException("Oof");
    }

    public void AddComponent(GameObject gameObject)
    {
        if (!children.TryAdd(gameObject.GetType(), gameObject)) throw new ArgumentException("Can't add component");
        if(gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
    }

    public bool TryAddComponent(GameObject gameObject)
    {
        bool success = children.TryAdd(gameObject.GetType(), gameObject);
        if(success && gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
        return success;
    }

    public GameObject Parent { get; internal set; }

    protected GameObject(GameObject parent)
    {
        Parent = parent;
        children = new Dictionary<Type, GameObject>();
    }

    public GameObject()
    {
        Parent = this;
        children = new Dictionary<Type, GameObject>();
    }

    public GameObject(params GameObject[] newChildren)
    {
        Parent = this;
        children = new Dictionary<Type, GameObject>();
        foreach (var child in newChildren)
        {
            child.Parent = this;
            AddComponent(child);
        }
        foreach (var child in newChildren)
        {
            child.Parent = this;
            AddComponent(child);
        }
    }
}

public class Scene : GameObject
{
    public Scene() 
    {
        AddComponent(new GameObjectCollection(this));
    }
    public Scene(params GameObject[] newChildren)
    {
        GameObjectCollection collection = new GameObjectCollection(this);
        AddComponent(collection);
        foreach (var child in newChildren)
        {
            child.Parent = collection;
            collection.AddComponent(child);
            if(child is IPostInitializedGameObject initialize) initialize.Initialize();
        }
    }
}

public class GameObjectCollection : GameObject
{
    private readonly List<GameObject> collection;
    public IReadOnlyCollection<GameObject> Collection => collection.AsReadOnly();

    public GameObjectCollection(GameObject parent) : base(parent)
    {
        collection = new List<GameObject>();
    }

    public override IEnumerable<T> GetAllInChildren<T>()
    {
        foreach (var child in base.GetAllInChildren<T>())
            yield return child;

        foreach (GameObject child in collection)
            foreach (T childComponent in child.GetAllInChildren<T>())
                yield return childComponent;
    }

    public void AddGameObject(GameObject gameObject) => collection.Add(gameObject);
}

public class Transform : GameObject
{
    public Vector3 Position { get; set; }
    public Vector3 Orientation { get; set; }
    public Vector3 Scale { get; set; }
    public Vector3 OrientationOffset { get; set; }

    public Transform(Vector3? position = null, Vector3? orientation = null, Vector3? scale = null,
        Vector3? orientationOffset = null) 
    {
        Position = position ?? Vector3.Zero;
        Orientation = orientation ?? Vector3.Zero;
        Scale = scale ?? Vector3.One;
        OrientationOffset = orientationOffset ?? Vector3.Zero;
    }
}

public class Mesh : GameObject, IPostInitializedGameObject
{
    private int VAO; 
    private int triangles;

    private readonly VertexDataBundle tempData;
    public Mesh(VertexDataBundle data)
    {
        tempData = data;
        VAO = -1;
    }

    public void Initialize()
    {
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        var vertices = tempData.CreateVertices(shader) ?? Array.Empty<float>();
        triangles = tempData.Rows;
        
        int vertexBufferObj = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObj);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);

        foreach (IShaderAttribute attribute in shader.Attributes)
            interpretVertexDataFloat(shader, attribute, shader.Stride);
    }

    public void Draw(Matrix4 view, Matrix4 projection)
    {
        if(VAO == -1) return;
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        (Vector3 Position, Vector3 Orientation, Vector3 Scale, Vector3 OrientationOffset)  = GetTransformData();
        shader.Use();
        Matrix4 model = Matrix4.CreateScale(Scale)
                        * Matrix4.CreateTranslation(OrientationOffset)
                        * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Orientation.X))
                        * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Orientation.Y))
                        * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Orientation.Z))
                        * Matrix4.CreateTranslation(-OrientationOffset)
                        * Matrix4.CreateTranslation(Position);

        shader.SetMatrix4("model", model);
        shader.SetMatrix4("view", view);
        shader.SetMatrix4("projection", projection);

        GL.BindVertexArray(VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, triangles);
    }

    public (Vector3 Position, Vector3 Orientation, Vector3 Scale, Vector3 OrientationOffset) GetTransformData()
    {
        if (Parent.TryGetComponent<Transform>(out var transform))
            return (transform!.Position, transform.Orientation, transform.Scale, transform.OrientationOffset);
        return (Vector3.Zero, Vector3.Zero, Vector3.One, Vector3.Zero);
    }
    private void interpretVertexData(Shader.Shader shader, string attribute, int size, VertexAttribPointerType type, int stride, int offset)
    {
        int index = shader.GetAttributeLocation(attribute);
        if (index != -1)
        {
            GL.EnableVertexAttribArray(index);
            GL.VertexAttribPointer(index, size, type, false, stride, offset);
        }
    }

    private void interpretVertexDataFloat(Shader.Shader shader, IShaderAttribute attribute, int stride)
    {
        interpretVertexData(shader, attribute.AttributeName, attribute.Size, VertexAttribPointerType.Float,
            stride * sizeof(float),
            attribute.Offset * sizeof(float));
    }
}

public class Shading : GameObject
{
    public Shader.Shader Shader { get; set; }
    public Shading(Shader.Shader shader) 
    {
        Shader = shader;
    }
}

public class GameObjSkybox : GameObject //Todo: Split
{
    private readonly int textureHandle;
    private readonly int cubeHandle;
    private readonly Shader.Shader shader;
    public GameObjSkybox(string[] filePath)
    {
        textureHandle = GetCubeMap(filePath);
        shader = new Shader.CubeMapShader();
        cubeHandle = GetSkyBoxCube();
    }
    private int GetCubeMap(string[] path)
    {
        TextureTarget[] orientations =
        {
            TextureTarget.TextureCubeMapPositiveX,
            TextureTarget.TextureCubeMapNegativeX,
            TextureTarget.TextureCubeMapPositiveY,
            TextureTarget.TextureCubeMapNegativeY,
            TextureTarget.TextureCubeMapPositiveZ,
            TextureTarget.TextureCubeMapNegativeZ
        };
        if (path.Length != 6) throw new ArgumentException(nameof(path));
        GL.GenTextures(1, out int Handle);
        GL.BindTexture(TextureTarget.TextureCubeMap, Handle);
        for (int i = 0; i < 6; i++)
        {
            ImageResult image = ImageResult.FromStream(File.OpenRead(path[i]), ColorComponents.RedGreenBlueAlpha);
            GL.TexImage2D(orientations[i], 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0,
                PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        }
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

        return Handle;
    }
    private int GetSkyBoxCube()
    {
        float[] vertices= {
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            1.0f, -1.0f, -1.0f,
            1.0f, -1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            1.0f,  1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            -1.0f,  1.0f, -1.0f,
            1.0f,  1.0f, -1.0f,
            1.0f,  1.0f,  1.0f,
            1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
            1.0f, -1.0f, -1.0f,
            1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
            1.0f, -1.0f,  1.0f
        };
        int vertexBufferObj = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObj);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        int vertexArrayObj = GL.GenVertexArray();
        GL.BindVertexArray(vertexArrayObj);

        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

        return vertexArrayObj;
    }

    public void Draw(Matrix4 view, Matrix4 projection)
    {
        GL.DepthMask(false);

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
        shader.Use();

        shader.SetMatrix4("view", new Matrix4(new Matrix3(view)));
        shader.SetMatrix4("projection", projection);

        GL.BindVertexArray(cubeHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

        GL.DepthMask(true);
    }
}