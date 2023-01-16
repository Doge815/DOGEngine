using DOGEngine.Shader;
using DOGEngine.Texture;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;
using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;

namespace DOGEngine.RenderObjects;
public readonly struct VertexDataBundle
{
    private Dictionary<Type, float[]> Data { get; }
    public int Rows { get; }
    public float[] CreateVertices(Shader.Shader shader)
    {
        int columns = 0;
        foreach (IShaderAttribute attribute in shader.Attributes)
            columns += attribute.Size;

        float[] verts = new float[columns * Rows];
        foreach (IShaderAttribute attribute in shader.Attributes)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j <  attribute.Size; j++)
                {
                    int writePosition = i * columns + j + attribute.Offset;
                    if (Data.TryGetValue(attribute.GetType(), out var attributeData))
                        verts[writePosition] = attributeData[i * attribute.Size + j];
                    else
                        verts[writePosition] = 0;
                }
            }
        }
        return verts;
    }

    public VertexDataBundle(Dictionary<Type, float[]> data, int rows)
    {
        Data = data;
        Rows = rows;
    }
}

public interface IPostInitializedGameObject
{
    protected bool NotInitialized { get; set; }

    internal sealed void Initialize()
    {
        if (NotInitialized) InitFunc();
        NotInitialized = false;
    }

    public void InitFunc();
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

    public virtual IEnumerable<GameObject> GetAllWithName(string name)
    {
        
        foreach (GameObject child in Children)
        foreach (var childComponent in child.GetAllWithName(name))
            yield return childComponent;

        if (TryGetComponent(out Name? objName) && objName!.ObjName == name) yield return this;
    }

    public void AddComponent(GameObject gameObject)
    {
        if (!children.TryAdd(gameObject.GetType(), gameObject)) throw new ArgumentException("Can't add component");
        gameObject.Parent = this;
        if(gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
    }

    public bool TryAddComponent(GameObject gameObject)
    {
        bool success = children.TryAdd(gameObject.GetType(), gameObject);
        if(success && gameObject is IPostInitializedGameObject initialize) initialize.Initialize();
        return success;
    }

    public GameObject Parent { get; internal set; }

    private GameObject()
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
            if (!children.TryAdd(child.GetType(), child)) throw new ArgumentException("Can't add component");
            child.Parent = this;
        }
        foreach (var child in newChildren)
        {
            if(child is IPostInitializedGameObject initialize)
                initialize.Initialize();
        }
    }
}

public class GameObjectCollection : GameObject
{
    private readonly List<GameObject> collection;
    public IReadOnlyCollection<GameObject> Collection => collection.AsReadOnly();
    
    public GameObjectCollection(params GameObject[] newChildren)
    {
        collection = new List<GameObject>();
        CollectionAddComponents(newChildren);
    }

    public void CollectionAddComponent(GameObject child)
    {
        collection.Add(child);
        child.Parent = this;
        if(child is IPostInitializedGameObject initialize)
            initialize.Initialize();
    }

    public void CollectionAddComponents(params GameObject[] children)
    {
        foreach (var child in children)
        {
            collection.Add(child);
            child.Parent = this;
        }
        foreach (var child in children)
            if(child is IPostInitializedGameObject initialize)
                initialize.Initialize();
    }


    public override IEnumerable<T> GetAllInChildren<T>()
    {
        foreach (var child in base.GetAllInChildren<T>())
            yield return child;

        foreach (GameObject child in collection)
        foreach (T childComponent in child.GetAllInChildren<T>())
            yield return childComponent;
    }
    public override IEnumerable<GameObject> GetAllWithName(string name)
    {
        foreach (var child in base.GetAllWithName(name))
            yield return child;

        foreach (GameObject child in collection)
        foreach (GameObject childComponent in child.GetAllWithName(name))
            yield return childComponent;
    }
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

public partial class Mesh : GameObject, IPostInitializedGameObject
{
    private int VAO; 
    private int triangles;

    private readonly VertexDataBundle tempData;
    public Mesh(VertexDataBundle data)
    {
        tempData = data;
        VAO = -1;
    }

    public bool NotInitialized { get; set; } = true;

