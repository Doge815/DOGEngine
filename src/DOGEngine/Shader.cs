using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace DOGEngine;

public interface IShader
{
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
                throw new Exception(GL.GetShaderInfoLog(shader));

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
    public int Handle { get; init; }

    public void SetInt(string name, int data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(uniformPos[name], data);
    }

    public void SetSingle(string name, float data)
    {
        GL.UseProgram(Handle);
        GL.Uniform1(uniformPos[name], data);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
        GL.UseProgram(Handle);
        GL.UniformMatrix4(uniformPos[name], true, ref data);
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

public struct ShaderAttribute
{
    public string AttributeName { get; init; }
    public int Offset { get; init; }
    public int Size { get; init; }
}