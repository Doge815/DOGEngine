namespace DOGEngine.Shader;

public interface IShader
{
    public IShaderAttribute[] Attributes { get; }
    public int Handle { get; init; }
    public void Use();

    public int GetAttributeLocation(string attribute)
    {
        return GL.GetAttribLocation(Handle, attribute);
    }

    public void SetInt(string name, int data);
    public void SetSingle(string name, float data);
    public void SetMatrix4(string name, Matrix4 data);
}

public abstract class Shader : IShader
{
    protected Shader(string vertexPath, string fragmentPath)
    {
        int createShader(string source, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                throw new ArgumentException(GL.GetShaderInfoLog(shader));

            return shader;
        }

        int vertexShader = createShader(File.ReadAllText(vertexPath), ShaderType.VertexShader);
        int fragmentShader = createShader(File.ReadAllText(fragmentPath), ShaderType.FragmentShader);
        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);

        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
            Console.WriteLine(GL.GetProgramInfoLog(Handle));

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out int numberOfUniforms);

        uniformPos = new Dictionary<string, int>();

        for (int i = 0; i < numberOfUniforms; i++)
        {
            string? key = GL.GetActiveUniform(Handle, i, out _, out _);
            int position = GL.GetUniformLocation(Handle, key);
            uniformPos.Add(key, position);
        }
    }

    private Dictionary<string, int> uniformPos { get; }
    public abstract IShaderAttribute[] Attributes { get; }  
    public int Stride => Attributes.Sum(x => x.Size);
    public int Handle { get; init; }
    private int getUniformPos(string name) {
        if (uniformPos.TryGetValue(name, out int value)) return value; //value already found
        int position = GL.GetUniformLocation(Handle, name);
        uniformPos.Add(name, position);
        return position;
    }

    public void SetInt(string name, int data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(getUniformPos(name), data);
    }

    public void SetSingle(string name, float data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(getUniformPos(name), data);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(getUniformPos(name), true, ref data);
    }

    public void SetVector3(string name, Vector3 data)
    {
        GL.UseProgram(Handle);
        GL.Uniform3(getUniformPos(name), data);
    }

    public virtual void Use()
    {
        GL.UseProgram(Handle);
    }

    public int GetAttributeLocation(string attribute)
    {
        return GL.GetAttribLocation(Handle, attribute);
    }
}

public interface IShaderAttribute
{
    int Size { get; }
    int Offset { get; }
    string AttributeName { get; }
}

public sealed class VertexShaderAttribute : IShaderAttribute
{
    public int Size => 3;
    public int Offset { get; }
    public string AttributeName { get; }

    public VertexShaderAttribute(int offset, string attributeName)
    {
        Offset = offset;
        AttributeName = attributeName;
    }
}
public interface IVertexShader
{
    static abstract VertexShaderAttribute Vertex { get; }
}
public sealed class NormalShaderAttribute : IShaderAttribute
{
    public int Size => 3;
    public int Offset { get; }
    public string AttributeName { get; }

    public NormalShaderAttribute(int offset, string attributeName)
    {
        Offset = offset;
        AttributeName = attributeName;
    }
}
public interface INormalShader
{
    static abstract NormalShaderAttribute Normal { get; }
}
public sealed class Position2DShaderAttribute : IShaderAttribute
{
    public int Size => 2;
    public int Offset { get; }
    public string AttributeName { get; }

    public Position2DShaderAttribute(int offset, string attributeName)
    {
        Offset = offset;
        AttributeName = attributeName;
    }
}
public interface I2DPositionShader
{
    static abstract Position2DShaderAttribute Position2D { get; }
}
public sealed class TextureCoordShaderAttribute : IShaderAttribute
{
    public int Size => 2;
    public int Offset { get; }
    public string AttributeName { get; }

    public TextureCoordShaderAttribute(int offset, string attributeName)
    {
        Offset = offset;
        AttributeName = attributeName;
    }
}
public interface ITextureCoordShader
{
    static abstract TextureCoordShaderAttribute TextureCoord{ get; }
}

public interface IViewShader
{
    public void SetView(Matrix4 view);
}
public interface IModelShader
{
    public void SetModel(Matrix4 model);
}
public interface IProjectionShader
{
    public void SetProjection(Matrix4 projection);
}
public interface ICameraPosShader
{
    public void SetCameraPos(Vector3 cameraPos);
}