    public void InitFunc()
    {
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        var vertices = tempData.CreateVertices(shader);
        triangles = tempData.Rows;
        
        int vertexBufferObj = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObj);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        VAO = GL.GenVertexArray();
        GL.BindVertexArray(VAO);

        foreach (IShaderAttribute attribute in shader.Attributes)
            interpretVertexDataFloat(shader, attribute, shader.Stride);
    }

    public void Draw(Matrix4 view, Matrix4 projection, Vector3 cameraPosition)
    {
        if(VAO == -1) return;
        Shader.Shader shader = Parent.GetComponent<Shading>().Shader;
        shader.Use();

        if (shader is IModelShader modelShader)
        {
            (Vector3 Position, Vector3 Orientation, Vector3 Scale, Vector3 OrientationOffset)  = GetTransformData();
            Matrix4 model = Matrix4.CreateScale(Scale)
                            * Matrix4.CreateTranslation(OrientationOffset)
                            * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Orientation.X))
                            * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Orientation.Y))
                            * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(Orientation.Z))
                            * Matrix4.CreateTranslation(-OrientationOffset)
                            * Matrix4.CreateTranslation(Position);
            modelShader.SetModel(model);
        }
        if(shader is IViewShader viewShader)
            viewShader.SetView(view);
        if(shader is IProjectionShader projectionShader)
            projectionShader.SetProjection(projection);
        if(shader is ICameraPosShader cameraPosShader)
            cameraPosShader.SetCameraPos(cameraPosition);

        GL.BindVertexArray(VAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, triangles);
    }

    private (Vector3 Position, Vector3 Orientation, Vector3 Scale, Vector3 OrientationOffset) GetTransformData()
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
    public GameObjSkybox(string directory)
    {
        textureHandle = GetCubeMap(new []
        {
            directory + "/right.jpg",
            directory + "/left.jpg",
            directory + "/top.jpg",
            directory + "/bottom.jpg",
            directory + "/front.jpg",
            directory + "/back.jpg",
        });
        shader = new CubeMapShader();
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

public class Name : GameObject
{
    public string ObjName { get; }

    public Name(string objName) => ObjName = objName;
}

public class Lightning : GameObject
{
    public Vector3? Color { get; set; }
    public Vector3? Position { get; set; }

    public Vector3 GetColor
    {
        get => Color ?? parentColor() ?? Vector3.Zero;
        set => Color = value;
    }
    public Vector3 GetPosition
    {
        get => Position ?? parentPosition() ?? Vector3.Zero;
        set => Position = value;
    }

    public Lightning(Vector3? color = null, Vector3? position = null)
    {
        Color = color;
        Position = position;
    }

    private Vector3? parentColor()  
    {
        if(Parent.TryGetComponent<Shading>(out var shading) && shading!.Shader is PlainColorShader plainColorShader)
            return plainColorShader.Color * 255;
        return null;
    }
    private Vector3? parentPosition()  
    {
        if(Parent.TryGetComponent<Transform>(out var transform))
            return transform!.Position;
        return null;
    }
}

public enum Corner
{
    TopLeft,
    BottomLeft,
    TopRight,
    BottomRight
}
public class RenderText : GameObject
{
    private static TextShader? _textShader = null;
    private static TextShader Shader {get {
        if (_textShader is not null) return _textShader!;
        _textShader = new TextShader();
        return _textShader!;
    }}
    public Vector3 Color { get; set; }
    public string Text { get; set; }
    public Font Font { get; set; }
    public float Scale { get; set; }
    public Vector2 Position { get; set; }
    public Corner RelativePosition { get; set; }

    public RenderText(Font font, string? text = null, Vector2? position = null, Corner? relativePosition = null, Vector3? color = null,
        float? scale = null)
    {
        Font = font;
        Text = text ?? string.Empty;
        Position = position ?? Vector2.Zero;
        RelativePosition = relativePosition ?? Corner.BottomLeft;
        Color = color ?? Vector3.Zero;
        Scale = scale ?? 1;
    }
    
    public void Draw(int width, int height)
    {
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1);
        Shader.UseTextShader(null, Color, projection);
        RenderCharacterProvider.BindBuffer();

        int offset = 0;
        foreach (char c in Text)
        {

            RenderCharacter ch = RenderCharacterProvider.GetRenderCharacter(Font, c);
            float x = Position.X + offset + ch.Bearing.X * Scale;
            float y = Position.Y - (ch.Size.Y - ch.Bearing.Y) * Scale;
            float w = ch.Size.X * Scale;
            float h = ch.Size.Y * Scale;

            if (RelativePosition == Corner.TopLeft) y += height;
            if (RelativePosition == Corner.BottomRight) x += width;
            if (RelativePosition == Corner.TopRight)
            {
                y += height;
                x += width;
            }
            
            float[] pos = new float[] {
                x, y + h, 0, 0,
                x, y, 0, 1,
                x + w, y, 1, 1,
                x, y + h, 0, 0,
                x + w, y, 1, 1,
                x + w, y + h, 1, 0
            };
            Shader.UseTextShader(ch);
            GL.BufferSubData(BufferTarget.ArrayBuffer, 0, sizeof(float)*pos.Length, pos);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            offset += (int)(ch.Offset * Scale);
        }
    }
}

