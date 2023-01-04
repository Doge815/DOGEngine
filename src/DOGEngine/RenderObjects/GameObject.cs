using System.Runtime.CompilerServices;
using DOGEngine.Shader;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace DOGEngine.RenderObjects;

public abstract class GameObject
{
    private readonly Dictionary<Type, GameObject?> children;
    public IReadOnlyCollection<GameObject?> Children => children.Values;

    public T GetComponent<T>() where T : GameObject
    {
        if (children.TryGetValue(typeof(T), out var gameObject))
            return Unsafe.As<T>(gameObject)?? throw new InvalidOperationException("Internal engine error");
        throw new ArgumentException("Can't find component");
    }

    public bool TryGetComponent<T>(out T? gameObject) where T : GameObject
    {
        if (children.TryGetValue(typeof(T), out GameObject? obj))
        {
            gameObject = Unsafe.As<T>(obj)?? throw new InvalidOperationException("Internal engine error");
            return true;
        }

        gameObject = null;
        return false;
    }

    public void AddComponent(GameObject? gameObject)
    {
        if (!children.TryAdd(gameObject.GetType(), gameObject)) throw new ArgumentException("Can't add component");
    }

    public bool TryAddComponent(GameObject? gameObject) => children.TryAdd(gameObject.GetType(), gameObject);
    
    public GameObject Parent { get; init; }

    protected GameObject(GameObject parent)
    {
        Parent = parent;
        children = new Dictionary<Type, GameObject?>();
    }
}

public class Scene : GameObject
{
    public Scene() : base(null!)
    {
        Parent = this;
        AddComponent(new GameObjectCollection(this));
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

    public void AddGameObject(GameObject gameObject) => collection.Add(gameObject);
}

public class Transform : GameObject
{
    public Vector3 Position { get; set; }
    public Vector3 Orientation { get; set; }
    public Vector3 Scale { get; set; }
    public Vector3 OrientationOffset { get; set; }

    public Transform(GameObject parent, Vector3? position, Vector3? orientation, Vector3? scale,
        Vector3? orientationOffset) : base(parent)
    {
        Position = position ?? Vector3.Zero;
        Orientation = orientation ?? Vector3.Zero;
        Scale = scale ?? Vector3.One;
        OrientationOffset = orientationOffset ?? Vector3.Zero;
    }
}

public class Mesh : GameObject
{
    public int VAO { get; }
    private readonly int triangles;

    public Mesh(GameObject parent, VertexDataBundle data) : base(parent)
    {
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        var vertices = data.CreateVertices(shader) ?? Array.Empty<float>();
        triangles = data.Rows;
        
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
    public Shading(GameObject parent, Shader.Shader shader) : base(parent)
    {
        Shader = shader;
    }
}