internal static class RenderCharacterProvider
{
    private static int? textVAO = null;
    private static int? textVBO = null;
    private static Dictionary<Font, Dictionary<char, RenderCharacter>> fonts { get; set; } =
        new Dictionary<Font, Dictionary<char, RenderCharacter>>();
    internal static RenderCharacter GetRenderCharacter(Font font, char character)
    {
        if (fonts.TryGetValue(font, out var fontFamily))
        {
            if (fontFamily.TryGetValue(character, out var renderCharacter)) return renderCharacter;
            throw new AggregateException(nameof(character) + " out of range");
        }
        else
        {
            createFont(font);
            return GetRenderCharacter(font, character);
        }
    }

    private static void createFont(Font font)
    {
        var characters = new Dictionary<char, RenderCharacter>();
        FreeTypeSharp.FreeTypeLibrary lib = new FreeTypeSharp.FreeTypeLibrary();
        FT_Error error;

        error = FT_New_Face(lib.Native, font.FontFile, 0, out var face);
        if(error != 0) throw new AggregateException(error.ToString());
        FreeTypeSharp.FreeTypeFaceFacade faceWrapper = new FreeTypeSharp.FreeTypeFaceFacade(lib, face);
        
        error = FT_Set_Pixel_Sizes(faceWrapper.Face, 0, font.Height);
        if(error != 0) throw new AggregateException(error.ToString());
        
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);

        RenderCharacter createCharacter(char c){
            error = FT_Load_Char(faceWrapper.Face, Convert.ToUInt32(c), FT_LOAD_RENDER);
            if(error != 0) throw new AggregateException(error.ToString());

            GL.GenTextures(1, out int texture);
            GL.BindTexture(TextureTarget.Texture2D, texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.CompressedRed, (int)faceWrapper.GlyphBitmap.width, (int)faceWrapper.GlyphBitmap.rows, 0, PixelFormat.Red, PixelType.UnsignedByte, faceWrapper.GlyphBitmap.buffer);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            return new RenderCharacter( texture, new Vector2(faceWrapper.GlyphBitmap.width, faceWrapper.GlyphBitmap.rows), new Vector2(faceWrapper.GlyphBitmapLeft, faceWrapper.GlyphBitmapTop),  (uint) faceWrapper.GlyphMetricHorizontalAdvance);
        }
        
        for (byte b = 0; b < 255; b++)
        {
            characters.Add((char)b, createCharacter(((char)b)));
        }
        FT_Done_Face(faceWrapper.Face);
        FT_Done_FreeType(lib.Native);
        
        fonts.Add(font, characters);
    }

    internal static void BindBuffer()
    {
        if (textVAO is null || textVBO is null)
        {
            GL.GenVertexArrays(1, out int vao);
            GL.GenBuffers(1, out int vbo);

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 24, 0, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 4* sizeof(float), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 4* sizeof(float), 2 * sizeof(float));
            GL.BindVertexArray(0);

            textVAO = vao;
            textVBO = vbo;
        }
        GL.BindVertexArray(textVAO.Value);
        GL.BindBuffer(BufferTarget.ArrayBuffer, textVBO.Value);
    }
}